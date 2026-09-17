namespace GiftStore.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string message) : base(message) { }
}

public class PriceChangedException : DomainException
{
    public decimal OriginalPrice { get; }
    public decimal CurrentPrice { get; }

    public PriceChangedException(decimal originalPrice, decimal currentPrice)
        : base($"Price has changed from {originalPrice:N0} to {currentPrice:N0} Tomans.")
    {
        OriginalPrice = originalPrice;
        CurrentPrice = currentPrice;
    }
}

public class FulfillmentException : DomainException
{
    public bool RequiresManualReview { get; }

    public FulfillmentException(string message, bool requiresManualReview = false)
        : base(message)
    {
        RequiresManualReview = requiresManualReview;
    }
}
