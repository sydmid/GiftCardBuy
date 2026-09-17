namespace GiftStore.Web.Pages;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GiftStore.Application.Interfaces;
using GiftStore.Application.UseCases.Checkout;
using GiftStore.Infrastructure.Persistence;

public class CheckoutModel : PageModel
{
    private readonly GiftStoreDbContext _db;
    private readonly IPriceCalculator _priceCalculator;
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly InitiatePaymentHandler _initiatePaymentHandler;

    public CheckoutModel(
        GiftStoreDbContext db,
        IPriceCalculator priceCalculator,
        CreateOrderHandler createOrderHandler,
        InitiatePaymentHandler initiatePaymentHandler)
    {
        _db = db;
        _priceCalculator = priceCalculator;
        _createOrderHandler = createOrderHandler;
        _initiatePaymentHandler = initiatePaymentHandler;
    }

    [BindProperty]
    public Guid VariantMappingId { get; set; }

    [BindProperty]
    public int Quantity { get; set; }

    [BindProperty]
    public decimal ExpectedUnitPriceToman { get; set; }

    [BindProperty]
    public string CustomerEmail { get; set; } = string.Empty;

    [BindProperty]
    public string? CustomerMobile { get; set; }

    public string ProductTitle { get; set; } = string.Empty;
    public string VariantTitle { get; set; } = string.Empty;
    public decimal ItemTotalToman => ExpectedUnitPriceToman * Quantity;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid mappingId, int qty = 1)
    {
        VariantMappingId = mappingId;
        Quantity = Math.Clamp(qty, 1, 5);

        var mapping = await _db.VariantMappings
            .Include(m => m.StoreProduct)
            .Include(m => m.SupplierVariant)
            .FirstOrDefaultAsync(m => m.Id == mappingId);

        if (mapping == null) return RedirectToPage("/Index");

        ProductTitle = mapping.StoreProduct.TitleFa;
        VariantTitle = mapping.DisplayTitleFa;
        ExpectedUnitPriceToman = _priceCalculator.CalculateUnitSellingPrice(mapping.SupplierVariant, mapping, null);

        if (User.Identity?.IsAuthenticated == true)
        {
            CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name ?? "";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString("N");

        var createCmd = new CreateOrderCommand(
            VariantMappingId: VariantMappingId,
            Quantity: Quantity,
            ExpectedUnitPriceToman: ExpectedUnitPriceToman,
            CustomerUserId: userId,
            CustomerEmail: CustomerEmail,
            CustomerMobile: CustomerMobile
        );

        var orderRes = await _createOrderHandler.HandleAsync(createCmd);
        if (!orderRes.IsSuccess)
        {
            ErrorMessage = orderRes.ErrorMessage;
            return await OnGetAsync(VariantMappingId, Quantity);
        }

        var callbackUrl = Url.Page("/PaymentReturn", pageHandler: null, values: null, protocol: Request.Scheme)!;
        var payCmd = new InitiatePaymentCommand(orderRes.OrderId, callbackUrl);
        var payRes = await _initiatePaymentHandler.HandleAsync(payCmd);

        if (!payRes.IsSuccess)
        {
            ErrorMessage = payRes.ErrorMessage;
            return await OnGetAsync(VariantMappingId, Quantity);
        }

        return Redirect(payRes.GatewayRedirectUrl);
    }
}
