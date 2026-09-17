namespace GiftStore.Domain.Aggregates.Audit;

using GiftStore.Domain.Common;

public class AuditEvent : BaseEntity
{
    public string EventType { get; private set; } = string.Empty; // e.g. "PriceChanged", "CodeRevealed", "FulfillmentRetried", "AdminAction"
    public string PerformedByUserId { get; private set; } = string.Empty;
    public string? PerformedByUserEmail { get; private set; }
    public string? IpAddress { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string DetailsJson { get; private set; } = string.Empty;

    private AuditEvent() { }

    public AuditEvent(
        string eventType,
        string performedByUserId,
        string? performedByUserEmail,
        string? ipAddress,
        string entityType,
        string entityId,
        string detailsJson)
    {
        EventType = eventType;
        PerformedByUserId = performedByUserId;
        PerformedByUserEmail = performedByUserEmail;
        IpAddress = ipAddress;
        EntityType = entityType;
        EntityId = entityId;
        DetailsJson = detailsJson;
    }
}
