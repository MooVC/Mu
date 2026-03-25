namespace Mu.Communications.PubSub;

using Microsoft.Extensions.Hosting;
using Mu.Communications.Messaging;

/// <summary>
/// Represents a hosted subscriber that receives published events.
/// </summary>
public interface ISubscriber
    : IHostedService
{
    event EventHandler<Event> Received;
}
