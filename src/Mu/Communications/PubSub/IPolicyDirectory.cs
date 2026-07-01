namespace Mu.Communications.PubSub;

using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Messaging;

public interface IPolicyDirectory
{
    IPolicy? Find(Event @event, IServiceScope scope);
}