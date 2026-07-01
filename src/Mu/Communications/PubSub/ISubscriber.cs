namespace Mu.Communications.PubSub;

using Microsoft.Extensions.Hosting;
using Mu.Communications.Messaging;

/// <summary>
/// Represents a hosted subscriber that receives published events.
/// </summary>
public interface ISubscriber
    : IHostedService
{
    public delegate Task EventReceivedHandler(ISubscriber sender, Event @event, CancellationToken cancellationToken);

    event EventReceivedHandler Received;
}