namespace GiftStore.Web.Pages.Account;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // Simple authentication check for local demo/admin
        bool isAdmin = string.Equals(Email, "admin@giftcardbuy.local", StringComparison.OrdinalIgnoreCase);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Email),
            new(ClaimTypes.Name, Email),
            new(ClaimTypes.Email, Email),
            new(ClaimTypes.Role, isAdmin ? "Admin" : "Customer")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToPage(isAdmin ? "/Admin/Index" : "/Account/Orders");
    }
}
