namespace Mu.Architecture.Communication.PubSub;

using Microsoft.Extensions.Hosting;
using Mu.Architecture.Messaging;

public interface ISubscriber
    : IHostedService
{
    event EventHandler<Event> Received;
}