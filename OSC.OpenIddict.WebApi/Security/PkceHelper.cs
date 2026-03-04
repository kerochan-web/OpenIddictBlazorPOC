namespace OSC.OpenIddict.WebApi.Security
{
    using System.Security.Cryptography;
    using System.Text;

    namespace OSC.OpenIddict.WebApi.Security
    {
        public static class PkceHelper
        {
            public static string GenerateCodeVerifier()
            {
                var bytes = new byte[32];
                RandomNumberGenerator.Fill(bytes);
                return Base64UrlEncode(bytes);
            }

            public static string GenerateCodeChallenge(string codeVerifier)
            {
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
                return Base64UrlEncode(hash);
            }

            private static string Base64UrlEncode(byte[] input)
            {
                return Convert.ToBase64String(input)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }

}
