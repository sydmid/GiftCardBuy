namespace GiftStore.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using GiftStore.Application.Interfaces;
using GiftStore.Cms.Services;

[ApiController]
[Route("api/cms/webhook")]
public class CmsWebhookController : ControllerBase
{
    private readonly ICmsService _cmsService;
    private readonly IOptions<OrchardCoreCmsOptions> _options;

    public CmsWebhookController(ICmsService cmsService, IOptions<OrchardCoreCmsOptions> options)
    {
        _cmsService = cmsService;
        _options = options;
    }

    [HttpPost("cache-invalidate")]
    public async Task<IActionResult> Invalidate([FromHeader(Name = "X-Cms-Signature")] string? signature, [FromBody] CmsWebhookPayload? payload)
    {
        if (string.IsNullOrWhiteSpace(signature) || signature != _options.Value.WebhookSecret)
        {
            return Unauthorized("Invalid CMS webhook secret signature.");
        }

        await _cmsService.InvalidateArticleCacheAsync(payload?.Slug);
        return Ok(new { message = "Cache successfully invalidated." });
    }

    public record CmsWebhookPayload(string? Event, string? Slug);
}
