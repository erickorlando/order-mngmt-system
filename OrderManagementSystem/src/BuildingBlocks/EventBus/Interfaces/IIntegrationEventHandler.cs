using EventBus.Events;

namespace EventBus.Interfaces;

public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
{
    Task HandleAsync(T @event);
}