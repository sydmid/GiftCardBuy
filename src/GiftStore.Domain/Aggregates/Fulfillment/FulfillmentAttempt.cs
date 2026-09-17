namespace GiftStore.Domain.Aggregates.Fulfillment;

using GiftStore.Contracts.Enums;
using GiftStore.Domain.Common;

public class FulfillmentAttempt : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public int AttemptNumber { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public string? SupplierOrderReference { get; private set; } // ORD-xxxx
    public string? ErrorMessage { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private FulfillmentAttempt() { }

    public FulfillmentAttempt(Guid orderId, string idempotencyKey, int attemptNumber)
    {
        OrderId = orderId;
        IdempotencyKey = idempotencyKey;
        AttemptNumber = attemptNumber;
        Status = FulfillmentStatus.Pending;
    }

    public void SetSupplierOrderReference(string reference)
    {
        SupplierOrderReference = reference;
        Status = FulfillmentStatus.SupplierRequested;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkSupplierConfirmed()
    {
        Status = FulfillmentStatus.SupplierConfirmed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCodesRetrieved()
    {
        Status = FulfillmentStatus.CodesRetrieved;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string error, bool requiresManualReview)
    {
        ErrorMessage = error;
        Status = requiresManualReview ? FulfillmentStatus.ManualReview : FulfillmentStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
