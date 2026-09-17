namespace GiftStore.Domain.Aggregates.Orders;

public record PriceSnapshot(
    decimal UnitSupplierCostToman,
    decimal UnitSellingPriceToman,
    decimal TotalSellingPriceToman,
    string Currency = "IRT"
);
