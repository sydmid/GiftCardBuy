namespace GiftStore.Web.Pages.Admin;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Contracts.Enums;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Infrastructure.Persistence;

public class IndexModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public IndexModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    public int TotalOrdersCount { get; set; }
    public int FulfilledOrdersCount { get; set; }
    public int ManualReviewQueueCount { get; set; }
    public decimal TotalRevenueToman { get; set; }
    public List<Order> RecentOrders { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalOrdersCount = await _db.Orders.CountAsync();
        FulfilledOrdersCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Fulfilled);
        ManualReviewQueueCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.ManualReviewRequired);
        TotalRevenueToman = await _db.Orders
            .Where(o => o.Status == OrderStatus.Fulfilled)
            .SumAsync(o => (decimal?)o.TotalAmountToman) ?? 0m;

        RecentOrders = await _db.Orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(5)
            .ToListAsync();
    }
}
