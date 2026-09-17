namespace GiftStore.Application.UseCases.Fulfillment;

using Microsoft.Extensions.Logging;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Fulfillment;
using GiftStore.Domain.Aggregates.Audit;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Gifticard.Models;

public record ProcessFulfillmentCommand(Guid OrderId);

public class ProcessFulfillmentHandler
{
    private readonly IGiftStoreDbContext _db;
    private readonly IGiftCardSupplier _supplier;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ProcessFulfillmentHandler> _logger;

    public ProcessFulfillmentHandler(
        IGiftStoreDbContext db,
        IGiftCardSupplier supplier,
        IEncryptionService encryptionService,
        ILogger<ProcessFulfillmentHandler> logger)
    {
        _db = db;
        _supplier = supplier;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ProcessFulfillmentCommand command, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.Id == command.OrderId);
        if (order == null || order.Status != OrderStatus.PaymentConfirmed && order.Status != OrderStatus.FulfillmentPending)
        {
            _logger.LogWarning("Order {OrderId} is not in a valid status for fulfillment: {Status}", command.OrderId, order?.Status);
            return false;
        }

        var orderItem = _db.OrderItems.FirstOrDefault(i => i.OrderId == order.Id);
        if (orderItem == null)
        {
            order.MarkFulfillmentFailed("Order item missing", manualReview: true);
            await _db.SaveChangesAsync(cancellationToken);
            return false;
        }

        order.TransitionTo(OrderStatus.FulfillmentPending, "Starting 3-step Gift-i-Card fulfillment pipeline");

        var attempt = new FulfillmentAttempt(order.Id, Guid.NewGuid().ToString("N"), 1);
        await _db.AddAsync(attempt, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            // Step 1: Initiate Purchase at Gift-i-Card
            var buyReq = new PurchaseRequest(
                SupplierVariantId: orderItem.SupplierVariantId,
                Quantity: orderItem.Quantity,
                LocalOrderId: order.OrderNumber
            );

            var initResult = await _supplier.InitiatePurchaseAsync(buyReq, cancellationToken);
            if (!initResult.IsSuccess || string.IsNullOrWhiteSpace(initResult.TrackingCode))
            {
                attempt.MarkFailed(initResult.ErrorMessage ?? "InitiatePurchase failed", requiresManualReview: false);
                order.MarkFulfillmentFailed(initResult.ErrorMessage ?? "خطا در ثبت سفارش نزد تامین‌کننده", manualReview: false);
                await _db.SaveChangesAsync(cancellationToken);
                return false;
            }

            attempt.SetSupplierOrderReference(initResult.TrackingCode);
            order.TransitionTo(OrderStatus.SupplierOrderCreated, $"Supplier Order {initResult.TrackingCode} created");

            var supplierOrder = new SupplierOrder(
                orderId: order.Id,
                supplierTrackingCode: initResult.TrackingCode,
                supplierVariantId: orderItem.SupplierVariantId,
                quantity: orderItem.Quantity,
                cost: initResult.AmountToman
            );
            await _db.AddAsync(supplierOrder, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Step 2: Confirm Purchase at Gift-i-Card (deducts supplier wallet & initiates code processing)
            order.TransitionTo(OrderStatus.SupplierOrderConfirmationPending, "Confirming order with supplier");
            var confirmRef = new SupplierPurchaseReference(initResult.TrackingCode);
            var confirmResult = await _supplier.ConfirmPurchaseAsync(confirmRef, cancellationToken);

            if (!confirmResult.IsSuccess)
            {
                attempt.MarkFailed(confirmResult.ErrorMessage ?? "ConfirmPurchase failed", requiresManualReview: true);
                order.MarkFulfillmentFailed(confirmResult.ErrorMessage ?? "خطا در تایید سفارش نزد تامین‌کننده", manualReview: true);
                await _db.SaveChangesAsync(cancellationToken);
                return false;
            }

            attempt.MarkSupplierConfirmed();
            supplierOrder.MarkConfirmed();
            order.TransitionTo(OrderStatus.SupplierOrderRetrievalPending, "Awaiting digital card codes from supplier");
            await _db.SaveChangesAsync(cancellationToken);

            // Step 3: Retrieve Order & Digital Card Codes
            var retrieveResult = await _supplier.RetrieveOrderAsync(confirmRef, cancellationToken);
            if (!retrieveResult.IsSuccess || retrieveResult.Cards.Count == 0)
            {
                _logger.LogWarning("Codes retrieval pending or failed for tracking {TrackingCode}", initResult.TrackingCode);
                attempt.MarkFailed("Codes not ready or supplier error", requiresManualReview: true);
                order.MarkFulfillmentFailed("کدهای گیفت کارت در صف پردازش تامین‌کننده هستند یا خطایی رخ داده است.", manualReview: true);
                await _db.SaveChangesAsync(cancellationToken);
                return false;
            }

            // Step 4: Encrypt codes at rest using Data Protection
            foreach (var card in retrieveResult.Cards)
            {
                var encryptedCode = _encryptionService.Encrypt(card.Code);
                var encryptedPin = !string.IsNullOrWhiteSpace(card.Pin) ? _encryptionService.Encrypt(card.Pin) : null;
                var maskedSerial = !string.IsNullOrWhiteSpace(card.SerialNumber) && card.SerialNumber.Length > 4
                    ? $"***-{card.SerialNumber[^4..]}"
                    : card.SerialNumber;

                var encryptedRecord = new EncryptedGiftCardCode(
                    orderId: order.Id,
                    orderItemId: orderItem.Id,
                    supplierTrackingCode: initResult.TrackingCode,
                    encryptedCode: encryptedCode,
                    encryptedPin: encryptedPin,
                    serialNumberMasked: maskedSerial,
                    expirationDate: card.ExpirationDate
                );

                await _db.AddAsync(encryptedRecord, cancellationToken);
            }

            attempt.MarkCodesRetrieved();
            supplierOrder.MarkRetrieved();
            order.MarkFulfilled(initResult.TrackingCode, retrieveResult.Cards.Count);

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Order {OrderId} successfully fulfilled with {Count} codes!", order.Id, retrieveResult.Cards.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception during fulfillment of order {OrderId}", order.Id);
            attempt.MarkFailed(ex.Message, requiresManualReview: true);
            order.MarkFulfillmentFailed(ex.Message, manualReview: true);
            await _db.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}

public record RevealCardCodeCommand(Guid CodeRecordId, string RequestingUserId, string? UserEmail, string? IpAddress, bool IsStaff);

public record RevealCardCodeResult(
    bool IsSuccess,
    string DecryptedCode,
    string? DecryptedPin,
    string? MaskedSerial,
    string? ExpirationDate,
    string? ErrorMessage = null
);

public class RevealCardCodeHandler
{
    private readonly IGiftStoreDbContext _db;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<RevealCardCodeHandler> _logger;

    public RevealCardCodeHandler(IGiftStoreDbContext db, IEncryptionService encryptionService, ILogger<RevealCardCodeHandler> logger)
    {
        _db = db;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<RevealCardCodeResult> HandleAsync(RevealCardCodeCommand command, CancellationToken cancellationToken = default)
    {
        var record = _db.EncryptedGiftCardCodes.FirstOrDefault(c => c.Id == command.CodeRecordId);
        if (record == null)
        {
            return new RevealCardCodeResult(false, string.Empty, null, null, null, "کد یافت نشد.");
        }

        var order = _db.Orders.FirstOrDefault(o => o.Id == record.OrderId);
        if (order == null)
        {
            return new RevealCardCodeResult(false, string.Empty, null, null, null, "سفارش مربوطه یافت نشد.");
        }

        // Security / Authorization check: Customer can only reveal their own code, or audited staff
        if (!command.IsStaff && order.CustomerUserId != command.RequestingUserId)
        {
            _logger.LogWarning("SECURITY VIOLATION: User {UserId} attempted unauthorized access to code {CodeId}", command.RequestingUserId, command.CodeRecordId);
            return new RevealCardCodeResult(false, string.Empty, null, null, null, "دسترسی به این کد امکان‌پذیر نیست.");
        }

        record.RecordReveal(command.RequestingUserId);

        // Audit the reveal operation strictly
        var audit = new AuditEvent(
            eventType: "GiftCardCodeRevealed",
            performedByUserId: command.RequestingUserId,
            performedByUserEmail: command.UserEmail,
            ipAddress: command.IpAddress,
            entityType: "EncryptedGiftCardCode",
            entityId: record.Id.ToString(),
            detailsJson: System.Text.Json.JsonSerializer.Serialize(new
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                IsStaffAccess = command.IsStaff,
                TimestampUtc = DateTime.UtcNow
            })
        );
        await _db.AddAsync(audit, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var plainCode = _encryptionService.Decrypt(record.EncryptedCode);
        var plainPin = !string.IsNullOrWhiteSpace(record.EncryptedPin)
            ? _encryptionService.Decrypt(record.EncryptedPin)
            : null;

        return new RevealCardCodeResult(true, plainCode, plainPin, record.SerialNumberMasked, record.ExpirationDate);
    }
}
