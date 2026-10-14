namespace Mu.Persistence.Composition.ContainerExtensionsTests;

using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;
using Mu.Modelling.State;
using Mu.Persistence.Configuration;
using Mu.Testing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenRegisterWriteStoreIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenMissingOptionsThenRegistersATransientWriteStoreAndReturnsContainer(bool explicitlyNull)
    {
        // Arrange
        using var container = new Container();
        container.RegisterInstance(Substitute.For<IStream<Guid>>());
        container.RegisterInstance(Substitute.For<ITransform<TestData.TestAggregate>>());

        // Act
        Container result = explicitlyNull
            ? container.RegisterWriteStore<TestData.TestAggregate, Guid>(options: default)
            : container.RegisterWriteStore<TestData.TestAggregate, Guid>();
        IWriteStore<TestData.TestAggregate, Guid> first = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();
        IWriteStore<TestData.TestAggregate, Guid> second = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(first).IsTypeOf<WriteStore<TestData.TestAggregate, Guid>>();
        _ = await Assert.That(ReferenceEquals(first, second)).IsFalse();
    }

    [Test]
    public async Task GivenDefaultDependenciesWhenFactsAreSavedThenAnotherScopeCanRehydrateTheAggregate()
    {
        // Arrange
        const ulong firstRevision = 1;
        using Container container = CreateContainer();
        container.Collection.Register<ITransform<TestData.TestAggregate, TestData.TestFact>>([typeof(TestData.TestTransform)], Lifestyle.Scoped);
        TestData.TestAggregate aggregate = TestData.CreateAggregateWithChanges(TestData.CreateRevision(number: 0), new TestData.TestFact());
        IStream<Guid> firstStream;
        ITransform<TestData.TestAggregate> firstTransform;
        ITransform<TestData.TestAggregate> repeatedTransform;
        IWriteStore<TestData.TestAggregate, Guid> firstStore;
        IWriteStore<TestData.TestAggregate, Guid> repeatedStore;

        // Act
        _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>();
        _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>();
        bool wasLocked = container.IsLocked;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            firstStore = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();
            repeatedStore = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();
            firstStream = container.GetInstance<IStream<Guid>>();
            firstTransform = container.GetInstance<ITransform<TestData.TestAggregate>>();
            repeatedTransform = container.GetInstance<ITransform<TestData.TestAggregate>>();
            await firstStore.Save(aggregate, TestData.Identity, CancellationToken.None);
        }

        using DependencyScope next = AsyncScopedLifestyle.BeginScope(container);
        IWriteStore<TestData.TestAggregate, Guid> secondStore = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();
        IStream<Guid> secondStream = container.GetInstance<IStream<Guid>>();
        ITransform<TestData.TestAggregate> secondTransform = container.GetInstance<ITransform<TestData.TestAggregate>>();
        TestData.TestAggregate? restored = await secondStore.Get(TestData.Identity, firstRevision, CancellationToken.None);

        // Assert
        _ = await Assert.That(wasLocked).IsFalse();
        _ = await Assert.That(firstStore).IsTypeOf<WriteStore<TestData.TestAggregate, Guid>>();
        _ = await Assert.That(ReferenceEquals(firstStore, repeatedStore)).IsFalse();
        _ = await Assert.That(ReferenceEquals(firstStore, secondStore)).IsFalse();
        _ = await Assert.That(firstStream).IsTypeOf<InMemoryStream<Guid>>();
        _ = await Assert.That(secondStream).IsSameReferenceAs(firstStream);
        _ = await Assert.That(firstTransform).IsTypeOf<ReflectionTransform<TestData.TestAggregate>>();
        _ = await Assert.That(repeatedTransform).IsSameReferenceAs(firstTransform);
        _ = await Assert.That(ReferenceEquals(firstTransform, secondTransform)).IsFalse();
        _ = await Assert.That(restored).IsNotNull();
        _ = await Assert.That(restored!.Value).IsEqualTo(TestData.DefaultValue + TestData.FactValue);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenCustomDependenciesBeforeOrAfterDefaultsThenTheyArePreserved(bool registerAfterDefaults)
    {
        // Arrange
        using Container container = CreateContainer();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        ITransform<TestData.TestAggregate> transform = Substitute.For<ITransform<TestData.TestAggregate>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();

        if (!registerAfterDefaults)
        {
            container.RegisterInstance(stream);
            container.RegisterInstance(transform);
            container.RegisterInstance(store);
        }

        // Act
        _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>();

        if (registerAfterDefaults)
        {
            container.RegisterInstance(stream);
            container.RegisterInstance(transform);
            container.RegisterInstance(store);
        }

        IStream<Guid> actualStream = container.GetInstance<IStream<Guid>>();
        ITransform<TestData.TestAggregate> actualTransform = container.GetInstance<ITransform<TestData.TestAggregate>>();
        IWriteStore<TestData.TestAggregate, Guid> actualStore = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();

        // Assert
        _ = await Assert.That(actualStream).IsSameReferenceAs(stream);
        _ = await Assert.That(actualTransform).IsSameReferenceAs(transform);
        _ = await Assert.That(actualStore).IsSameReferenceAs(store);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenOpenGenericCustomDependenciesWhenTheyAreConditionalOrUnconditionalThenTheyArePreserved(bool conditional)
    {
        // Arrange
        using Container container = CreateContainer();

        if (conditional)
        {
            container.RegisterConditional(typeof(IStream<>), typeof(CustomStream<>), Lifestyle.Singleton, context => !context.Handled);
            container.RegisterConditional(typeof(ITransform<>), typeof(CustomTransform<>), Lifestyle.Scoped, context => !context.Handled);
            container.RegisterConditional(typeof(IWriteStore<,>), typeof(CustomWriteStore<,>), Lifestyle.Transient, context => !context.Handled);
        }
        else
        {
            container.Register(typeof(IStream<>), typeof(CustomStream<>), Lifestyle.Singleton);
            container.Register(typeof(ITransform<>), typeof(CustomTransform<>), Lifestyle.Scoped);
            container.Register(typeof(IWriteStore<,>), typeof(CustomWriteStore<,>), Lifestyle.Transient);
        }

        // Act
        _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>();
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IStream<Guid> stream = container.GetInstance<IStream<Guid>>();
        ITransform<TestData.TestAggregate> transform = container.GetInstance<ITransform<TestData.TestAggregate>>();
        IWriteStore<TestData.TestAggregate, Guid> store = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();

        // Assert
        _ = await Assert.That(stream).IsTypeOf<CustomStream<Guid>>();
        _ = await Assert.That(transform).IsTypeOf<CustomTransform<TestData.TestAggregate>>();
        _ = await Assert.That(store).IsTypeOf<CustomWriteStore<TestData.TestAggregate, Guid>>();
    }

    [Test]
    public async Task GivenExistingUnregisteredTypeResolutionThenItsCustomStoreIsPreserved()
    {
        // Arrange
        using Container container = CreateContainer();
        IWriteStore<TestData.TestAggregate, Guid> expected = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();
        Registration registration = Lifestyle.Singleton.CreateRegistration(() => expected, container);
        container.ResolveUnregisteredType += (_, arguments) =>
        {
            if (arguments.UnregisteredServiceType == typeof(IWriteStore<TestData.TestAggregate, Guid>))
            {
                arguments.Register(registration);
            }
        };

        // Act
        _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>();
        IWriteStore<TestData.TestAggregate, Guid> actual = container.GetInstance<IWriteStore<TestData.TestAggregate, Guid>>();

        // Assert
        _ = await Assert.That(actual).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task GivenNullConfigurationThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The source for configuration values must be provided.";
        IConfiguration configuration = default!;
        using var container = new Container();

        // Act
        Exception? exception = Capture(() => _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(configuration));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenNullContainerWhenConfigurationIsPresentOrMissingThenThrowsArgumentNullExceptionForContainer(bool hasConfiguration)
    {
        // Arrange
        const string expectedMessage = "The container used to register the write store must be provided.";
        Container container = default!;
        IConfiguration configuration = hasConfiguration ? new ConfigurationBuilder().Build() : default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(container));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenNullContainerWhenOptionsArePresentOrMissingThenThrowsArgumentNullExceptionForContainer(bool hasOptions)
    {
        // Arrange
        const string expectedMessage = "The container used to register the write store must be provided.";
        Container container = default!;
        WriteStoreOptions? options = hasOptions ? new WriteStoreOptions() : default;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterWriteStore<TestData.TestAggregate, Guid>(options: options));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(container));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }

    private static Container CreateContainer()
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.RegisterInstance<IServiceProvider>(container);

        return container;
    }

    public sealed class CustomStream<TIdentity> : IStream<TIdentity>
        where TIdentity : struct
    {
        private readonly InMemoryStream<TIdentity> _stream = new();

        public Task<DateTimeOffset> Append(IEnumerable<Fact> facts, TIdentity identity, Revision revision, CancellationToken cancellationToken)
        {
            return _stream.Append(facts, identity, revision, cancellationToken);
        }

        public Task<ImmutableArray<Event>> Find(IStream<TIdentity>.FindOptions options, CancellationToken cancellationToken)
        {
            return _stream.Find(options, cancellationToken);
        }

        public Task<DateTimeOffset> Initiate(IEnumerable<Fact> facts, TIdentity identity, CancellationToken cancellationToken)
        {
            return _stream.Initiate(facts, identity, cancellationToken);
        }
    }

    public sealed class CustomTransform<TAggregate> : ITransform<TAggregate>
        where TAggregate : Aggregate
    {
        public TAggregate Apply(TAggregate aggregate, Fact fact)
        {
            return aggregate;
        }
    }

    public sealed class CustomWriteStore<TAggregate, TIdentity> : IWriteStore<TAggregate, TIdentity>
        where TAggregate : Aggregate
        where TIdentity : struct
    {
        public Task<TAggregate?> Get(TIdentity identity, ulong revision, CancellationToken cancellationToken)
        {
            return Task.FromResult<TAggregate?>(default);
        }

        public Task Save(TAggregate aggregate, TIdentity identity, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}