namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mu.Composition;
using Mu.Modelling.Services;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAMutationalFeatureWhenItsRootIsDefaultOrCustomThenItsScopedRootCanBeResolved(bool hasCustomRoot)
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Open";
        const string configurationProperty = "Configuration";
        const string dependencyProperty = "Dependency";
        const string registrationsProperty = "Registrations";
        const int expectedRegistrations = 1;
        const string source = """
            namespace Muify.Domain
            {
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class UnitAttribute<TIdentity> : System.Attribute
                    where TIdentity : struct;
            }

            namespace Muify.Service
            {
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class CreationalAttribute<TFact> : System.Attribute;
            }

            namespace MooVC.Testing.Mechanics.Car
            {
                [Muify.Domain.Unit<System.Guid>]
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                [Muify.Service.Creational<Opened>]
                public sealed partial record Open;
            }
            """;
        const string customSource = """
            namespace MooVC.Testing.Mechanics.Car.Open.Custom
            {
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Extensions.Configuration;
                using Mu;
                using Mu.Composition;
                using Mu.Modelling.Services;
                using SimpleInjector;

                public sealed class RootDependency(IConfiguration configuration)
                {
                    public IConfiguration Configuration { get; } = configuration;
                }

                public sealed class CustomRoot(RootDependency dependency) : IRoot<Car, Open>, IRegistrar
                {
                    public RootDependency Dependency { get; } = dependency;

                    public static int Registrations { get; private set; }

                    public static void Register(IConfiguration configuration, Container container)
                    {
                        Registrations++;
                        container.RegisterInstance(new RootDependency(configuration));
                    }

                    public Task<Result<Car>> Apply(Car aggregate, Open mutation, CancellationToken cancellationToken)
                        => throw new NotSupportedException();
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(hasCustomRoot ? source + customSource : source, featureAssemblyName);
        Type aggregate = assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type request = assembly.GetType($"{featureAssemblyName}.Open", throwOnError: true)!;
        Type fact = assembly.GetType($"{featureAssemblyName}.Opened", throwOnError: true)!;
        Type contract = typeof(IRoot<,>).MakeGenericType(aggregate, request);
        Type expectedRoot = hasCustomRoot
            ? assembly.GetType($"{featureAssemblyName}.Custom.CustomRoot", throwOnError: true)!
            : typeof(Root<,,>).MakeGenericType(aggregate, fact, request);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;
        MethodInfo registrar = request.GetMethod(nameof(IRegistrar.Register))!;

        // Act
        _ = registrar.Invoke(default, [configuration, container]);
        object root;
        object repeated;
        using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            root = container.GetInstance(contract);
            repeated = container.GetInstance(contract);
        }

        object next;
        using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            next = container.GetInstance(contract);
        }

        // Assert
        _ = await Assert.That(root.GetType()).IsEqualTo(expectedRoot);
        _ = await Assert.That(repeated).IsSameReferenceAs(root);
        _ = await Assert.That(ReferenceEquals(next, root)).IsFalse();

        if (hasCustomRoot)
        {
            object dependency = expectedRoot.GetProperty(dependencyProperty)!.GetValue(root)!;
            _ = await Assert.That(expectedRoot.GetProperty(registrationsProperty)!.GetValue(default)).IsEqualTo(expectedRegistrations);
            _ = await Assert.That(dependency.GetType().GetProperty(configurationProperty)!.GetValue(dependency)).IsSameReferenceAs(configuration);
        }
    }
}