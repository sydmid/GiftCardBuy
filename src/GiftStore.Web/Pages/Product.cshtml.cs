namespace GiftStore.Web.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Application.Interfaces;
using GiftStore.Domain.Aggregates.Catalog;
using GiftStore.Infrastructure.Persistence;

public class ProductModel : PageModel
{
    private readonly GiftStoreDbContext _db;
    private readonly IPriceCalculator _priceCalculator;

    public ProductModel(GiftStoreDbContext db, IPriceCalculator priceCalculator)
    {
        _db = db;
        _priceCalculator = priceCalculator;
    }

    public StoreProduct? Product { get; set; }
    public List<VariantViewModel> Variants { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToPage("/Index");

        Product = await _db.Products
            .Include(p => p.VariantMappings)
            .ThenInclude(m => m.SupplierVariant)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

        if (Product == null) return NotFound();

        Variants = Product.VariantMappings
            .Where(m => m.IsAvailableForPurchase && m.SupplierVariant.InStock)
            .Select(m => new VariantViewModel(
                m.Id,
                m.DisplayTitleFa,
                m.SupplierVariant.FaceValue,
                m.SupplierVariant.FaceCurrency,
                _priceCalculator.CalculateUnitSellingPrice(m.SupplierVariant, m, null)
            )).ToList();

        return Page();
    }

    public IActionResult OnPostBuyNow(string slug, Guid selectedVariantMappingId, int quantity)
    {
        return RedirectToPage("/Checkout", new
        {
            mappingId = selectedVariantMappingId,
            qty = Math.Clamp(quantity, 1, 5)
        });
    }

    public record VariantViewModel(
        Guid MappingId,
        string DisplayTitleFa,
        decimal FaceValue,
        string FaceCurrency,
        decimal CalculatedPriceToman
    );
}
