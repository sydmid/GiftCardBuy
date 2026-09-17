namespace GiftStore.Contracts.Dtos;

using GiftStore.Contracts.Enums;

public record OrderFulfillmentDto(
    Guid OrderId,
    OrderStatus Status,
    string? SupplierOrderReference,
    int TotalCodesCount,
    DateTime? FulfilledAtUtc,
    string? ErrorMessage
);
