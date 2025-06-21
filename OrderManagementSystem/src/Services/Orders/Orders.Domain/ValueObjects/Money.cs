namespace Orders.Domain.ValueObjects;

public record Money(decimal Amount)
{
    public static Money Zero => new(0);
    
    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);
    public static Money operator *(Money money, int multiplier) => new(money.Amount * multiplier);
    
    public override string ToString() => $"${Amount:F2}";
}