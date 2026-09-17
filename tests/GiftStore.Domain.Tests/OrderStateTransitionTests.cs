namespace GiftStore.Domain.Tests;

using FluentAssertions;
using Xunit;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Exceptions;

public class OrderStateTransitionTests
{
    [Fact]
    public void Order_WhenCreated_ShouldStartInDraftState()
    {
        var order = new Order("user-1", "user@test.local", null);
        order.Status.Should().Be(OrderStatus.Draft);
        order.OrderNumber.Should().StartWith("GIC-");
    }

    [Fact]
    public void MarkAwaitingPayment_FromDraft_ShouldSucceed()
    {
        var order = new Order("user-1", "user@test.local", null);
        order.MarkAwaitingPayment();
        order.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    [Fact]
    public void InvalidTransition_FromDraftToFulfilled_ShouldThrowException()
    {
        var order = new Order("user-1", "user@test.local", null);
        var act = () => order.TransitionTo(OrderStatus.Fulfilled);
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void CompleteValidOrderLifecycle_ShouldTransitionSmoothly()
    {
        var order = new Order("user-1", "user@test.local", null);
        order.MarkAwaitingPayment();
        order.TransitionTo(OrderStatus.PaymentPendingVerification);
        order.MarkPaymentConfirmed("PAY-123", 100000m);
        order.Status.Should().Be(OrderStatus.PaymentConfirmed);

        order.TransitionTo(OrderStatus.FulfillmentPending);
        order.TransitionTo(OrderStatus.SupplierOrderCreated);
        order.TransitionTo(OrderStatus.SupplierOrderConfirmationPending);
        order.TransitionTo(OrderStatus.SupplierOrderRetrievalPending);
        order.MarkFulfilled("ORD-999", 1);

        order.Status.Should().Be(OrderStatus.Fulfilled);
    }
}
