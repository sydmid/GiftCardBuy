namespace GiftStore.Cms.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Dtos;

public class OrchardCoreCmsOptions
{
    public const string SectionName = "Cms";
    public string BaseUrl { get; set; } = "http://localhost:5050/";
    public string? ApiKey { get; set; }
    public string WebhookSecret { get; set; } = "secret-cms-key";
    public int CacheTtlMinutes { get; set; } = 60;
}

public class CachedPersianCmsService : ICmsService
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCacheService _cache;
    private readonly IOptions<OrchardCoreCmsOptions> _options;
    private readonly ILogger<CachedPersianCmsService> _logger;

    public CachedPersianCmsService(
        HttpClient httpClient,
        IDistributedCacheService cache,
        IOptions<OrchardCoreCmsOptions> options,
        ILogger<CachedPersianCmsService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CmsArticleDto>> GetPublishedArticlesAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"cms:articles:page_{page}_size_{pageSize}";
        var cached = await _cache.GetAsync<List<CmsArticleDto>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            // Call Orchard Core decoupled content item API
            var response = await _httpClient.GetAsync($"/api/content?contentType=BlogPost&page={page}&pageSize={pageSize}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var articles = await response.Content.ReadFromJsonAsync<List<CmsArticleDto>>(cancellationToken: cancellationToken);
                if (articles != null && articles.Count > 0)
                {
                    await _cache.SetAsync(cacheKey, articles, TimeSpan.FromMinutes(_options.Value.CacheTtlMinutes), cancellationToken);
                    return articles;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CMS service unavailable. Serving fallback Persian articles.");
        }

        // Resilient fallback articles for Persian blog
        var fallbackList = GetSamplePersianArticles();
        await _cache.SetAsync(cacheKey, fallbackList, TimeSpan.FromMinutes(10), cancellationToken);
        return fallbackList;
    }

    public async Task<CmsArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"cms:article:{slug}";
        var cached = await _cache.GetAsync<CmsArticleDto>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        try
        {
            var response = await _httpClient.GetAsync($"/api/content/slug/{Uri.EscapeDataString(slug)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var article = await response.Content.ReadFromJsonAsync<CmsArticleDto>(cancellationToken: cancellationToken);
                if (article != null)
                {
                    await _cache.SetAsync(cacheKey, article, TimeSpan.FromMinutes(_options.Value.CacheTtlMinutes), cancellationToken);
                    return article;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CMS service lookup failed for slug {Slug}. Checking fallback.", slug);
        }

        var sample = GetSamplePersianArticles().FirstOrDefault(a => a.Slug == slug);
        if (sample != null)
        {
            await _cache.SetAsync(cacheKey, sample, TimeSpan.FromMinutes(10), cancellationToken);
        }
        return sample;
    }

    public async Task InvalidateArticleCacheAsync(string? slug = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating CMS cache for slug: {Slug}", slug ?? "ALL");
        if (!string.IsNullOrEmpty(slug))
        {
            await _cache.RemoveAsync($"cms:article:{slug}", cancellationToken);
        }
        // Invalidate common listing pages
        for (int i = 1; i <= 5; i++)
        {
            await _cache.RemoveAsync($"cms:articles:page_{i}_size_10", cancellationToken);
        }
    }

    private static List<CmsArticleDto> GetSamplePersianArticles()
    {
        return
        [
            new(
                Id: "art-1",
                Title: "راهنمای جامع ساخت اپل آیدی آمریکا بدون نیاز به شماره مجازی",
                Slug: "how-to-create-us-apple-id-guide",
                Excerpt: "در این راهنما گام‌به‌گام با نحوه ایجاد یک حساب معتبر اپل آیدی ریجن ایالات متحده با آدرس معتبر و نکات امنیتی آشنا می‌شوید.",
                BodyHtml: "<p>داشتن اپل آیدی معتبر آمریکا به شما امکان دسترسی به گسترده‌ترین فروشگاه اپ استور و امکان ردیم مستقیم گیفت کارت‌های آیتونز را می‌دهد...</p>",
                FeaturedImageUrl: "/images/blog/apple-guide.jpg",
                FeaturedImageAlt: "آموزش ساخت اپل آیدی آمریکا",
                AuthorName: "تیم فنی گیفت‌کارت‌بای",
                CategoryName: "آموزش اپل",
                Tags: ["اپل آیدی", "گیفت کارت اپل", "آموزش"],
                PublishedDateUtc: DateTime.UtcNow.AddDays(-3),
                ModifiedDateUtc: DateTime.UtcNow.AddDays(-1),
                CanonicalUrl: "https://giftcardbuy.ir/blog/how-to-create-us-apple-id-guide",
                SeoTitle: "آموزش ساخت اپل آیدی آمریکا رایگان و قدم به قدم | گیفت‌کارت‌بای",
                SeoDescription: "کامل‌ترین آموزش تصویری ساخت اکانت Apple ID ریجن آمریکا بدون شماره تلفن و نحوه استفاده از گیفت کارت دلاری.",
                NoIndex: false
            ),
            new(
                Id: "art-2",
                Title: "نحوه ردیم و شارژ استیم والت با گیفت کارت استیم آمریکا",
                Slug: "how-to-redeem-steam-wallet-code",
                Excerpt: "آموزش کامل نحوه وارد کردن کدهای استیم در کلاینت Steam و نکات تغییر ریجن و جلوگیری از مسدودی حساب کاربری.",
                BodyHtml: "<p>برای شارژ کیف پول استیم، ابتدا باید اطمینان حاصل کنید آی‌پی شما با ریجن والت مطابقت داشته باشد...</p>",
                FeaturedImageUrl: "/images/blog/steam-guide.jpg",
                FeaturedImageAlt: "آموزش شارژ استیم والت",
                AuthorName: "بخش گیمینگ",
                CategoryName: "آموزش گیمینگ",
                Tags: ["استیم", "بازی کامپیوتر", "شارژ استیم"],
                PublishedDateUtc: DateTime.UtcNow.AddDays(-7),
                ModifiedDateUtc: null,
                CanonicalUrl: "https://giftcardbuy.ir/blog/how-to-redeem-steam-wallet-code",
                SeoTitle: "آموزش ردیم گیفت کارت استیم آمریکا | مراحل گام به گام",
                SeoDescription: "راهنمای آسان برای وارد کردن کد گیفت کارت استیم در والت بدون ارور ریجن و دریافت شارژ ارزی.",
                NoIndex: false
            )
        ];
    }
}
