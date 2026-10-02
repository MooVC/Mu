namespace Muify.Semantics.CompilationExtensionTests;

using Mu.Modelling;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    public async Task GivenMissingServicesThenAllServiceVisitorsAreEnabled()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register;
            """;

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasService).IsFalse();
        _ = await Assert.That(result.HasServiceContract).IsFalse();
        _ = await Assert.That(result.HasGrpcClient).IsFalse();
        _ = await Assert.That(result.HasGrpcService).IsFalse();
        _ = await Assert.That(result.HasGrpcServiceContract).IsFalse();
    }

    [Test]
    public async Task GivenExistingServicesThenTheirVisitorsAreDisabled()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register;

            public partial interface IRegisterService
            {
                public interface IGrpc;
            }

            public sealed partial class RegisterService
            {
                public static class Grpc
                {
                    public sealed class Client;

                    public sealed class Service;
                }
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasService).IsTrue();
        _ = await Assert.That(result.HasServiceContract).IsTrue();
        _ = await Assert.That(result.HasGrpcClient).IsTrue();
        _ = await Assert.That(result.HasGrpcService).IsTrue();
        _ = await Assert.That(result.HasGrpcServiceContract).IsTrue();
    }

    [Test]
    public async Task GivenOnlyServiceContractsThenImplementationsRemainMissing()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register;

            public partial interface IRegisterService
            {
                public interface IGrpc;
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasService).IsFalse();
        _ = await Assert.That(result.HasServiceContract).IsTrue();
        _ = await Assert.That(result.HasGrpcClient).IsFalse();
        _ = await Assert.That(result.HasGrpcService).IsFalse();
        _ = await Assert.That(result.HasGrpcServiceContract).IsTrue();
    }

    [Test]
    public async Task GivenCustomHandlersAndServicesThenOnlyMatchingImplementationsAreSelected()
    {
        // Arrange
        const string source = """
            namespace Mu.Communications.Mediation
            {
                public interface IHandler<TRequest, TResult>;
            }

            namespace Mu.Modelling.Services
            {
                public interface IService<TRequest, TResult>;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed partial record Register;

                public sealed class OtherHandler : Mu.Communications.Mediation.IHandler<object, int>;

                public sealed class OtherService : Mu.Modelling.Services.IService<object, int>;
            }

            namespace MooVC.Testing.Mechanics.Car.Register.Custom
            {
                public sealed class Handler : Mu.Communications.Mediation.IHandler<Register, int>;

                public sealed class Service : Mu.Modelling.Services.IService<Register, int>;
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(source).Metadata;

        // Assert
        _ = await Assert.That(result.Handler.Moniker.ToString()).IsEqualTo("Handler");
        _ = await Assert.That(result.Handler.Qualifier.ToString()).IsEqualTo($"{AssemblyName}.Custom");
        _ = await Assert.That(result.Service.Moniker.ToString()).IsEqualTo("Service");
        _ = await Assert.That(result.Service.Qualifier.ToString()).IsEqualTo($"{AssemblyName}.Custom");
    }

    [Test]
    public async Task GivenAReferenceThenItsOwnPropertiesAreCatalogued()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register(Details Details);

            public sealed partial record Details(string Value);
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single();

        // Assert
        _ = await Assert.That(result.Attributes.Select(attribute => attribute.Name.ToString())).IsEquivalentTo(["Value"]);
    }

    [Test]
    public async Task GivenAReferenceWithABinderThenTheExistingBinderIsDetected()
    {
        // Arrange
        const string source = """
            namespace Mu.Serialization
            {
                public interface IBinder;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed partial record Register(Details Details);

                public sealed partial record Details(string Value) : Mu.Serialization.IBinder;
            }
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single();

        // Assert
        _ = await Assert.That(result.HasBinder).IsTrue();
    }
}