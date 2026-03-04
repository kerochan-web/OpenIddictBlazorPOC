using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OSC.OpenIddict.WebApi.Configuration;
using OSC.OpenIddict.WebApi.Models;
using OSC.OpenIddict.WebApi.Security.OSC.OpenIddict.WebApi.Security;
using System.Security.Cryptography;
using System.Text;

namespace OSC.OpenIddict.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenIdConfigurationOptions _options;

        // DEV-ONLY PKCE storage (replace with distributed cache in prod)
        private static readonly Dictionary<string, string> _pkceStore = new();

        public SecurityController(
            IHttpClientFactory httpClientFactory,
            IOptions<OpenIdConfigurationOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        // ------------------------------------------------------------
        // 1. OpenID Discovery
        // ------------------------------------------------------------
        [HttpGet("openid-configuration")]
        public async Task<IActionResult> GetOpenIdConfiguration()
        {
            var client = _httpClientFactory.CreateClient();

            var url =
                $"{_options.Authority.TrimEnd('/')}/.well-known/openid-configuration";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return Content(
                await response.Content.ReadAsStringAsync(),
                "application/json");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        // ------------------------------------------------------------
        // 2. Start Authorization Code + PKCE
        // ------------------------------------------------------------
        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            var codeVerifier = PkceHelper.GenerateCodeVerifier();
            var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);

            var state = Guid.NewGuid().ToString("N");
            _pkceStore[state] = codeVerifier;

            var authorizeUrl =
                $"{_options.Authority.TrimEnd('/')}/connect/authorize" +
                $"?client_id={_options.ClientId}" +
                $"&response_type=code" +
                $"&scope=openid profile offline_access api" +
                $"&redirect_uri={Uri.EscapeDataString(_options.CallbackUrl)}" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method=S256" +
                $"&state={state}";

            return Redirect(authorizeUrl);
        }

        // ------------------------------------------------------------
        // 3. Authorization Callback (receives code)
        // ------------------------------------------------------------
        [HttpGet("callback")]
        public IActionResult Callback(
            [FromQuery] string code,
            [FromQuery] string state)
        {
            if (!_pkceStore.TryGetValue(state, out var codeVerifier))
                return BadRequest("Invalid or expired state.");

            _pkceStore.Remove(state);

            return Ok(new
            {
                code,
                codeVerifier
            });
        }

        // ------------------------------------------------------------
        // 4. Token Exchange (Authorization Code → Tokens)
        // ------------------------------------------------------------
        [HttpPost("token")]
        public async Task<IActionResult> ExchangeToken([FromBody] TokenRequest request)
        {
            var client = _httpClientFactory.CreateClient();

            var tokenEndpoint =
                $"{_options.Authority.TrimEnd('/')}/connect/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = request.ClientId,
                ["code"] = request.Code,
                ["redirect_uri"] = request.RedirectUri,
                ["code_verifier"] = request.CodeVerifier
            };

            var response = await client.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(form));

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        // ------------------------------------------------------------
        // 5. Refresh Token
        // ------------------------------------------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh token is required.");

            var client = _httpClientFactory.CreateClient();

            var tokenEndpoint =
                $"{_options.Authority.TrimEnd('/')}/connect/token";

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = request.ClientId,
                ["refresh_token"] = request.RefreshToken
            };

            var response = await client.PostAsync(
                tokenEndpoint,
                new FormUrlEncodedContent(form));

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        // ------------------------------------------------------------
        // 6. Logout (front-channel helper)
        // ------------------------------------------------------------
        [HttpPost("logout")]
        public IActionResult Logout([FromQuery] string idTokenHint)
        {
            var logoutUrl =
                $"{_options.Authority.TrimEnd('/')}/connect/logout" +
                $"?id_token_hint={Uri.EscapeDataString(idTokenHint)}";

            return Ok(new { logoutUrl });
        }

        //// ------------------------------------------------------------
        //// PKCE Helpers
        //// ------------------------------------------------------------
        //private static string GenerateCodeVerifier()
        //{
        //    var bytes = new byte[32];
        //    RandomNumberGenerator.Fill(bytes);
        //    return Base64UrlEncode(bytes);
        //}

        //private static string GenerateCodeChallenge(string codeVerifier)
        //{
        //    using var sha256 = SHA256.Create();
        //    var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        //    return Base64UrlEncode(hash);
        //}

        //private static string Base64UrlEncode(byte[] input)
        //{
        //    return Convert.ToBase64String(input)
        //        .TrimEnd('=')
        //        .Replace('+', '-')
        //        .Replace('/', '_');
        //}
    }
}
