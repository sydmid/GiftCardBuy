namespace GiftStore.Application.Interfaces;

using GiftStore.Domain.Aggregates.Catalog;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Aggregates.Payments;
using GiftStore.Domain.Aggregates.Fulfillment;
using GiftStore.Domain.Aggregates.Cart;
using GiftStore.Domain.Aggregates.Audit;
using GiftStore.Contracts.Dtos;

public interface IGiftStoreDbContext
{
    IQueryable<StoreProduct> Products { get; }
    IQueryable<SupplierCatalogVariant> SupplierVariants { get; }
    IQueryable<ProductVariantMapping> VariantMappings { get; }
    IQueryable<PriceRule> PriceRules { get; }
    IQueryable<Order> Orders { get; }
    IQueryable<OrderItem> OrderItems { get; }
    IQueryable<PaymentAttempt> PaymentAttempts { get; }
    IQueryable<FulfillmentAttempt> FulfillmentAttempts { get; }
    IQueryable<SupplierOrder> SupplierOrders { get; }
    IQueryable<EncryptedGiftCardCode> EncryptedGiftCardCodes { get; }
    IQueryable<ShoppingCart> ShoppingCarts { get; }
    IQueryable<AuditEvent> AuditEvents { get; }

    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOutboxService
{
    Task EnqueueEventAsync(string eventType, string payloadJson, CancellationToken cancellationToken = default);
}

public interface IPriceCalculator
{
    decimal CalculateUnitSellingPrice(SupplierCatalogVariant variant, ProductVariantMapping mapping, PriceRule? rule);
}

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public interface IDistributedCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public interface ICmsService
{
    Task<IReadOnlyList<CmsArticleDto>> GetPublishedArticlesAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<CmsArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task InvalidateArticleCacheAsync(string? slug = null, CancellationToken cancellationToken = default);
}
