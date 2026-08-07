namespace Mu.Communications.PubSub;

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mu.Communications.Messaging;

/// <summary>
/// Represents an in-memory subscriber that aggregates multiple subscribers and handles received events.
/// </summary>
/// <param name="subscribers">The collection of subscribers.</param>
public sealed class InMemoryPolicyApplicator(IPolicyDirectory directory, IServiceScopeFactory factory, IEnumerable<ISubscriber> subscribers)
    : IHostedService
{
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (ISubscriber subscriber in subscribers)
        {
            subscriber.Received += OnReceived;

            await subscriber
                .StartAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (ISubscriber subscriber in subscribers)
        {
            subscriber.Received -= OnReceived;

            await subscriber
                .StopAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task OnReceived(ISubscriber sender, Event @event, CancellationToken cancellationToken)
    {
        using AsyncServiceScope scope = factory.CreateAsyncScope();

        IPolicy? policy = directory.Find(@event, scope.ServiceProvider);

        if (policy is null)
        {
            return;
        }

        await policy
            .Apply(@event, cancellationToken)
            .ConfigureAwait(false);
    }
}