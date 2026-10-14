namespace Muify.Service.GenerateRegisterModeWhenFeatureVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed partial class WhenObserveIsCalled
{
    private const string ExpectedRegisterMode = """
        #nullable enable

        namespace MooVC.Testing.Mechanics.Car.Register;

        public static partial class ContainerExtensions
        {
            extension(global::SimpleInjector.Container container)
            {
                public global::SimpleInjector.Container RegisterMode(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                    var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                    return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                }

                public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                    options ??= new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                    if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                    {
                        container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>>(global::SimpleInjector.Lifestyle.Scoped);
                        container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Registration, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                        container.Collection.Register<global::Mu.Modelling.Integrity.IInvariant<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::System.Array.Empty<global::System.Type>(), global::SimpleInjector.Lifestyle.Scoped);
                        container.Collection.Register<global::Mu.Modelling.Services.ITransform<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Registered>>(new[] { typeof(global::MooVC.Testing.Mechanics.Car.Register.Transform) }, global::SimpleInjector.Lifestyle.Scoped);
                        container.Register<global::Mu.Modelling.Services.IRoot<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Register>, global::Mu.Modelling.Services.Root<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Registered, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                        _ = global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                    }
                    else if (options.Mode == global::Mu.Configuration.ModeType.Remote)
                    {
                        _ = global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcClient(container, options.Remote);
                    }
                    else
                    {
                        throw new global::System.NotSupportedException("The configured feature mode is not supported.");
                    }

                    return container;
                }
            }
        }
        """;

    private const string ExpectedFindCarsByMode = """
        #nullable enable

        namespace MooVC.Testing.Mechanics.Car.FindCarsBy;

        public static partial class ContainerExtensions
        {
            extension(global::SimpleInjector.Container container)
            {
                public global::SimpleInjector.Container RegisterMode(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                    var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsByOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsByOptions))) ?? new global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsByOptions();

                    return global::MooVC.Testing.Mechanics.Car.FindCarsBy.ContainerExtensions.RegisterMode(container, options);
                }

                public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                    options ??= new global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsByOptions();

                    if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                    {
                        container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsBy, global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsBy.Result>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsBy, global::MooVC.Testing.Mechanics.Car.FindCarsBy.FindCarsBy.Result>>(global::SimpleInjector.Lifestyle.Scoped);
                        _ = global::MooVC.Testing.Mechanics.Car.FindCarsBy.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                    }
                    else if (options.Mode == global::Mu.Configuration.ModeType.Remote)
                    {
                        _ = global::MooVC.Testing.Mechanics.Car.FindCarsBy.ContainerExtensions.RegisterIpcClient(container, options.Remote);
                    }
                    else
                    {
                        throw new global::System.NotSupportedException("The configured feature mode is not supported.");
                    }

                    return container;
                }
            }
        }
        """;

    [Test]
    public async Task GivenAFeatureWhenOptionsAreMissingThenConfigurationAndOptionsOverloadsAreGenerated()
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode";
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasBase(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedRegisterMode);
    }

    [Test]
    public async Task GivenExistingOptionsThenTheirExactQualificationIsUsedForBindingAndDefaults()
    {
        // Arrange
        const string expected = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public static partial class ContainerExtensions
            {
                extension(global::SimpleInjector.Container container)
                {
                    public global::SimpleInjector.Container RegisterMode(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Configuration.CustomOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Configuration.CustomOptions))) ?? new global::MooVC.Testing.Configuration.CustomOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Configuration.CustomOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Registration, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Collection.Register<global::Mu.Modelling.Integrity.IInvariant<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::System.Array.Empty<global::System.Type>(), global::SimpleInjector.Lifestyle.Scoped);
                            container.Collection.Register<global::Mu.Modelling.Services.ITransform<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Registered>>(new[] { typeof(global::MooVC.Testing.Mechanics.Car.Register.Transform) }, global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IRoot<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Register>, global::Mu.Modelling.Services.Root<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Registered, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                            _ = global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                        }
                        else if (options.Mode == global::Mu.Configuration.ModeType.Remote)
                        {
                            _ = global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterIpcClient(container, options.Remote);
                        }
                        else
                        {
                            throw new global::System.NotSupportedException("The configured feature mode is not supported.");
                        }

                        return container;
                    }
                }
            }
            """;
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
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
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenPartialRecordOptionsWhenNoBaseIsDefinedThenModeRegistrationIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Poco options = Poco.Undefined
            .IsPartial(true)
            .WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true))
            .WithQualification((Name: "RegisterOptions", Qualifier: TestData.Single.Register.Namespace));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedRegisterMode);
    }

    [Test]
    [Arguments(false, false, true, true)]
    [Arguments(true, true, true, true)]
    [Arguments(true, false, false, true)]
    [Arguments(true, false, true, false)]
    public async Task GivenIncompatibleOptionsThenNothingIsGenerated(bool isPartial, bool hasBase, bool isClass, bool isRecord)
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(hasBase)
            .IsPartial(isPartial)
            .WithCharacteristics(characteristics => characteristics.IsClass(isClass).IsRecord(isRecord))
            .WithQualification((Name: "RegisterOptions", Qualifier: TestData.Single.Register.Namespace));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(false, true)]
    [Arguments(true, false)]
    [Arguments(true, true)]
    public async Task GivenAFeatureWhenRegistrarOrPartialStatusVariesThenModeRegistrationIsGenerated(bool hasRegistrar, bool isPartial)
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasBase(false)
            .HasRegistrar(hasRegistrar)
            .IsPartial(isPartial));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedRegisterMode);
    }

    [Test]
    public async Task GivenInaccessibleOptionsThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(true)
            .HasOptionsBase(true)
            .IsAccessible(false)
            .WithCharacteristics(characteristics => characteristics.IsClass(true).IsRecord(true))
            .WithQualification((Name: "PrivateOptions", Qualifier: TestData.Single.Register.Namespace));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenModeRegistrationExistsThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasModeRegistration(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenANonMutationalFeatureThenNoRootIsRegistered()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Feature query = TestData.Single.FindCarsBy._Value.WithMetadata(metadata => metadata.HasBase(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 0, TestData.Single.Model, query);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedFindCarsByMode);
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}