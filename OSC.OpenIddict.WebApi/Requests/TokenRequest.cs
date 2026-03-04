namespace OSC.OpenIddict.WebApi.Models
{
    public record TokenRequest(
        string GrantType,
        string ClientId,
        string Code,
        string RedirectUri,
        string? CodeVerifier,
        string? RefreshToken
    );
}
