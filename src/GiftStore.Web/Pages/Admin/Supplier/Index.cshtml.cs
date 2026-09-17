namespace GiftStore.Web.Pages.Admin.Supplier;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Gifticard.Models;

public class IndexModel : PageModel
{
    private readonly IGiftCardSupplier _supplier;

    public IndexModel(IGiftCardSupplier supplier)
    {
        _supplier = supplier;
    }

    public SupplierAccountStatus? Status { get; set; }

    public async Task OnGetAsync()
    {
        Status = await _supplier.GetAccountStatusAsync();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        await _supplier.GetVariantsAsync();
        return RedirectToPage();
    }
}
