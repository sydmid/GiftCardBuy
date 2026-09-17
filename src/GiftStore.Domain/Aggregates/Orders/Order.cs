namespace GiftStore.Domain.Aggregates.Orders;

using GiftStore.Contracts.Enums;
using GiftStore.Domain.Common;
using GiftStore.Domain.Aggregates.Orders.Events;
using GiftStore.Domain.Exceptions;

public class Order : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerUserId { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string? CustomerPhoneNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmountToman { get; private set; }
    public string? StatusNote { get; private set; }
    public uint RowVersion { get; private set; } // Concurrency control

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(string customerUserId, string customerEmail, string? customerPhoneNumber)
    {
        CustomerUserId = customerUserId;
        CustomerEmail = customerEmail;
        CustomerPhoneNumber = customerPhoneNumber;
        OrderNumber = GenerateOrderNumber();
        Status = OrderStatus.Draft;
    }

    public static string GenerateOrderNumber()
    {
        var random = Random.Shared.Next(1000, 9999);
        return $"GIC-{DateTime.UtcNow:yyMMdd}-{random}";
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.AwaitingPayment)
            throw new InvalidOrderStateException($"Cannot modify items when order is in {Status} state.");

        _items.Add(item);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        TotalAmountToman = _items.Sum(i => i.PriceSnapshot.TotalSellingPriceToman);
    }

    public void MarkAwaitingPayment()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOrderStateException($"Cannot move to AwaitingPayment from {Status}.");

        Status = OrderStatus.AwaitingPayment;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new OrderCreatedEvent(Id, OrderNumber, CustomerUserId, TotalAmountToman));
    }

    public void TransitionTo(OrderStatus newStatus, string? note = null)
    {
        ValidateStateTransition(Status, newStatus);
        Status = newStatus;
        StatusNote = note;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkPaymentConfirmed(string paymentAttemptId, decimal amountPaid)
    {
        TransitionTo(OrderStatus.PaymentConfirmed, $"Payment confirmed via {paymentAttemptId}");
        AddDomainEvent(new OrderPaymentConfirmedEvent(Id, paymentAttemptId, amountPaid));
    }

    public void MarkFulfilled(string supplierRef, int codesCount)
    {
        TransitionTo(OrderStatus.Fulfilled, $"Successfully fulfilled with {codesCount} codes (Ref: {supplierRef})");
        AddDomainEvent(new OrderFulfilledEvent(Id, supplierRef, codesCount));
    }

    public void MarkFulfillmentFailed(string reason, bool manualReview)
    {
        var nextState = manualReview ? OrderStatus.ManualReviewRequired : OrderStatus.FulfillmentFailed;
        TransitionTo(nextState, reason);
        AddDomainEvent(new OrderFulfillmentFailedEvent(Id, reason, manualReview));
    }

    private static void ValidateStateTransition(OrderStatus current, OrderStatus next)
    {
        // Explicit valid transitions for robust state machine
        bool valid = (current, next) switch
        {
            (OrderStatus.Draft, OrderStatus.AwaitingPayment) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,

            (OrderStatus.AwaitingPayment, OrderStatus.PaymentPendingVerification) => true,
            (OrderStatus.AwaitingPayment, OrderStatus.PaymentExpired) => true,
            (OrderStatus.AwaitingPayment, OrderStatus.Cancelled) => true,

            (OrderStatus.PaymentPendingVerification, OrderStatus.PaymentConfirmed) => true,
            (OrderStatus.PaymentPendingVerification, OrderStatus.PaymentFailed) => true,

            (OrderStatus.PaymentConfirmed, OrderStatus.FulfillmentPending) => true,
            (OrderStatus.PaymentConfirmed, OrderStatus.RefundPending) => true,

            (OrderStatus.FulfillmentPending, OrderStatus.SupplierOrderCreated) => true,
            (OrderStatus.FulfillmentPending, OrderStatus.FulfillmentFailed) => true,
            (OrderStatus.FulfillmentPending, OrderStatus.ManualReviewRequired) => true,

            (OrderStatus.SupplierOrderCreated, OrderStatus.SupplierOrderConfirmationPending) => true,
            (OrderStatus.SupplierOrderCreated, OrderStatus.ManualReviewRequired) => true,

            (OrderStatus.SupplierOrderConfirmationPending, OrderStatus.SupplierOrderRetrievalPending) => true,
            (OrderStatus.SupplierOrderConfirmationPending, OrderStatus.ManualReviewRequired) => true,

            (OrderStatus.SupplierOrderRetrievalPending, OrderStatus.Fulfilled) => true,
            (OrderStatus.SupplierOrderRetrievalPending, OrderStatus.ManualReviewRequired) => true,
            (OrderStatus.SupplierOrderRetrievalPending, OrderStatus.FulfillmentFailed) => true,

            (OrderStatus.FulfillmentFailed, OrderStatus.FulfillmentPending) => true, // Retry
            (OrderStatus.FulfillmentFailed, OrderStatus.ManualReviewRequired) => true,
            (OrderStatus.FulfillmentFailed, OrderStatus.RefundPending) => true,

            (OrderStatus.ManualReviewRequired, OrderStatus.FulfillmentPending) => true, // Admin re-queued
            (OrderStatus.ManualReviewRequired, OrderStatus.Fulfilled) => true, // Admin manual resolution
            (OrderStatus.ManualReviewRequired, OrderStatus.RefundPending) => true,

            (OrderStatus.RefundPending, OrderStatus.Refunded) => true,

            _ => false
        };

        if (!valid)
        {
            throw new InvalidOrderStateException($"Invalid transition from {current} to {next}.");
        }
    }
}
