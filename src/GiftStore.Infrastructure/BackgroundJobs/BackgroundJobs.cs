namespace GiftStore.Infrastructure.BackgroundJobs;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Quartz;
using GiftStore.Application.UseCases.Fulfillment;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Infrastructure.Persistence;

[DisallowConcurrentExecution]
public class OutboxProcessorJob : IJob
{
    private readonly GiftStoreDbContext _db;
    private readonly ProcessFulfillmentHandler _fulfillmentHandler;
    private readonly ILogger<OutboxProcessorJob> _logger;

    public OutboxProcessorJob(GiftStoreDbContext db, ProcessFulfillmentHandler fulfillmentHandler, ILogger<OutboxProcessorJob> logger)
    {
        _db = db;
        _fulfillmentHandler = fulfillmentHandler;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var pendingMessages = await _db.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null && m.RetryCount < 5)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(10)
            .ToListAsync(context.CancellationToken);

        if (pendingMessages.Count == 0) return;

        foreach (var msg in pendingMessages)
        {
            try
            {
                if (msg.EventType == "OrderPaymentConfirmed")
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(msg.Payload);
                    var orderId = doc.RootElement.GetProperty("OrderId").GetGuid();

                    _logger.LogInformation("Outbox: Processing OrderPaymentConfirmed for order {OrderId}", orderId);
                    await _fulfillmentHandler.HandleAsync(new ProcessFulfillmentCommand(orderId), context.CancellationToken);
                }

                msg.ProcessedAtUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {Id}", msg.Id);
                msg.RetryCount++;
                msg.Error = ex.Message;
            }
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}

[DisallowConcurrentExecution]
public class SupplierCatalogSyncJob : IJob
{
    private readonly GiftStoreDbContext _db;
    private readonly IGiftCardSupplier _supplier;
    private readonly ILogger<SupplierCatalogSyncJob> _logger;

    public SupplierCatalogSyncJob(GiftStoreDbContext db, IGiftCardSupplier supplier, ILogger<SupplierCatalogSyncJob> logger)
    {
        _db = db;
        _supplier = supplier;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("SupplierCatalogSyncJob: Starting periodic variant sync from Gift-i-Card...");
        try
        {
            var variants = await _supplier.GetVariantsAsync(context.CancellationToken);
            foreach (var v in variants)
            {
                var existing = await _db.SupplierVariants.FirstOrDefaultAsync(s => s.SupplierVariantId == v.VariantId, context.CancellationToken);
                if (existing != null)
                {
                    existing.UpdateFromSupplier(v.Title, v.PriceToman, v.InStock);
                }
            }

            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("SupplierCatalogSyncJob: Successfully synchronized {Count} variants.", variants.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SupplierCatalogSyncJob: Error during variant catalog synchronization");
        }
    }
}
