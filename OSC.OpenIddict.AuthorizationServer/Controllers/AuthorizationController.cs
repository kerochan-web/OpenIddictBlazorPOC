using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OSC.OpenIddict.AuthorizationServer.Controllers;

public class AuthorizationController : Controller
{

    [HttpGet("/")]
    public IActionResult Index()
    {
        return Redirect("/Identity/Account/Login");
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // 1. Try to get the user principal from the local cookie
        // You may need to add: using Microsoft.AspNetCore.Identity;
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // 2. If the user is NOT logged in, redirect them to the Login page
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.Query.ToList())
                });
        }

        // 3. Create the claims principal for the token
        var claims = new List<Claim>
        {
            new Claim(OpenIddictConstants.Claims.Subject, result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            new Claim(OpenIddictConstants.Claims.Name, result.Principal.Identity.Name)
        };

        // Important: Add roles so your [Authorize(Roles = "Admin")] works later
        foreach (var role in result.Principal.FindAll(ClaimTypes.Role))
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, role.Value));
        }

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // 4. Set the scopes (must match what the client requested)
        claimsPrincipal.SetScopes(request.GetScopes());

        // 5. Sign in the user into OpenIddict
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            // Create a new claims principal because the internal one might have internal-only claims.
            // This ensures the token contains exactly what it needs.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }
}
