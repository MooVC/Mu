namespace Muify.Service.GenerateRegisterModeWhenFeatureVisitedTests;

using MooVC.Syntax.CSharp;
using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed partial class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAGuidCreationalFeatureWhenNoCustomAllocatorExistsThenItsServiceGetsAConditionalAllocator()
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

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::System.Guid, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.RegisterConditional<global::Mu.Modelling.Services.IAllocator<global::System.Guid>, global::Mu.Modelling.Services.GuidAllocator>(global::SimpleInjector.Lifestyle.Scoped, context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof(global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::System.Guid, global::MooVC.Testing.Mechanics.Car.Register.Register>));
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
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenACustomAllocatorThenNoDefaultAllocatorIsRegistered()
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

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::System.Guid, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
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
        Unit car = TestData.Single.Car._Value
            .IdentifiedBy((Qualification)typeof(Guid))
            .WithMetadata(metadata => metadata.WithAllocator(allocator => allocator
                .WithDefinition((Name: "Allocator", Qualifier: TestData.Single.Car.Namespace))));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Int64", "System")]
    [Arguments("Guid", "Custom")]
    public async Task GivenANonGuidIdentityThenNoDefaultAllocatorIsRegistered(string name, string qualifier)
    {
        // Arrange
        string expected = $$"""
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
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::{{qualifier}}.{{name}}>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::{{qualifier}}.{{name}}>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::{{qualifier}}.{{name}}>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::{{qualifier}}.{{name}}, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
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
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)(name, qualifier));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAComponentIdentityThenNoDefaultAllocatorIsRegistered()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(TestData.Single.Car._Value, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedRegisterMode);
    }

    [Test]
    public async Task GivenATransitionalFeatureThenNoDefaultAllocatorIsRegistered()
    {
        // Arrange
        const string expected = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Unregister;

            public static partial class ContainerExtensions
            {
                extension(global::SimpleInjector.Container container)
                {
                    public global::SimpleInjector.Container RegisterMode(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, configuration, nameof(configuration), message: "The source for configuration values must be provided.");

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Unregister.UnregisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Unregister.UnregisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Unregister.UnregisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Unregister.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Mechanics.Car.Unregister.UnregisterOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Unregister.Unregister, global::Mu.Modelling.State.Revision>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Unregister.Unregister, global::Mu.Modelling.State.Revision>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Unregister.Unregister, global::System.Guid>, global::Mu.Modelling.Services.TransitionalService<global::MooVC.Testing.Mechanics.Car.Car, global::System.Guid, global::MooVC.Testing.Mechanics.Car.Unregister.Unregister>>(global::SimpleInjector.Lifestyle.Scoped);
                            container.Collection.Register<global::Mu.Modelling.Integrity.IInvariant<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Unregister.Unregister>>(global::System.Array.Empty<global::System.Type>(), global::SimpleInjector.Lifestyle.Scoped);
                            container.Collection.Register<global::Mu.Modelling.Services.ITransform<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Unregister.Unregistered>>(new[] { typeof(global::MooVC.Testing.Mechanics.Car.Unregister.Transform) }, global::SimpleInjector.Lifestyle.Scoped);
                            container.Register<global::Mu.Modelling.Services.IRoot<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Unregister.Unregister>, global::Mu.Modelling.Services.Root<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Unregister.Unregistered, global::MooVC.Testing.Mechanics.Car.Unregister.Unregister>>(global::SimpleInjector.Lifestyle.Scoped);
                            _ = global::MooVC.Testing.Mechanics.Car.Unregister.ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                        }
                        else if (options.Mode == global::Mu.Configuration.ModeType.Remote)
                        {
                            _ = global::MooVC.Testing.Mechanics.Car.Unregister.ContainerExtensions.RegisterIpcClient(container, options.Remote);
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
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Unregister._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAQueryFeatureThenNoDefaultAllocatorIsRegistered()
    {
        // Arrange
        var visitor = new GenerateRegisterModeWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.FindCarsBy._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(ExpectedFindCarsByMode);
    }

    [Test]
    public async Task GivenACustomServiceThenNoDefaultAllocatorIsRegistered()
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

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
                            container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::System.Guid>>(global::SimpleInjector.Lifestyle.Scoped);
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
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Feature request = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .WithService((Name: "Service", Qualifier: TestData.Single.Register.Namespace)));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, request);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenACustomHandlerThenNoDefaultAllocatorIsRegistered()
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

                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions>(configuration.GetSection(nameof(global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions))) ?? new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        return global::MooVC.Testing.Mechanics.Car.Register.ContainerExtensions.RegisterMode(container, options);
                    }

                    public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, container, nameof(container), message: "The container must be provided.");
                        options ??= new global::MooVC.Testing.Mechanics.Car.Register.RegisterOptions();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted)
                        {
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
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Feature request = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .WithHandler((Name: "Handler", Qualifier: TestData.Single.Register.Namespace)));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, request);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
    }

    private static Model.Graph.Areas.Area.Units.Unit.Features.Feature DefineAllocatorFeature(Unit car, Feature request)
    {
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);
        Model.Graph.Areas.Area.Units.Unit.Features features = new(unit, model, car.Features);
        Feature definition = request.WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasRegistrar(false));

        return new(features, 0, model, definition);
    }
}