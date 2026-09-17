namespace GiftStore.Application.Tests;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using GiftStore.Application.Interfaces;
using GiftStore.Application.UseCases.Payments;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Aggregates.Payments;
using GiftStore.Payment.Interfaces;
using GiftStore.Payment.Models;

public class CheckoutAndPaymentHandlerTests
{
    [Fact]
    public async Task ProcessPaymentCallback_WhenAlreadyConfirmed_ShouldBeIdempotent()
    {
        var db = Substitute.For<IGiftStoreDbContext>();
        var gateway = Substitute.For<IPaymentGateway>();
        var outbox = Substitute.For<IOutboxService>();
        var logger = NullLogger<ProcessPaymentCallbackHandler>.Instance;

        var orderId = Guid.NewGuid();
        var order = new Order("user1", "test@test.local", null);
        order.MarkAwaitingPayment();
        order.TransitionTo(OrderStatus.PaymentPendingVerification);
        order.MarkPaymentConfirmed("tx1", 100000m);

        db.Orders.Returns(new[] { order }.AsQueryable());

        var handler = new ProcessPaymentCallbackHandler(db, gateway, outbox, logger);
        var res = await handler.HandleAsync(new ProcessPaymentCallbackCommand(order.Id, "AUTH-123", "OK"));

        res.IsSuccess.Should().BeTrue();
        res.ReferenceNumber.Should().Be("ALREADY_VERIFIED");
        await gateway.DidNotReceive().VerifyPaymentAsync(Arg.Any<PaymentVerificationRequest>());
    }
}
