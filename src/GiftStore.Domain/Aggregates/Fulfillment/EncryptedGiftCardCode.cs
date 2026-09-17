namespace GiftStore.Domain.Aggregates.Fulfillment;

using GiftStore.Domain.Common;

public class EncryptedGiftCardCode : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public string SupplierTrackingCode { get; private set; } = string.Empty;

    // Encrypted ciphertexts protected with ASP.NET Core Data Protection
    public string EncryptedCode { get; private set; } = string.Empty;
    public string? EncryptedPin { get; private set; }
    public string? SerialNumberMasked { get; private set; } // Masked e.g. "XXXX-XXXX-1234"
    public string? ExpirationDate { get; private set; }

    public bool IsRevealed { get; private set; }
    public DateTime? FirstRevealedAtUtc { get; private set; }
    public string? RevealedByUserId { get; private set; }

    private EncryptedGiftCardCode() { }

    public EncryptedGiftCardCode(
        Guid orderId,
        Guid orderItemId,
        string supplierTrackingCode,
        string encryptedCode,
        string? encryptedPin,
        string? serialNumberMasked,
        string? expirationDate)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        SupplierTrackingCode = supplierTrackingCode;
        EncryptedCode = encryptedCode;
        EncryptedPin = encryptedPin;
        SerialNumberMasked = serialNumberMasked;
        ExpirationDate = expirationDate;
        IsRevealed = false;
    }

    public void RecordReveal(string userId)
    {
        if (!IsRevealed)
        {
            IsRevealed = true;
            FirstRevealedAtUtc = DateTime.UtcNow;
            RevealedByUserId = userId;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
