namespace GiftStore.Gifticard.Fakes;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GiftStore.Contracts.Enums;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Gifticard.Models;

public class FakeGiftCardSupplier : IGiftCardSupplier
{
    private readonly ILogger<FakeGiftCardSupplier> _logger;
    private readonly ConcurrentDictionary<string, FakeSupplierOrderRecord> _orders = new();

    public bool SimulateTransientFailures { get; set; } = false;
    public decimal WalletBalanceToman { get; set; } = 50_000_000m; // 50 million tomans

    public FakeGiftCardSupplier(ILogger<FakeGiftCardSupplier> logger)
    {
        _logger = logger;
    }

    public Task<SupplierAccountStatus> GetAccountStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SupplierAccountStatus(
            IsConnected: true,
            AccountName: "GiftCardBuy Merchant (Test Sandbox)",
            WalletBalanceToman: WalletBalanceToman,
            Currency: "IRT",
            CheckedAtUtc: DateTime.UtcNow
        ));
    }

    public Task<IReadOnlyList<SupplierProductVariant>> GetVariantsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SupplierProductVariant> variants =
        [
            new("gic-app-us-10", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۱۰ دلاری اپل آیتونز آمریکا", "APL-US-10", "US", "USD", 10m, 620_000m, true),
            new("gic-app-us-25", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۲۵ دلاری اپل آیتونز آمریکا", "APL-US-25", "US", "USD", 25m, 1_550_000m, true),
            new("gic-app-us-50", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۵۰ دلاری اپل آیتونز آمریکا", "APL-US-50", "US", "USD", 50m, 3_100_000m, true),
            new("gic-app-us-100", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۱۰۰ دلاری اپل آیتونز آمریکا", "APL-US-100", "US", "USD", 100m, 6_200_000m, true),
            new("gic-stm-us-20", "prod-steam-us", CardBrand.Steam, "گیفت کارت ۲۰ دلاری استیم والت آمریکا", "STM-US-20", "US", "USD", 20m, 1_240_000m, true),
            new("gic-psn-us-25", "prod-psn-us", CardBrand.PlayStation, "گیفت کارت ۲۵ دلاری پلی‌استیشن آمریکا", "PSN-US-25", "US", "USD", 25m, 1_550_000m, true),
            new("gic-ggl-us-15", "prod-google-us", CardBrand.GooglePlay, "گیفت کارت ۱۵ دلاری گوگل پلی آمریکا", "GGL-US-15", "US", "USD", 15m, 930_000m, true),
            new("gic-xbx-us-25", "prod-xbox-us", CardBrand.Xbox, "گیفت کارت ۲۵ دلاری ایکس‌باکس آمریکا", "XBX-US-25", "US", "USD", 25m, 1_550_000m, true)
        ];

        return Task.FromResult(variants);
    }

    public Task<PurchaseInitiationResult> InitiatePurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        if (SimulateTransientFailures)
        {
            _logger.LogWarning("FakeSupplier: Simulating transient error during InitiatePurchase");
            return Task.FromResult(new PurchaseInitiationResult(false, null, null, 0m, "Simulated supplier gateway timeout"));
        }

        var trackingCode = $"ORD-{Random.Shared.Next(10000, 99999)}";
        var amount = request.Quantity * 620_000m;

        _orders[trackingCode] = new FakeSupplierOrderRecord(
            TrackingCode: trackingCode,
            VariantId: request.SupplierVariantId,
            Quantity: request.Quantity,
            Status: "waiting_payment",
            CreatedAt: DateTime.UtcNow
        );

        _logger.LogInformation("FakeSupplier: Created purchase {TrackingCode} for variant {VariantId}", trackingCode, request.SupplierVariantId);
        return Task.FromResult(new PurchaseInitiationResult(true, trackingCode, "waiting_payment", amount));
    }

    public Task<PurchaseConfirmationResult> ConfirmPurchaseAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryGetValue(reference.TrackingCode, out var order))
        {
            return Task.FromResult(new PurchaseConfirmationResult(false, reference.TrackingCode, "failed", null, "Order not found in fake supplier"));
        }

        order.Status = "processing";
        _logger.LogInformation("FakeSupplier: Confirmed and processed {TrackingCode}", reference.TrackingCode);
        return Task.FromResult(new PurchaseConfirmationResult(true, reference.TrackingCode, "processing", "Order queued for digital code generation"));
    }

    public Task<SupplierOrderResult> RetrieveOrderAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryGetValue(reference.TrackingCode, out var order))
        {
            return Task.FromResult(new SupplierOrderResult(false, reference.TrackingCode, "failed", [], "Order not found"));
        }

        var cards = new List<PurchasedCardItem>();
        for (int i = 0; i < order.Quantity; i++)
        {
            var code = $"X{GenerateRandomCode(15)}";
            var pin = $"{Random.Shared.Next(1000, 9999)}";
            var serial = $"SN-{Random.Shared.Next(100000, 999999)}";
            cards.Add(new PurchasedCardItem(code, pin, serial, "2028-12-31"));
        }

        order.Status = "completed";
        _logger.LogInformation("FakeSupplier: Retrieved {Count} codes for {TrackingCode}", cards.Count, reference.TrackingCode);
        return Task.FromResult(new SupplierOrderResult(true, reference.TrackingCode, "completed", cards));
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    private class FakeSupplierOrderRecord
    {
        public string TrackingCode { get; set; }
        public string VariantId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public FakeSupplierOrderRecord(string TrackingCode, string VariantId, int Quantity, string Status, DateTime CreatedAt)
        {
            this.TrackingCode = TrackingCode;
            this.VariantId = VariantId;
            this.Quantity = Quantity;
            this.Status = Status;
            this.CreatedAt = CreatedAt;
        }
    }
}
