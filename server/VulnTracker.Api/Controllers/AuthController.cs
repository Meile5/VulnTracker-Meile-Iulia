using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VulnTracker.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private const string AngularOrigin = "http://localhost:4200";

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        // only allow sending people back to the Angular app, so this can't be used as an open redirect
        var target = Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) && uri.GetLeftPart(UriPartial.Authority) == AngularOrigin
            ? uri.ToString()
            : $"{AngularOrigin}/home";

        return Challenge(
            new AuthenticationProperties { RedirectUri = target },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "http://localhost:4200/login" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    // AllowAnonymous so Angular gets a plain 401 when logged out, instead of a redirect to Keycloak
    [AllowAnonymous]
    [HttpGet("me")]
    public IActionResult Me()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Unauthorized();

        return Ok(new
        {
            id = User.FindFirst("sub")?.Value,
            username = User.Identity.Name,
            roles = User.Claims
                .Where(c => c.Type is "role" or ClaimTypes.Role)
                .Select(c => c.Value)
        });
    }
}
