using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Web.Controllers;

[Route("[controller]/[action]")]
public class AccountController : ControllerBase
{
    public IDataProtectionProvider provider { get; }

    public AccountController(IDataProtectionProvider _provider)
        => provider = _provider;

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
        => Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Discord");
    
    [HttpGet]
    public async Task<IActionResult> LogOut(string returnUrl = "/")
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect(returnUrl);
    }
}