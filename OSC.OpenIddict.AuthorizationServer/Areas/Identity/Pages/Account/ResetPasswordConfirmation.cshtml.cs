#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OSC.OpenIddict.AuthorizationServer.Areas.Identity.Pages.Account
{
[AllowAnonymous]
public class ResetPasswordConfirmationModel : PageModel
{
    public string ReturnUrl { get; set; }

    public void OnGet(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }
}
}
