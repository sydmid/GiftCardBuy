namespace GiftStore.Domain.Aggregates.Orders;

using GiftStore.Domain.Common;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductVariantMappingId { get; private set; }
    public string SupplierVariantId { get; private set; } = string.Empty;
    public string ProductTitleFa { get; private set; } = string.Empty;
    public string VariantTitleFa { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public PriceSnapshot PriceSnapshot { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(
        Guid productVariantMappingId,
        string supplierVariantId,
        string productTitleFa,
        string variantTitleFa,
        int quantity,
        PriceSnapshot priceSnapshot)
    {
        ProductVariantMappingId = productVariantMappingId;
        SupplierVariantId = supplierVariantId;
        ProductTitleFa = productTitleFa;
        VariantTitleFa = variantTitleFa;
        Quantity = quantity;
        PriceSnapshot = priceSnapshot;
    }
}
