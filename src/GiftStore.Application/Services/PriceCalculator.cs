namespace GiftStore.Application.Services;

using GiftStore.Application.Interfaces;
using GiftStore.Domain.Aggregates.Catalog;

public class PriceCalculator : IPriceCalculator
{
    public decimal CalculateUnitSellingPrice(SupplierCatalogVariant variant, ProductVariantMapping mapping, PriceRule? rule)
    {
        return mapping.GetCurrentSellingPrice(variant, rule);
    }
}
