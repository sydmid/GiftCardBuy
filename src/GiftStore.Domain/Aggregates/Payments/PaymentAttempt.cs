namespace GiftStore.Domain.Aggregates.Payments;

using GiftStore.Contracts.Enums;
using GiftStore.Domain.Common;

public class PaymentAttempt : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string GatewayProvider { get; private set; } = string.Empty; // e.g. "Fake", "Zarinpal"
    public string TransactionReference { get; private set; } = string.Empty;
    public string? GatewayTransactionId { get; private set; }
    public decimal AmountToman { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayResponseRaw { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }

    private PaymentAttempt() { }

    public PaymentAttempt(Guid orderId, string gatewayProvider, string transactionReference, decimal amountToman)
    {
        OrderId = orderId;
        GatewayProvider = gatewayProvider;
        TransactionReference = transactionReference;
        AmountToman = amountToman;
        Status = PaymentStatus.Initiated;
    }

    public void MarkPending()
    {
        Status = PaymentStatus.Pending;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkSuccess(string gatewayTxId, string rawResponse)
    {
        GatewayTransactionId = gatewayTxId;
        GatewayResponseRaw = rawResponse;
        Status = PaymentStatus.Success;
        VerifiedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage, string? rawResponse = null)
    {
        ErrorMessage = errorMessage;
        GatewayResponseRaw = rawResponse;
        Status = PaymentStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
