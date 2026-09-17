namespace GiftStore.Gifticard.Models;

using GiftStore.Contracts.Enums;

public record SupplierAccountStatus(
    bool IsConnected,
    string AccountName,
    decimal WalletBalanceToman,
    string Currency,
    DateTime CheckedAtUtc
);

public record SupplierProductVariant(
    string VariantId,
    string? ProductId,
    CardBrand Brand,
    string Title,
    string Sku,
    string Country,
    string Currency,
    decimal FaceValue,
    decimal PriceToman,
    bool InStock
);

public record PurchaseRequest(
    string SupplierVariantId,
    int Quantity,
    string LocalOrderId,
    string? CallbackUrl = null
);

public record PurchaseInitiationResult(
    bool IsSuccess,
    string? TrackingCode, // e.g. "ORD-12345"
    string? Status,       // "waiting_payment"
    decimal AmountToman,
    string? ErrorMessage = null
);

public record PurchaseConfirmationResult(
    bool IsSuccess,
    string TrackingCode,
    string Status,        // "processing"
    string? Message = null,
    string? ErrorMessage = null
);

public record PurchasedCardItem(
    string Code,
    string? Pin,
    string? SerialNumber,
    string? ExpirationDate
);

public record SupplierOrderResult(
    bool IsSuccess,
    string TrackingCode,
    string Status, // "completed", "processing", "failed"
    IReadOnlyList<PurchasedCardItem> Cards,
    string? ErrorMessage = null
);

public record SupplierPurchaseReference(
    string TrackingCode,
    string? SupplierOrderId = null
);
