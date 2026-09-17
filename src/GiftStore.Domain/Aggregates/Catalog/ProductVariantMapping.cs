namespace GiftStore.Domain.Aggregates.Catalog;

using GiftStore.Domain.Common;

public class ProductVariantMapping : BaseEntity
{
    public Guid StoreProductId { get; private set; }
    public StoreProduct StoreProduct { get; private set; } = null!;

    public Guid SupplierCatalogVariantId { get; private set; }
    public SupplierCatalogVariant SupplierVariant { get; private set; } = null!;

    public string DisplayTitleFa { get; private set; } = string.Empty;
    public decimal OverrideSellingPriceToman { get; private set; }
    public bool UsePriceRule { get; private set; } = true;
    public Guid? PriceRuleId { get; private set; }
    public PriceRule? PriceRule { get; private set; }

    public bool IsAvailableForPurchase { get; private set; } = true;
    public int MinPurchaseQuantity { get; private set; } = 1;
    public int MaxPurchaseQuantity { get; private set; } = 5;

    private ProductVariantMapping() { }

    public ProductVariantMapping(
        Guid storeProductId,
        Guid supplierCatalogVariantId,
        string displayTitleFa,
        Guid? priceRuleId = null,
        decimal overrideSellingPriceToman = 0m,
        bool usePriceRule = true)
    {
        StoreProductId = storeProductId;
        SupplierCatalogVariantId = supplierCatalogVariantId;
        DisplayTitleFa = displayTitleFa;
        PriceRuleId = priceRuleId;
        OverrideSellingPriceToman = overrideSellingPriceToman;
        UsePriceRule = usePriceRule;
        IsAvailableForPurchase = true;
    }

    public decimal GetCurrentSellingPrice(SupplierCatalogVariant supplierVariant, PriceRule? rule)
    {
        if (!UsePriceRule && OverrideSellingPriceToman > 0)
        {
            return OverrideSellingPriceToman;
        }

        if (rule != null)
        {
            return rule.CalculateSellingPrice(supplierVariant.SupplierCostToman);
        }

        // Fallback default margin: 7% markup
        return Math.Ceiling((supplierVariant.SupplierCostToman * 1.07m) / 1000m) * 1000m;
    }

    public void UpdateSettings(string displayTitleFa, bool isAvailable, decimal overridePrice, bool useRule, Guid? ruleId, int minQty, int maxQty)
    {
        DisplayTitleFa = displayTitleFa;
        IsAvailableForPurchase = isAvailable;
        OverrideSellingPriceToman = overridePrice;
        UsePriceRule = useRule;
        PriceRuleId = ruleId;
        MinPurchaseQuantity = minQty;
        MaxPurchaseQuantity = maxQty;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
