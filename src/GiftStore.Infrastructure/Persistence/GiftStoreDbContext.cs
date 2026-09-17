namespace GiftStore.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using GiftStore.Application.Interfaces;
using GiftStore.Domain.Aggregates.Catalog;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Aggregates.Payments;
using GiftStore.Domain.Aggregates.Fulfillment;
using GiftStore.Domain.Aggregates.Cart;
using GiftStore.Domain.Aggregates.Audit;
using GiftStore.Infrastructure.Persistence.Entities;

public class GiftStoreDbContext : DbContext, IGiftStoreDbContext
{
    public GiftStoreDbContext(DbContextOptions<GiftStoreDbContext> options) : base(options)
    {
    }

    public DbSet<StoreProduct> Products => Set<StoreProduct>();
    public DbSet<SupplierCatalogVariant> SupplierVariants => Set<SupplierCatalogVariant>();
    public DbSet<ProductVariantMapping> VariantMappings => Set<ProductVariantMapping>();
    public DbSet<PriceRule> PriceRules => Set<PriceRule>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<FulfillmentAttempt> FulfillmentAttempts => Set<FulfillmentAttempt>();
    public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
    public DbSet<EncryptedGiftCardCode> EncryptedGiftCardCodes => Set<EncryptedGiftCardCode>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<CmsArticleCache> CmsArticleCaches => Set<CmsArticleCache>();

    IQueryable<StoreProduct> IGiftStoreDbContext.Products => Products.AsNoTracking();
    IQueryable<SupplierCatalogVariant> IGiftStoreDbContext.SupplierVariants => SupplierVariants.AsNoTracking();
    IQueryable<ProductVariantMapping> IGiftStoreDbContext.VariantMappings => VariantMappings.AsNoTracking();
    IQueryable<PriceRule> IGiftStoreDbContext.PriceRules => PriceRules.AsNoTracking();
    IQueryable<Order> IGiftStoreDbContext.Orders => Orders;
    IQueryable<OrderItem> IGiftStoreDbContext.OrderItems => OrderItems;
    IQueryable<PaymentAttempt> IGiftStoreDbContext.PaymentAttempts => PaymentAttempts;
    IQueryable<FulfillmentAttempt> IGiftStoreDbContext.FulfillmentAttempts => FulfillmentAttempts;
    IQueryable<SupplierOrder> IGiftStoreDbContext.SupplierOrders => SupplierOrders;
    IQueryable<EncryptedGiftCardCode> IGiftStoreDbContext.EncryptedGiftCardCodes => EncryptedGiftCardCodes;
    IQueryable<ShoppingCart> IGiftStoreDbContext.ShoppingCarts => ShoppingCarts;
    IQueryable<AuditEvent> IGiftStoreDbContext.AuditEvents => AuditEvents;

    public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        await Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<StoreProduct>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasIndex(p => p.Slug).IsUnique();
            b.Property(p => p.TitleFa).HasMaxLength(250).IsRequired();
            b.Property(p => p.Slug).HasMaxLength(150).IsRequired();
            b.Property(p => p.CountryCode).HasMaxLength(10).IsRequired();
        });

        // SupplierCatalogVariant
        modelBuilder.Entity<SupplierCatalogVariant>(b =>
        {
            b.HasKey(v => v.Id);
            b.HasIndex(v => v.SupplierVariantId).IsUnique();
            b.Property(v => v.SupplierVariantId).HasMaxLength(100).IsRequired();
            b.Property(v => v.Sku).HasMaxLength(100).IsRequired();
            b.Property(v => v.SupplierCostToman).HasPrecision(18, 2);
            b.Property(v => v.FaceValue).HasPrecision(18, 2);
        });

        // ProductVariantMapping
        modelBuilder.Entity<ProductVariantMapping>(b =>
        {
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.StoreProductId, m.SupplierCatalogVariantId }).IsUnique();
            b.HasOne(m => m.StoreProduct)
                .WithMany(p => p.VariantMappings)
                .HasForeignKey(m => m.StoreProductId);
            b.HasOne(m => m.SupplierVariant)
                .WithMany()
                .HasForeignKey(m => m.SupplierCatalogVariantId);
            b.Property(m => m.OverrideSellingPriceToman).HasPrecision(18, 2);
        });

        // PriceRule
        modelBuilder.Entity<PriceRule>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Value).HasPrecision(18, 2);
        });

        // Order
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.HasIndex(o => o.OrderNumber).IsUnique();
            b.HasIndex(o => o.CustomerUserId);
            b.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            b.Property(o => o.TotalAmountToman).HasPrecision(18, 2);
            b.Property(o => o.RowVersion).IsRowVersion();
            b.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.OwnsOne(i => i.PriceSnapshot, ps =>
            {
                ps.Property(p => p.UnitSupplierCostToman).HasPrecision(18, 2);
                ps.Property(p => p.UnitSellingPriceToman).HasPrecision(18, 2);
                ps.Property(p => p.TotalSellingPriceToman).HasPrecision(18, 2);
            });
        });

        // PaymentAttempt
        modelBuilder.Entity<PaymentAttempt>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasIndex(p => p.OrderId);
            b.HasIndex(p => p.TransactionReference).IsUnique();
            b.Property(p => p.AmountToman).HasPrecision(18, 2);
        });

        // FulfillmentAttempt
        modelBuilder.Entity<FulfillmentAttempt>(b =>
        {
            b.HasKey(f => f.Id);
            b.HasIndex(f => f.OrderId);
            b.HasIndex(f => f.IdempotencyKey).IsUnique();
        });

        // SupplierOrder
        modelBuilder.Entity<SupplierOrder>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => s.SupplierTrackingCode).IsUnique();
            b.Property(s => s.SupplierCost).HasPrecision(18, 2);
        });

        // EncryptedGiftCardCode
        modelBuilder.Entity<EncryptedGiftCardCode>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.OrderId);
            b.HasIndex(c => c.OrderItemId);
            b.Property(c => c.EncryptedCode).IsRequired();
        });

        // ShoppingCart
        modelBuilder.Entity<ShoppingCart>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.AnonymousCartSessionId);
            b.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.ShoppingCartId);
        });

        // IdempotencyRecord
        modelBuilder.Entity<IdempotencyRecord>(b =>
        {
            b.HasKey(r => r.Key);
        });

        // CmsArticleCache
        modelBuilder.Entity<CmsArticleCache>(b =>
        {
            b.HasKey(c => c.Slug);
        });
    }
}
