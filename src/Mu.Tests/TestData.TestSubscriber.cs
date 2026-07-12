namespace Mu.Testing;

using Mu.Communications.Messaging;
using Mu.Communications.PubSub;

public static partial class TestData
{
    public sealed class TestSubscriber
        : ISubscriber
    {
        public event ISubscriber.EventReceivedHandler? Received;

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public Task Raise(Event @event, CancellationToken cancellationToken)
        {
            return Received?.Invoke(this, @event, cancellationToken) ?? Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;

            return Task.CompletedTask;
        }
    }
}