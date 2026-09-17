namespace GiftStore.Application.UseCases.Checkout;

using Microsoft.Extensions.Logging;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Catalog;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Aggregates.Payments;
using GiftStore.Domain.Exceptions;
using GiftStore.Payment.Interfaces;
using GiftStore.Payment.Models;

public record CreateOrderCommand(
    Guid VariantMappingId,
    int Quantity,
    decimal ExpectedUnitPriceToman,
    string CustomerUserId,
    string CustomerEmail,
    string? CustomerMobile
);

public record CreateOrderResult(
    bool IsSuccess,
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmountToman,
    string? ErrorMessage = null
);

public class CreateOrderHandler
{
    private readonly IGiftStoreDbContext _db;
    private readonly IPriceCalculator _priceCalculator;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(IGiftStoreDbContext db, IPriceCalculator priceCalculator, ILogger<CreateOrderHandler> logger)
    {
        _db = db;
        _priceCalculator = priceCalculator;
        _logger = logger;
    }

    public async Task<CreateOrderResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var mapping = _db.VariantMappings.FirstOrDefault(m => m.Id == command.VariantMappingId);
        if (mapping == null || !mapping.IsAvailableForPurchase)
        {
            return new CreateOrderResult(false, Guid.Empty, string.Empty, 0m, "واریانت انتخاب شده در دسترس نیست.");
        }

        var product = _db.Products.FirstOrDefault(p => p.Id == mapping.StoreProductId);
        if (product == null || !product.IsPublished)
        {
            return new CreateOrderResult(false, Guid.Empty, string.Empty, 0m, "محصول مورد نظر غیرفعال است.");
        }

        var supplierVariant = _db.SupplierVariants.FirstOrDefault(v => v.Id == mapping.SupplierCatalogVariantId);
        if (supplierVariant == null || !supplierVariant.InStock)
        {
            return new CreateOrderResult(false, Guid.Empty, string.Empty, 0m, "موجودی این گیفت‌کارت نزد تامین‌کننده به اتمام رسیده است.");
        }

        if (command.Quantity < mapping.MinPurchaseQuantity || command.Quantity > mapping.MaxPurchaseQuantity)
        {
            return new CreateOrderResult(false, Guid.Empty, string.Empty, 0m, $"تعداد سفارش باید بین {mapping.MinPurchaseQuantity} و {mapping.MaxPurchaseQuantity} باشد.");
        }

        // Price change protection
        var rule = mapping.PriceRuleId.HasValue
            ? _db.PriceRules.FirstOrDefault(r => r.Id == mapping.PriceRuleId.Value)
            : null;

        var currentUnitPrice = _priceCalculator.CalculateUnitSellingPrice(supplierVariant, mapping, rule);
        if (currentUnitPrice != command.ExpectedUnitPriceToman)
        {
            _logger.LogWarning("Price discrepancy detected: Expected {Exp}, Current {Curr}", command.ExpectedUnitPriceToman, currentUnitPrice);
            return new CreateOrderResult(false, Guid.Empty, string.Empty, 0m, $"قیمت محصول تغییر کرده است. قیمت جدید: {currentUnitPrice:N0} تومان.");
        }

        var order = new Order(command.CustomerUserId, command.CustomerEmail, command.CustomerMobile);
        var snapshot = new PriceSnapshot(
            UnitSupplierCostToman: supplierVariant.SupplierCostToman,
            UnitSellingPriceToman: currentUnitPrice,
            TotalSellingPriceToman: currentUnitPrice * command.Quantity
        );

        var orderItem = new OrderItem(
            productVariantMappingId: mapping.Id,
            supplierVariantId: supplierVariant.SupplierVariantId,
            productTitleFa: product.TitleFa,
            variantTitleFa: mapping.DisplayTitleFa,
            quantity: command.Quantity,
            priceSnapshot: snapshot
        );

        order.AddItem(orderItem);
        order.MarkAwaitingPayment();

        await _db.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order created successfully: {OrderNumber}, Total: {Total} IRT", order.OrderNumber, order.TotalAmountToman);
        return new CreateOrderResult(true, order.Id, order.OrderNumber, order.TotalAmountToman);
    }
}

public record InitiatePaymentCommand(
    Guid OrderId,
    string CallbackUrl
);

public class InitiatePaymentHandler
{
    private readonly IGiftStoreDbContext _db;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<InitiatePaymentHandler> _logger;

    public InitiatePaymentHandler(IGiftStoreDbContext db, IPaymentGateway paymentGateway, ILogger<InitiatePaymentHandler> logger)
    {
        _db = db;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<PaymentInitiationResult> HandleAsync(InitiatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.Id == command.OrderId);
        if (order == null || order.Status != OrderStatus.AwaitingPayment)
        {
            return new PaymentInitiationResult(false, string.Empty, string.Empty, "سفارش در وضعیت مناسب جهت پرداخت قرار ندارد.");
        }

        var request = new PaymentInitiationRequest(
            OrderId: order.Id,
            OrderNumber: order.OrderNumber,
            AmountToman: order.TotalAmountToman,
            Description: $"خرید گیفت کارت - سفارش {order.OrderNumber}",
            CustomerEmail: order.CustomerEmail,
            CustomerMobile: order.CustomerPhoneNumber,
            CallbackUrl: command.CallbackUrl
        );

        var result = await _paymentGateway.InitiatePaymentAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        var attempt = new PaymentAttempt(order.Id, _paymentGateway.ProviderName, result.TransactionReference, order.TotalAmountToman);
        attempt.MarkPending();

        await _db.AddAsync(attempt, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return result;
    }
}
