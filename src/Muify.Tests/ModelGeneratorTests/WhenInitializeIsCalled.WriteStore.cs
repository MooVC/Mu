namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mu.Composition;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Persistence.Configuration;
using Mu.Serialization;
using ProtoBuf.Meta;
using SimpleInjector;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    public async Task GivenAGuidUnitThenItsRegistrarRegistersATransientWriteStoreFromConfiguration()
    {
        // Arrange
        const int expectedConfigurationRequests = 1;
        const string unitAssemblyName = "MooVC.Testing.Mechanics.VehicleWriteStore";
        const string unitTypeName = $"{unitAssemblyName}.VehicleWriteStore";
        const string source = """
            namespace Muify.Domain
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class UnitAttribute<TIdentity> : global::System.Attribute
                    where TIdentity : struct;
            }

            namespace MooVC.Testing.Mechanics.VehicleWriteStore
            {
                [global::Muify.Domain.Unit<global::System.Guid>]
                public sealed partial record VehicleWriteStore : global::Mu.Modelling.State.Aggregate;

                public static class ContainerExtensions
                {
                    public static global::SimpleInjector.Container RegisterWriteStore<TAggregate, TIdentity>(
                        this global::SimpleInjector.Container container,
                        global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                        where TAggregate : global::Mu.Modelling.State.Aggregate, new()
                        where TIdentity : struct
                        => throw new global::System.NotSupportedException();
                }
            }
            """;
        Assembly assembly = await CompileRegistrar(source, unitAssemblyName);
        Type aggregate = assembly.GetType(unitTypeName, throwOnError: true)!;
        Type contract = typeof(IWriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        Type implementation = typeof(WriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        Type transform = typeof(ITransform<>).MakeGenericType(aggregate);
        IConfiguration configuration = Substitute.For<IConfiguration>();
        IConfigurationSection options = new ConfigurationBuilder().Build().GetSection(nameof(WriteStoreOptions));
        _ = configuration.GetSection(nameof(WriteStoreOptions)).Returns(options);
        using var container = new Container();
        container.Options.EnableAutoVerification = false;
        container.RegisterInstance(Substitute.For<IStream<Guid>>());
        container.RegisterInstance(transform, Substitute.For([transform], []));
        MethodInfo registrar = aggregate.GetMethod(nameof(IRegistrar.Register))!;
        _ = RuntimeTypeModel.Default.AddMu();

        // Act
        _ = registrar.Invoke(default, [configuration, container]);
        object first = container.GetInstance(contract);
        object second = container.GetInstance(contract);
        InstanceProducer registration = container.GetRegistration(contract, throwOnFailure: true)!;

        // Assert
        _ = await Assert.That(first.GetType()).IsEqualTo(implementation);
        _ = await Assert.That(second.GetType()).IsEqualTo(implementation);
        _ = await Assert.That(ReferenceEquals(first, second)).IsFalse();
        _ = await Assert.That(registration.Lifestyle).IsSameReferenceAs(Lifestyle.Transient);
        _ = configuration.Received(expectedConfigurationRequests).GetSection(nameof(WriteStoreOptions));
    }
}