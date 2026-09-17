namespace GiftStore.Contracts.Dtos;

public record SupplierVariantDto(
    string VariantId,
    string? ProductId,
    string Title,
    string Sku,
    string Country,
    string Currency,
    decimal FaceValue,
    decimal SupplierPrice,
    bool InStock,
    DateTime UpdatedAtUtc
);
