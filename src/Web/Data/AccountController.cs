using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Discord.OAuth2;

namespace Web.Data;

[Route("[controller]/[action]")]
public class AccountController : ControllerBase
{
    public IDataProtectionProvider Provider { get; }

    public AccountController(IDataProtectionProvider provider)
    {
        Provider = provider;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties {RedirectUri = returnUrl}, "Discord");
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl);
    }
}