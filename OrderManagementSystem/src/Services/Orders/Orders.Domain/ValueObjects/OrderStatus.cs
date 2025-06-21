namespace Orders.Domain.ValueObjects;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}