namespace Mu.Communications.PubSub;

using Mu.Communications.Messaging;

public interface IPolicyDirectory
{
    IPolicy? Find(Event @event, IServiceProvider provider);
}