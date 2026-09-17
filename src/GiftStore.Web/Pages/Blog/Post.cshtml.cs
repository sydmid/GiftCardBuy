namespace GiftStore.Web.Pages.Blog;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Dtos;

public class PostModel : PageModel
{
    private readonly ICmsService _cmsService;

    public PostModel(ICmsService cmsService)
    {
        _cmsService = cmsService;
    }

    public CmsArticleDto? Article { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToPage("/Blog/Index");

        Article = await _cmsService.GetArticleBySlugAsync(slug);
        if (Article == null) return NotFound();

        return Page();
    }
}
