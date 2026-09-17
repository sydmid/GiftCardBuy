namespace GiftStore.Web.Pages.Blog;

using Microsoft.AspNetCore.Mvc.RazorPages;
using GiftStore.Application.Interfaces;
using GiftStore.Contracts.Dtos;

public class IndexModel : PageModel
{
    private readonly ICmsService _cmsService;

    public IndexModel(ICmsService cmsService)
    {
        _cmsService = cmsService;
    }

    public IReadOnlyList<CmsArticleDto> Articles { get; set; } = [];

    public async Task OnGetAsync()
    {
        Articles = await _cmsService.GetPublishedArticlesAsync();
    }
}
