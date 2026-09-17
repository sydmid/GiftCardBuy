namespace GiftStore.Domain.Aggregates.Fulfillment;

using GiftStore.Domain.Common;

public class SupplierOrder : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string SupplierTrackingCode { get; private set; } = string.Empty; // e.g. ORD-12345
    public string SupplierVariantId { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal SupplierCost { get; private set; }
    public string Status { get; private set; } = "waiting_payment";
    public DateTime InitiatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? RetrievedAtUtc { get; private set; }

    private SupplierOrder() { }

    public SupplierOrder(Guid orderId, string supplierTrackingCode, string supplierVariantId, int quantity, decimal cost)
    {
        OrderId = orderId;
        SupplierTrackingCode = supplierTrackingCode;
        SupplierVariantId = supplierVariantId;
        Quantity = quantity;
        SupplierCost = cost;
        Status = "created";
        InitiatedAtUtc = DateTime.UtcNow;
    }

    public void MarkConfirmed()
    {
        Status = "processing";
        ConfirmedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkRetrieved()
    {
        Status = "completed";
        RetrievedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = "failed";
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
