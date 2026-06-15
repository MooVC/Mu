namespace Mu.Communications.Tracing.ScopeTests;

using Serilog.Core;
using Serilog.Events;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenLedgerThenLedgerIsAddedToLogContext()
    {
        // Arrange
        var sink = new TestSink();
        using Logger logger = new Serilog.LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var ledger = new Ledger(Guid.NewGuid(), Guid.NewGuid());

        // Act
        using (new Scope(ledger))
        {
            logger.Information("Inside scope.");
        }

        logger.Information("Outside scope.");

        // Assert
        StructureValue value = (StructureValue)sink.Events[0].Properties[nameof(Ledger)];
        _ = await Assert.That(sink.Events.Length).IsEqualTo(2);
        _ = await Assert.That(GetProperty<Guid>(value, nameof(Ledger.Causation))).IsEqualTo(ledger.Causation);
        _ = await Assert.That(GetProperty<Guid>(value, nameof(Ledger.Correlation))).IsEqualTo(ledger.Correlation);
        _ = await Assert.That(sink.Events[1].Properties.ContainsKey(nameof(Ledger))).IsFalse();
    }

    private static T GetProperty<T>(StructureValue structure, string name)
    {
        LogEventPropertyValue value = structure.Properties.Single(property => property.Name == name).Value;

        return (T)((ScalarValue)value).Value!;
    }

    private sealed class TestSink
        : ILogEventSink
    {
        private readonly List<LogEvent> _events = [];

        public LogEvent[] Events => [.. _events];

        public void Emit(LogEvent logEvent)
        {
            _events.Add(logEvent);
        }
    }
}