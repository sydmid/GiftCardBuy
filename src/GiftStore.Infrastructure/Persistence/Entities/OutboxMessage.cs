namespace GiftStore.Infrastructure.Persistence.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

public class IdempotencyRecord
{
    public string Key { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
}

public class CmsArticleCache
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public string? FeaturedImageAlt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string TagsJson { get; set; } = "[]";
    public DateTime PublishedDateUtc { get; set; }
    public DateTime? ModifiedDateUtc { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public bool NoIndex { get; set; }
    public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
}
