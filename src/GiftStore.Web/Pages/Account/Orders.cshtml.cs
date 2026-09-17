namespace GiftStore.Web.Pages.Account;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Infrastructure.Persistence;

public class OrdersModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public OrdersModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    public List<Order> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        var userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";

        Orders = await _db.Orders
            .Where(o => o.CustomerUserId == userIdentifier || o.CustomerEmail == userIdentifier)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();
    }
}
