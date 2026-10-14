namespace Muify.Service.GenerateRegisterIpcHostWhenFeatureVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenOptionsAreMissingThenConfigurationAndHostedGrpcOverloadsAreGenerated(bool hasOtherRegistrations)
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost";
        const string expected = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public static partial class ContainerExtensions
            {
                extension(global::SimpleInjector.Container container)
                {
                    public global::SimpleInjector.Container RegisterIpcHost(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(
                            configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                    }

                    public global::SimpleInjector.Container RegisterIpcHost(global::Mu.Configuration.Hosted? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::Mu.Configuration.Hosted();

                        if (options.Ipc != global::Mu.Configuration.IpcType.Grpc)
                        {
                            throw new global::System.NotSupportedException("The configured IPC transport is not supported.");
                        }

                        container.Register<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(global::SimpleInjector.Lifestyle.Scoped);
                        container.RegisterConditional(
                            typeof(global::Mu.Communications.Mediation.IMediator),
                            _ => typeof(global::Mu.Communications.Mediation.InMemoryMediator),
                            global::SimpleInjector.Lifestyle.Scoped,
                            context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof(global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service));

                        return global::Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions.RegisterGrpcHost<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(container, options.Grpc);
                    }
                }
            }
            """;

        var visitor = new GenerateRegisterIpcHostWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasBase(false)
            .HasIpcClientRegistration(hasOtherRegistrations)
            .HasModeRegistration(hasOtherRegistrations));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenExistingOptionsThenTheirExactQualificationIsUsedForConfigurationBinding()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost";
        const string expected = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public static partial class ContainerExtensions
            {
                extension(global::SimpleInjector.Container container)
                {
                    public global::SimpleInjector.Container RegisterIpcHost(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Configuration.CustomOptions>(
                            configuration.GetSection(nameof(global::MooVC.Testing.Configuration.CustomOptions))) ?? new global::MooVC.Testing.Configuration.CustomOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                    }

                    public global::SimpleInjector.Container RegisterIpcHost(global::Mu.Configuration.Hosted? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::Mu.Configuration.Hosted();

                        if (options.Ipc != global::Mu.Configuration.IpcType.Grpc)
                        {
                            throw new global::System.NotSupportedException("The configured IPC transport is not supported.");
                        }

                        container.Register<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(global::SimpleInjector.Lifestyle.Scoped);
                        container.RegisterConditional(
                            typeof(global::Mu.Communications.Mediation.IMediator),
                            _ => typeof(global::Mu.Communications.Mediation.InMemoryMediator),
                            global::SimpleInjector.Lifestyle.Scoped,
                            context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof(global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service));

                        return global::Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions.RegisterGrpcHost<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(container, options.Grpc);
                    }
                }
            }
            """;

        var visitor = new GenerateRegisterIpcHostWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(true)
            .HasOptionsBase(true)
            .WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true))
            .WithQualification((Name: "CustomOptions", Qualifier: "MooVC.Testing.Configuration"));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAFeatureWhenHostRegistrationExistsThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterIpcHostWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasIpcHostRegistration(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterIpcHostWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}