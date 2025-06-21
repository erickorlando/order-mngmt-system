namespace EventBus.Events;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
