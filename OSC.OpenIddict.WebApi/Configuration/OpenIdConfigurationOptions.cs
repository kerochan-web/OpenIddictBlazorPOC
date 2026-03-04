namespace OSC.OpenIddict.WebApi.Configuration
{
    public class OpenIdConfigurationOptions
    {
        public const string SectionName = "OpenId";

        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
