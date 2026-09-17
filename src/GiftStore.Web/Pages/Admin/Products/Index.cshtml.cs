namespace GiftStore.Web.Pages.Admin.Products;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Domain.Aggregates.Catalog;
using GiftStore.Infrastructure.Persistence;

public class IndexModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public IndexModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    public List<StoreProduct> Products { get; set; } = [];

    public async Task OnGetAsync()
    {
        Products = await _db.Products
            .Include(p => p.VariantMappings)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
    }
}
