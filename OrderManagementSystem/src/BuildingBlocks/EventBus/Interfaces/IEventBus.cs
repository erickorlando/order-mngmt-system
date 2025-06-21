using EventBus.Events;

namespace EventBus.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IntegrationEvent;
    Task SubscribeAsync<T, THandler>() 
        where T : IntegrationEvent 
        where THandler : class, IIntegrationEventHandler<T>;
}