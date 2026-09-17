namespace GiftStore.Domain.Aggregates.Orders.Events;

using GiftStore.Domain.Common;

public record OrderCreatedEvent(Guid OrderId, string OrderNumber, string CustomerUserId, decimal TotalAmountToman) : DomainEvent;
public record OrderPaymentConfirmedEvent(Guid OrderId, string PaymentAttemptId, decimal AmountPaidToman) : DomainEvent;
public record OrderFulfilledEvent(Guid OrderId, string SupplierReference, int DeliveredCodesCount) : DomainEvent;
public record OrderFulfillmentFailedEvent(Guid OrderId, string Reason, bool RequiresManualReview) : DomainEvent;
