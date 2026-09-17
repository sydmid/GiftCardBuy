namespace GiftStore.Web.Controllers;

using System.Text;
using Microsoft.AspNetCore.Mvc;
using GiftStore.Application.Interfaces;

[ApiController]
public class FeedController : ControllerBase
{
    private readonly ICmsService _cmsService;

    public FeedController(ICmsService cmsService)
    {
        _cmsService = cmsService;
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> GetSitemap()
    {
        var articles = await _cmsService.GetPublishedArticlesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version="1.0" encoding="UTF-8"?>");
        sb.AppendLine("<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">");

        // Static routes
        sb.AppendLine("  <url><loc>https://giftcardbuy.ir/</loc><priority>1.0</priority></url>");
        sb.AppendLine("  <url><loc>https://giftcardbuy.ir/Blog</loc><priority>0.8</priority></url>");

        foreach (var art in articles)
        {
            sb.AppendLine($"  <url><loc>https://giftcardbuy.ir/Blog/Post?slug={art.Slug}</loc><lastmod>{art.PublishedDateUtc:yyyy-MM-dd}</lastmod><priority>0.7</priority></url>");
        }

        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpGet("feed.xml")]
    public async Task<IActionResult> GetRss()
    {
        var articles = await _cmsService.GetPublishedArticlesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version="1.0" encoding="utf-8"?>");
        sb.AppendLine("<rss version="2.0">");
        sb.AppendLine("  <channel>");
        sb.AppendLine("    <title>مجله گیفتی‌کارت‌بای</title>");
        sb.AppendLine("    <link>https://giftcardbuy.ir/Blog</link>");
        sb.AppendLine("    <description>راهنماها و آموزش‌های تخصصی خرید و شارژ گیفت کارت‌های ارزی</description>");

        foreach (var art in articles)
        {
            sb.AppendLine("    <item>");
            sb.AppendLine($"      <title><![CDATA[{art.Title}]]></title>");
            sb.AppendLine($"      <link>https://giftcardbuy.ir/Blog/Post?slug={art.Slug}</link>");
            sb.AppendLine($"      <description><![CDATA[{art.Excerpt}]]></description>");
            sb.AppendLine($"      <pubDate>{art.PublishedDateUtc:R}</pubDate>");
            sb.AppendLine("    </item>");
        }

        sb.AppendLine("  </channel>");
        sb.AppendLine("</rss>");
        return Content(sb.ToString(), "application/rss+xml", Encoding.UTF8);
    }
}
