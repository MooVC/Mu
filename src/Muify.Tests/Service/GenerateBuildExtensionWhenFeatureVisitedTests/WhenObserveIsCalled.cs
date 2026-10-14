namespace Muify.Service.GenerateBuildExtensionWhenFeatureVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    private const string ExpectedApplication = """
        #nullable enable

        namespace MooVC.Testing.Mechanics.Car.Register;

        public static partial class WebApplicationExtensions
        {
            extension(global::Microsoft.AspNetCore.Builder.WebApplication app)
            {
                public global::Microsoft.AspNetCore.Builder.WebApplication BuildRegister()
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, app, nameof(app), message: "The application must be provided.");

                    var container = global::Mu.Composition.WebApplicationExtensions.GetMuContainer(app) ?? throw new global::System.InvalidOperationException("Mu must be added before building features.");

                    var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(app.Configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                    if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                    {
                        _ = global::Mu.Composition.ContainerExtensions.RegisterOnce<global::MooVC.Testing.Mechanics.Car.Car>(container, app.Configuration);
                    }
                    _ = global::Mu.Composition.ContainerExtensions.RegisterOnce<global::MooVC.Testing.Mechanics.Car.Register.Register>(container, app.Configuration);
                    _ = global::Mu.Composition.ContainerExtensions.RegisterDefaults(container, app.Configuration);

                    if (options.Mode == global::Mu.Configuration.ModeType.Hosted && options.Hosted.Ipc == global::Mu.Configuration.IpcType.Grpc)
                    {
                        _ = global::Microsoft.AspNetCore.Builder.GrpcEndpointRouteBuilderExtensions.MapGrpcService<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(app);
                    }

                    return app;
                }
            }
        }
        """;

    private const string ExpectedApplicationHint = "MooVC.Testing.Mechanics.Car.Register.WebApplicationExtensions.BuildRegister";

    private const string ExpectedBuilder = """
        #nullable enable

        namespace MooVC.Testing.Mechanics.Car.Register;

        public static partial class WebApplicationBuilderExtensions
        {
            extension(global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
            {
                public global::Microsoft.AspNetCore.Builder.WebApplication BuildRegister()
                {
                    _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, builder, nameof(builder), message: "The application builder must be provided.");

                    var app = global::Mu.Composition.WebApplicationBuilderExtensions.BuildMu(builder);

                    try
                    {
                        return global::MooVC.Testing.Mechanics.Car.Register.WebApplicationExtensions.BuildRegister(app);
                    }
                    catch
                    {
                        app.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        throw;
                    }
                }
            }
        }
        """;

    private const string ExpectedBuilderHint = "MooVC.Testing.Mechanics.Car.Register.WebApplicationBuilderExtensions.BuildRegister";

    [Test]
    public async Task GivenAPartialFeatureWithoutARegistrarThenItsBuildExtensionComposesAndMapsTheFeature()
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasRegistrar(false)
            .IsPartial(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        File[] result = visitor.Observe(feature).ToArray();

        // Assert
        File application = await Assert.That(result.Where(file => file.Hint == ExpectedApplicationHint)).HasSingleItem();
        File builder = await Assert.That(result.Where(file => file.Hint == ExpectedBuilderHint)).HasSingleItem();
        _ = await Assert.That(result.Select(file => file.Hint)).IsEquivalentTo([ExpectedApplicationHint, ExpectedBuilderHint]);
        _ = await Assert.That(application.Content).IsEqualTo(ExpectedApplication);
        _ = await Assert.That(builder.Content).IsEqualTo(ExpectedBuilder);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenExistingOptionsWhenDefaultConstructionIsAvailableOrUnavailableThenTheirSectionAndSupportedFallbackAreUsed(bool hasParameterlessConstructor)
    {
        // Arrange
        const string customOptions = "global::MooVC.Testing.Configuration.CustomOptions";
        const string baseOptions = "global::Mu.Configuration.Options";
        string defaults = hasParameterlessConstructor ? customOptions : baseOptions;
        string expected = $$"""
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public static partial class WebApplicationExtensions
            {
                extension(global::Microsoft.AspNetCore.Builder.WebApplication app)
                {
                    public global::Microsoft.AspNetCore.Builder.WebApplication BuildRegister()
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, app, nameof(app), message: "The application must be provided.");

                        var container = global::Mu.Composition.WebApplicationExtensions.GetMuContainer(app) ?? throw new global::System.InvalidOperationException("Mu must be added before building features.");

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Configuration.CustomOptions>(app.Configuration.GetSection(nameof(global::MooVC.Testing.Configuration.CustomOptions))) ?? new {{defaults}}();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            _ = global::Mu.Composition.ContainerExtensions.RegisterOnce<global::MooVC.Testing.Mechanics.Car.Car>(container, app.Configuration);
                        }
                        _ = global::Mu.Composition.ContainerExtensions.RegisterOnce<global::MooVC.Testing.Mechanics.Car.Register.Register>(container, app.Configuration);
                        _ = global::Mu.Composition.ContainerExtensions.RegisterDefaults(container, app.Configuration);

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted && options.Hosted.Ipc == global::Mu.Configuration.IpcType.Grpc)
                        {
                            _ = global::Microsoft.AspNetCore.Builder.GrpcEndpointRouteBuilderExtensions.MapGrpcService<global::MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service>(app);
                        }

                        return app;
                    }
                }
            }
            """;

        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(true)
            .HasOptionsBase(true)
            .HasParameterlessConstructor(hasParameterlessConstructor)
            .WithQualification((Name: "CustomOptions", Qualifier: "MooVC.Testing.Configuration"));
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasBase(false)
            .WithOptions(options));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        File[] result = visitor.Observe(feature).ToArray();

        // Assert
        File application = await Assert.That(result.Where(file => file.Hint == ExpectedApplicationHint)).HasSingleItem();
        File builder = await Assert.That(result.Where(file => file.Hint == ExpectedBuilderHint)).HasSingleItem();
        _ = await Assert.That(result.Select(file => file.Hint)).IsEquivalentTo([ExpectedApplicationHint, ExpectedBuilderHint]);
        _ = await Assert.That(application.Content).IsEqualTo(expected);
        _ = await Assert.That(builder.Content).IsEqualTo(ExpectedBuilder);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAnExistingRegistrarWhenTheFeatureIsPartialOrNonPartialThenItsBuildExtensionIsGenerated(bool isPartial)
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasBase(false)
            .HasRegistrar(true)
            .IsPartial(isPartial));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        File[] result = visitor.Observe(feature).ToArray();

        // Assert
        File application = await Assert.That(result.Where(file => file.Hint == ExpectedApplicationHint)).HasSingleItem();
        File builder = await Assert.That(result.Where(file => file.Hint == ExpectedBuilderHint)).HasSingleItem();
        _ = await Assert.That(result.Select(file => file.Hint)).IsEquivalentTo([ExpectedApplicationHint, ExpectedBuilderHint]);
        _ = await Assert.That(application.Content).IsEqualTo(ExpectedApplication);
        _ = await Assert.That(builder.Content).IsEqualTo(ExpectedBuilder);
    }

    [Test]
    public async Task GivenANonPartialFeatureWithoutARegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasRegistrar(false)
            .IsPartial(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    [Arguments(false, false, true, true, true)]
    [Arguments(true, true, true, true, true)]
    [Arguments(true, false, false, true, true)]
    [Arguments(true, false, true, false, true)]
    [Arguments(true, false, true, true, false)]
    public async Task GivenIncompatibleOptionsThenNothingIsGenerated(bool isPartial, bool hasBase, bool isClass, bool isRecord, bool isAccessible)
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Poco options = Poco.Undefined
            .HasBase(hasBase)
            .IsAccessible(isAccessible)
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
    public async Task GivenAnExistingBuildExtensionThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasBuildExtension(true).HasBuilderBuildExtension(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAnOutOfScopeFeatureThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateBuildExtensionWhenFeatureVisited();

        // Act
        IEnumerable<File> result = visitor.Observe(TestData.Single.Register);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}