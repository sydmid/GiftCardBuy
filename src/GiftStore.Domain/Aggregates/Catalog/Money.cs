namespace GiftStore.Domain.Aggregates.Catalog;

public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Toman(decimal amount) => new(amount, "IRT");
    public static Money Usd(decimal amount) => new(amount, "USD");
    public static Money ZeroToman => new(0m, "IRT");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int count) => new(Amount * count, Currency);
    public Money Multiply(decimal factor) => new(Math.Round(Amount * factor, 0, MidpointRounding.AwayFromZero), Currency);

    public override string ToString() => $"{Amount:N0} {Currency}";
}
