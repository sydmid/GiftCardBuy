namespace GiftStore.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Catalog;

public static class GiftStoreDataSeeder
{
    public static async Task SeedAsync(GiftStoreDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Seeding initial Persian gift-card store catalog and pricing rules...");

        var defaultRule = new PriceRule("حاشیه سود استاندارد ۷ درصد", PriceRuleType.PercentageMargin, 7.0m, round: true);
        await context.PriceRules.AddAsync(defaultRule);

        // Seed Supplier Variants
        var appleUs10Var = new SupplierCatalogVariant("gic-app-us-10", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۱۰ دلاری اپل آمریکا", "APL-US-10", "US", "USD", 10m, 620_000m, true);
        var appleUs25Var = new SupplierCatalogVariant("gic-app-us-25", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۲۵ دلاری اپل آمریکا", "APL-US-25", "US", "USD", 25m, 1_550_000m, true);
        var appleUs50Var = new SupplierCatalogVariant("gic-app-us-50", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۵۰ دلاری اپل آمریکا", "APL-US-50", "US", "USD", 50m, 3_100_000m, true);
        var appleUs100Var = new SupplierCatalogVariant("gic-app-us-100", "prod-apple-us", CardBrand.Apple, "گیفت کارت ۱۰۰ دلاری اپل آمریکا", "APL-US-100", "US", "USD", 100m, 6_200_000m, true);

        var steamUs20Var = new SupplierCatalogVariant("gic-stm-us-20", "prod-steam-us", CardBrand.Steam, "گیفت کارت ۲۰ دلاری استیم والت آمریکا", "STM-US-20", "US", "USD", 20m, 1_240_000m, true);
        var psnUs25Var = new SupplierCatalogVariant("gic-psn-us-25", "prod-psn-us", CardBrand.PlayStation, "گیفت کارت ۲۵ دلاری پلی‌استیشن آمریکا", "PSN-US-25", "US", "USD", 25m, 1_550_000m, true);

        await context.SupplierVariants.AddRangeAsync(appleUs10Var, appleUs25Var, appleUs50Var, appleUs100Var, steamUs20Var, psnUs25Var);
        await context.SaveChangesAsync();

        // Seed Store Products
        var appleProduct = new StoreProduct(
            titleFa: "گیفت کارت اپل آیتونز و اپ‌استور آمریکا",
            slug: "apple-gift-card-usa",
            brand: CardBrand.Apple,
            descriptionFa: "خرید گیفت کارت اپل آمریکا با تحویل آنی پس از پرداخت. قابل استفاده در App Store، Apple Music، iCloud و کلیه سرویس‌های اپل در ریجن ایالات متحده.",
            instructionsFa: "برای شارژ، وارد تنظیمات اپل آیدی خود شوید، گزینه Redeem Gift Card or Code را انتخاب کرده و کد دریافت شده را وارد نمایید. حتما ریجن اکانت شما باید روی United States تنظیم شده باشد.",
            countryCode: "US",
            imageUrl: "/images/products/apple.svg",
            displayOrder: 1
        );

        var steamProduct = new StoreProduct(
            titleFa: "گیفت کارت استیم والت آمریکا",
            slug: "steam-wallet-card-usa",
            brand: CardBrand.Steam,
            descriptionFa: "شارژ مستقیم کیف پول استیم جهت خرید بازی‌های کامپیوتری، آیتم‌های درون بازی و پرداخت‌های کلاینت استیم آمریکا.",
            instructionsFa: "در نرم‌افزار استیم به منوی Games و سپس Redeem a Steam Wallet Code بروید و کد را ثبت کنید.",
            countryCode: "US",
            imageUrl: "/images/products/steam.svg",
            displayOrder: 2
        );

        var psnProduct = new StoreProduct(
            titleFa: "گیفت کارت پلی استیشن PSN آمریکا",
            slug: "playstation-network-card-usa",
            brand: CardBrand.PlayStation,
            descriptionFa: "کد شارژ حساب کاربری پلی‌استیشن برای خرید بازی‌های PS4 و PS5 و اشتراک پلی‌استیشن پلاس در فروشگاه PlayStation Store آمریکا.",
            instructionsFa: "در کنسول یا وبسایت پلی‌استیشن وارد PlayStation Store شده و از منوی پروفایل Redeem Code را بزنید.",
            countryCode: "US",
            imageUrl: "/images/products/playstation.svg",
            displayOrder: 3
        );

        await context.Products.AddRangeAsync(appleProduct, steamProduct, psnProduct);
        await context.SaveChangesAsync();

        // Map Variants
        var m1 = new ProductVariantMapping(appleProduct.Id, appleUs10Var.Id, "اعتبار ۱۰ دلار", defaultRule.Id);
        var m2 = new ProductVariantMapping(appleProduct.Id, appleUs25Var.Id, "اعتبار ۲۵ دلار", defaultRule.Id);
        var m3 = new ProductVariantMapping(appleProduct.Id, appleUs50Var.Id, "اعتبار ۵۰ دلار", defaultRule.Id);
        var m4 = new ProductVariantMapping(appleProduct.Id, appleUs100Var.Id, "اعتبار ۱۰۰ دلار", defaultRule.Id);

        var m5 = new ProductVariantMapping(steamProduct.Id, steamUs20Var.Id, "اعتبار ۲۰ دلار", defaultRule.Id);
        var m6 = new ProductVariantMapping(psnProduct.Id, psnUs25Var.Id, "اعتبار ۲۵ دلار", defaultRule.Id);

        await context.VariantMappings.AddRangeAsync(m1, m2, m3, m4, m5, m6);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with initial products and mappings.");
    }
}
