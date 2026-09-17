namespace GiftStore.Contracts.Dtos;

public record CmsArticleDto(
    string Id,
    string Title,
    string Slug,
    string Excerpt,
    string BodyHtml,
    string? FeaturedImageUrl,
    string? FeaturedImageAlt,
    string AuthorName,
    string CategoryName,
    IReadOnlyList<string> Tags,
    DateTime PublishedDateUtc,
    DateTime? ModifiedDateUtc,
    string? CanonicalUrl,
    string? SeoTitle,
    string? SeoDescription,
    bool NoIndex
);
