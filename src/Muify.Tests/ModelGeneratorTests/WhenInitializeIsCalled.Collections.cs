namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mu.Composition;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAMutationalFeatureWhenComponentsAreGeneratedOrCustomThenItsCollectionsAndRegistrarsAreApplied(bool hasCustomComponents)
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Open";
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
            namespace MooVC.Testing.Mechanics.Car.Open
            {
                using System.Collections.Generic;
                using System.ComponentModel.DataAnnotations;
                using System.Runtime.CompilerServices;
                using System.Threading;
                using System.Threading.Tasks;
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using Mu.Modelling.Integrity;
                using Mu.Modelling.Services;
                using SimpleInjector;

                public sealed class PlainTransform : ITransform<Car, Opened>
                {
                    public Car Apply(Car aggregate, Opened fact) => aggregate;
                }

                public abstract class InvariantBase : Invariant<Car, Open>
                {
                    protected override async IAsyncEnumerable<ValidationResult> PerformEnforce(
                        Car aggregate,
                        Open mutation,
                        [EnumeratorCancellation] CancellationToken cancellationToken)
                    {
                        await Task.CompletedTask;
                        yield break;
                    }
                }

                public sealed class PlainInvariant : InvariantBase;

                public sealed class InvariantDependency(IConfiguration configuration)
                {
                    public IConfiguration Configuration { get; } = configuration;
                }

                public static class Rules
                {
                    public sealed class Check(InvariantDependency dependency) : InvariantBase, IRegistrar
                    {
                        public InvariantDependency Dependency { get; } = dependency;

                        public static int Registrations { get; private set; }

                        public static void Register(IConfiguration configuration, Container container)
                        {
                            Registrations++;
                            container.RegisterInstance(new InvariantDependency(configuration));
                        }
                    }
                }
            }

            namespace MooVC.Testing.Mechanics.Car.Open.Custom
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using Mu.Modelling.Services;
                using SimpleInjector;

                public sealed class TransformDependency(IConfiguration configuration)
                {
                    public IConfiguration Configuration { get; } = configuration;
                }

                public sealed class CustomTransform(TransformDependency dependency) : ITransform<Car, Opened>, IRegistrar
                {
                    public TransformDependency Dependency { get; } = dependency;

                    public static int Registrations { get; private set; }

                    public static void Register(IConfiguration configuration, Container container)
                    {
                        Registrations++;
                        container.RegisterInstance(new TransformDependency(configuration));
                    }

                    public Car Apply(Car aggregate, Opened fact) => aggregate;
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(hasCustomComponents ? source + customSource : source, featureAssemblyName);
        Type aggregate = assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type request = assembly.GetType($"{featureAssemblyName}.Open", throwOnError: true)!;
        Type fact = assembly.GetType($"{featureAssemblyName}.Opened", throwOnError: true)!;
        Type invariantContract = typeof(IInvariant<,>).MakeGenericType(aggregate, request);
        Type transformContract = typeof(ITransform<,>).MakeGenericType(aggregate, fact);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;
        MethodInfo registrar = request.GetMethod(nameof(IRegistrar.Register))!;

        // Act
        _ = registrar.Invoke(default, [configuration, container]);
        object[] invariants;
        object[] repeatedInvariants;
        object[] repeatedTransforms;
        object[] transforms;
        using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            invariants = container.GetAllInstances(invariantContract).ToArray();
            transforms = container.GetAllInstances(transformContract).ToArray();

            repeatedInvariants = container.GetAllInstances(invariantContract).ToArray();
            repeatedTransforms = container.GetAllInstances(transformContract).ToArray();
        }

        // Assert
        string[] expectedInvariants = hasCustomComponents ? ["Check", "PlainInvariant"] : [];
        string[] expectedTransforms = hasCustomComponents ? ["CustomTransform", "PlainTransform"] : ["Transform"];
        _ = await Assert.That(repeatedInvariants).IsEquivalentTo(invariants);
        _ = await Assert.That(repeatedTransforms).IsEquivalentTo(transforms);
        _ = await Assert.That(invariants.Select(instance => instance.GetType().Name)).IsEquivalentTo(expectedInvariants);
        _ = await Assert.That(transforms.Select(instance => instance.GetType().Name)).IsEquivalentTo(expectedTransforms);

        using Scope nextScope = AsyncScopedLifestyle.BeginScope(container);
        _ = await Assert.That(container.GetAllInstances(invariantContract).Any(invariants.Contains)).IsFalse();
        _ = await Assert.That(container.GetAllInstances(transformContract).Any(transforms.Contains)).IsFalse();

        if (hasCustomComponents)
        {
            object check = invariants.Single(instance => instance.GetType().Name == "Check");
            object transform = transforms.Single(instance => instance.GetType().Name == "CustomTransform");

            foreach (object instance in new[] { check, transform })
            {
                Type implementation = instance.GetType();
                object dependency = implementation.GetProperty("Dependency")!.GetValue(instance)!;
                _ = await Assert.That(implementation.GetProperty("Registrations")!.GetValue(default)).IsEqualTo(expectedRegistrations);
                _ = await Assert.That(dependency.GetType().GetProperty("Configuration")!.GetValue(dependency)).IsSameReferenceAs(configuration);
            }
        }
    }
}