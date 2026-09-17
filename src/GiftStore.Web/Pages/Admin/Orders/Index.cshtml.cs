namespace GiftStore.Web.Pages.Admin.Orders;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Application.UseCases.Fulfillment;
using GiftStore.Domain.Aggregates.Orders;
using GiftStore.Infrastructure.Persistence;

public class IndexModel : PageModel
{
    private readonly GiftStoreDbContext _db;
    private readonly ProcessFulfillmentHandler _fulfillmentHandler;

    public IndexModel(GiftStoreDbContext db, ProcessFulfillmentHandler fulfillmentHandler)
    {
        _db = db;
        _fulfillmentHandler = fulfillmentHandler;
    }

    public List<Order> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        Orders = await _db.Orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRetryAsync(Guid orderId)
    {
        await _fulfillmentHandler.HandleAsync(new ProcessFulfillmentCommand(orderId));
        return RedirectToPage();
    }
}
