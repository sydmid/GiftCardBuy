namespace GiftStore.Web.Pages.Admin.Audit;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Domain.Aggregates.Audit;
using GiftStore.Infrastructure.Persistence;

public class IndexModel : PageModel
{
    private readonly GiftStoreDbContext _db;

    public IndexModel(GiftStoreDbContext db)
    {
        _db = db;
    }

    public List<AuditEvent> AuditEvents { get; set; } = [];

    public async Task OnGetAsync()
    {
        AuditEvents = await _db.AuditEvents
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(100)
            .ToListAsync();
    }
}
