namespace GiftStore.Domain.Aggregates.Catalog;

using GiftStore.Contracts.Enums;
using GiftStore.Domain.Common;

public class SupplierCatalogVariant : BaseEntity
{
    public string SupplierVariantId { get; private set; } = string.Empty;
    public string? SupplierProductId { get; private set; }
    public CardBrand Brand { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty; // e.g. US, TR, AE, GB
    public string FaceCurrency { get; private set; } = string.Empty; // e.g. USD, TRY, EUR
    public decimal FaceValue { get; private set; }
    public decimal SupplierCostToman { get; private set; }
    public bool InStock { get; private set; }
    public DateTime LastSyncedAtUtc { get; private set; }

    private SupplierCatalogVariant() { }

    public SupplierCatalogVariant(
        string supplierVariantId,
        string? supplierProductId,
        CardBrand brand,
        string title,
        string sku,
        string countryCode,
        string faceCurrency,
        decimal faceValue,
        decimal supplierCostToman,
        bool inStock)
    {
        SupplierVariantId = supplierVariantId;
        SupplierProductId = supplierProductId;
        Brand = brand;
        Title = title;
        Sku = sku;
        CountryCode = countryCode;
        FaceCurrency = faceCurrency;
        FaceValue = faceValue;
        SupplierCostToman = supplierCostToman;
        InStock = inStock;
        LastSyncedAtUtc = DateTime.UtcNow;
    }

    public void UpdateFromSupplier(string title, decimal supplierCostToman, bool inStock)
    {
        Title = title;
        SupplierCostToman = supplierCostToman;
        InStock = inStock;
        LastSyncedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
