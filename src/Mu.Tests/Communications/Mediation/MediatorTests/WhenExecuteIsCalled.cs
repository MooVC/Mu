namespace Mu.Communications.Mediation.MediatorTests;

using Microsoft.Extensions.Logging;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public sealed class WhenExecuteIsCalled
{
    private const string ResultValue = "Result";

    [Test]
    public async Task GivenHandlerSucceedsThenRequestAndSuccessfulOutcomeAreLogged()
    {
        // Arrange
        var logger = new TestLogger();
        IHandler<TestUseCase, string> handler = Substitute.For<IHandler<TestUseCase, string>>();
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        Result<string> expected = ResultValue;
        var useCase = new TestUseCase();

        _ = provider
            .GetService(typeof(IHandler<TestUseCase, string>))
            .Returns(handler);

        _ = handler
            .Handle(Arg.Any<Intent<TestUseCase>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(((Intent<TestUseCase>)callInfo[0]).Yields(expected)));

        var subject = new Mediator(logger, provider);

        // Act
        Result<string> result = await subject.Execute<TestUseCase, string>(useCase, CancellationToken.None);

        // Assert
        LogEntry[] entries = [.. logger.Entries];
        _ = await Assert.That(result.Value).IsEqualTo(ResultValue);
        _ = await Assert.That(entries.Length).IsEqualTo(2);
        await AssertEntry(entries[0], LogLevel.Information, 1, useCase);
        await AssertEntry(entries[1], LogLevel.Information, 2, useCase);
    }

    [Test]
    public async Task GivenHandlerThrowsThenRequestAndExceptionAreLogged()
    {
        // Arrange
        var exception = new InvalidOperationException();
        var logger = new TestLogger();
        IHandler<TestUseCase, string> handler = Substitute.For<IHandler<TestUseCase, string>>();
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        var useCase = new TestUseCase();

        _ = provider
            .GetService(typeof(IHandler<TestUseCase, string>))
            .Returns(handler);

        _ = handler
            .Handle(Arg.Any<Intent<TestUseCase>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Outcome<string>>(exception));

        var subject = new Mediator(logger, provider);

        // Act
        Exception? thrown = null;

        try
        {
            _ = await subject.Execute<TestUseCase, string>(useCase, CancellationToken.None);
        }
        catch (Exception error)
        {
            thrown = error;
        }

        // Assert
        LogEntry[] entries = [.. logger.Entries];
        _ = await Assert.That(thrown).IsSameReferenceAs(exception);
        _ = await Assert.That(entries.Length).IsEqualTo(2);
        await AssertEntry(entries[0], LogLevel.Information, 1, useCase);
        await AssertEntry(entries[1], LogLevel.Error, 3, useCase, exception);
    }

    private static async Task AssertEntry(LogEntry entry, LogLevel level, int eventId, TestUseCase useCase, Exception? exception = null)
    {
        _ = await Assert.That(entry.EventId.Id).IsEqualTo(eventId);
        _ = await Assert.That(entry.Exception).IsSameReferenceAs(exception);
        _ = await Assert.That(entry.Level).IsEqualTo(level);
        _ = await Assert.That(entry.Properties["UseCaseId"]).IsEqualTo(useCase.Identity);
        _ = await Assert.That(entry.Properties["UseCaseType"]).IsEqualTo(typeof(TestUseCase));
    }

    public sealed record TestAggregate
        : Aggregate;

    public sealed record TestUseCase
        : Query<TestAggregate>;

    private sealed record LogEntry(EventId EventId, Exception? Exception, LogLevel Level, IReadOnlyDictionary<string, object?> Properties);

    private sealed class TestLogger
        : ILogger<Mediator>
    {
        private readonly List<LogEntry> _entries = [];

        public IReadOnlyList<LogEntry> Entries => _entries;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            IEnumerable<KeyValuePair<string, object?>> values = state is IEnumerable<KeyValuePair<string, object?>> properties
                ? properties
                : throw new InvalidOperationException();

            _entries.Add(new(eventId, exception, logLevel, values.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
    }
}