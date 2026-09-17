namespace GiftStore.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GiftStore.Application.UseCases.Fulfillment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CodesController : ControllerBase
{
    private readonly RevealCardCodeHandler _revealHandler;

    public CodesController(RevealCardCodeHandler revealHandler)
    {
        _revealHandler = revealHandler;
    }

    [HttpPost("reveal")]
    public async Task<IActionResult> Reveal([FromBody] RevealCodeApiRequest request)
    {
        var userId = User.Identity?.Name ?? "anonymous";
        var isStaff = User.IsInRole("Admin");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var cmd = new RevealCardCodeCommand(request.CodeRecordId, userId, null, ip, isStaff);
        var res = await _revealHandler.HandleAsync(cmd);

        if (!res.IsSuccess)
        {
            return BadRequest(new { success = false, message = res.ErrorMessage });
        }

        return Ok(new
        {
            success = true,
            code = res.DecryptedCode,
            pin = res.DecryptedPin,
            serial = res.MaskedSerial,
            expiration = res.ExpirationDate
        });
    }

    public record RevealCodeApiRequest(Guid CodeRecordId);
}
