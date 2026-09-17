namespace GiftStore.Infrastructure.Persistence;

using GiftStore.Application.Interfaces;
using GiftStore.Infrastructure.Persistence.Entities;

public class OutboxService : IOutboxService
{
    private readonly GiftStoreDbContext _db;

    public OutboxService(GiftStoreDbContext db)
    {
        _db = db;
    }

    public async Task EnqueueEventAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default)
    {
        var msg = new OutboxMessage
        {
            EventType = eventType,
            Payload = payloadJson,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _db.OutboxMessages.AddAsync(msg, cancellationToken);
    }
}
