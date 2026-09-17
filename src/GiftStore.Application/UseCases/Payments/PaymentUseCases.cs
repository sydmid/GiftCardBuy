namespace GiftStore.Application.UseCases.Payments;

using Microsoft.Extensions.Logging;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Enums;
using GiftStore.Payment.Interfaces;
using GiftStore.Payment.Models;

public record ProcessPaymentCallbackCommand(
    Guid OrderId,
    string Authority,
    string? Status
);

public record PaymentCallbackResult(
    bool IsSuccess,
    Guid OrderId,
    string? ReferenceNumber,
    string? Message
);

public class ProcessPaymentCallbackHandler
{
    private readonly IGiftStoreDbContext _db;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOutboxService _outboxService;
    private readonly ILogger<ProcessPaymentCallbackHandler> _logger;

    public ProcessPaymentCallbackHandler(
        IGiftStoreDbContext db,
        IPaymentGateway paymentGateway,
        IOutboxService outboxService,
        ILogger<ProcessPaymentCallbackHandler> logger)
    {
        _db = db;
        _paymentGateway = paymentGateway;
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task<PaymentCallbackResult> HandleAsync(ProcessPaymentCallbackCommand command, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.Id == command.OrderId);
        if (order == null)
        {
            return new PaymentCallbackResult(false, command.OrderId, null, "سفارش مورد نظر یافت نشد.");
        }

        // Idempotency check: if order is already confirmed or fulfilled, return idempotent success
        if (order.Status >= OrderStatus.PaymentConfirmed && order.Status != OrderStatus.PaymentFailed)
        {
            _logger.LogInformation("Idempotent callback ignored for already paid order {OrderId}", order.Id);
            return new PaymentCallbackResult(true, order.Id, "ALREADY_VERIFIED", "پرداخت این سفارش قبلاً تایید شده است.");
        }

        var paymentAttempt = _db.PaymentAttempts
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefault(p => p.OrderId == order.Id);

        if (paymentAttempt == null)
        {
            return new PaymentCallbackResult(false, order.Id, null, "اطلاعات تراکنش پرداخت یافت نشد.");
        }

        var verificationRequest = new PaymentVerificationRequest(
            OrderId: order.Id,
            TransactionReference: command.Authority,
            ExpectedAmountToman: order.TotalAmountToman,
            Authority: command.Authority,
            StatusParam: command.Status
        );

        var verifyResult = await _paymentGateway.VerifyPaymentAsync(verificationRequest, cancellationToken);
        if (!verifyResult.IsSuccess)
        {
            paymentAttempt.MarkFailed(verifyResult.ErrorMessage ?? "خطای تایید پرداخت درگاه", verifyResult.RawResponse);
            order.TransitionTo(OrderStatus.PaymentFailed, verifyResult.ErrorMessage);
            await _db.SaveChangesAsync(cancellationToken);
            return new PaymentCallbackResult(false, order.Id, null, verifyResult.ErrorMessage ?? "پرداخت ناموفق بود.");
        }

        paymentAttempt.MarkSuccess(verifyResult.GatewayTransactionId, verifyResult.RawResponse);
        order.MarkPaymentConfirmed(paymentAttempt.Id.ToString(), order.TotalAmountToman);

        // Transactional outbox pattern: Schedule fulfillment domain event
        var outboxPayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            OrderId = order.Id,
            PaymentAttemptId = paymentAttempt.Id,
            ConfirmedAtUtc = DateTime.UtcNow
        });

        await _outboxService.EnqueueEventAsync("OrderPaymentConfirmed", outboxPayload, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment confirmed and outbox event enqueued for order {OrderId}, Ref: {Ref}", order.Id, verifyResult.ReferenceCode);
        return new PaymentCallbackResult(true, order.Id, verifyResult.ReferenceCode, "پرداخت با موفقیت تایید شد.");
    }
}
