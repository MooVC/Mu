namespace Mu.Testing;

using System.Collections.Concurrent;
using Mu.Communications.Messaging;
using Mu.Communications.PubSub;

public static partial class TestData
{
    public sealed class TestPolicy
        : IPolicy<TestFact, Guid>
    {
        private readonly ConcurrentBag<Event<TestFact, Guid>> _events = [];

        public IReadOnlyCollection<Event<TestFact, Guid>> Events => _events.ToArray();

        public Task Apply(Event<TestFact, Guid> @event, CancellationToken cancellationToken)
        {
            _events.Add(@event);

            return Task.CompletedTask;
        }
    }
}