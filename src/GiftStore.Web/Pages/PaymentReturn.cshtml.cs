namespace GiftStore.Web.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GiftStore.Application.UseCases.Payments;

public class PaymentReturnModel : PageModel
{
    private readonly ProcessPaymentCallbackHandler _callbackHandler;

    public PaymentReturnModel(ProcessPaymentCallbackHandler callbackHandler)
    {
        _callbackHandler = callbackHandler;
    }

    public bool IsSuccess { get; set; }
    public Guid OrderId { get; set; }
    public string? ReferenceCode { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] Guid orderId,
        [FromQuery] string authority,
        [FromQuery] string? status)
    {
        OrderId = orderId;
        var cmd = new ProcessPaymentCallbackCommand(orderId, authority, status);
        var result = await _callbackHandler.HandleAsync(cmd);

        IsSuccess = result.IsSuccess;
        ReferenceCode = result.ReferenceNumber;
        ErrorMessage = result.Message;

        return Page();
    }
}
