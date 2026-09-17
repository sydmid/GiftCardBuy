namespace GiftStore.Web.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Contracts.Enums;
using GiftStore.Infrastructure.Persistence;

public class IndexModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public IndexModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public CardBrand? Brand { get; set; }

    public CardBrand? SelectedBrand => Brand;

    public List<ProductItemViewModel> Products { get; set; } = [];

    public async Task OnGetAsync()
    {
        var query = _db.Products
            .Include(p => p.VariantMappings)
            .ThenInclude(m => m.SupplierVariant)
            .Where(p => p.IsPublished);

        if (Brand.HasValue)
        {
            query = query.Where(p => p.Brand == Brand.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var s = SearchQuery.Trim().ToLower();
            query = query.Where(p => p.TitleFa.ToLower().Contains(s) || p.Slug.ToLower().Contains(s));
        }

        var list = await query.OrderBy(p => p.DisplayOrder).ToListAsync();

        Products = list.Select(p =>
        {
            var lowest = p.VariantMappings
                .Where(m => m.IsAvailableForPurchase && m.SupplierVariant.InStock)
                .Select(m => m.OverrideSellingPriceToman > 0 ? m.OverrideSellingPriceToman : m.SupplierVariant.SupplierCostToman * 1.07m)
                .DefaultIfEmpty(0m)
                .Min();

            return new ProductItemViewModel(
                p.Id,
                p.TitleFa,
                p.Slug,
                p.Brand,
                p.DescriptionFa,
                p.CountryCode,
                p.ImageUrl,
                lowest
            );
        }).ToList();
    }

    public record ProductItemViewModel(
        Guid Id,
        string TitleFa,
        string Slug,
        CardBrand Brand,
        string DescriptionFa,
        string CountryCode,
        string ImageUrl,
        decimal LowestPriceToman
    );
}
