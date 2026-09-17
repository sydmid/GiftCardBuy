namespace GiftStore.Web.Pages.Account;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Domain.Aggregates.Fulfillment;
using GiftStore.Infrastructure.Persistence;

public class OrderDetailsModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public OrderDetailsModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    public Order? Order { get; set; }
    public List<EncryptedGiftCardCode> Codes { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (Order == null) return NotFound();

        Codes = await _db.EncryptedGiftCardCodes
            .Where(c => c.OrderId == id)
            .ToListAsync();

        return Page();
    }
}
