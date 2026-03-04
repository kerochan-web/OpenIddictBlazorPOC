using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Validation.AspNetCore;
using OSC.OpenIddict.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Add services to the container
// ------------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

// Define [Authorize(Policy = "AdminOnly")]

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Note: The WebApi "notes" who the issuer is to validate the signature
        options.SetIssuer("http://localhost:5181/"); // Change to your Auth Server URL
        options.AddAudiences("osc_web_api");

        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient (for calling OpenID discovery endpoint)
builder.Services.AddHttpClient();

// Strongly-typed configuration
builder.Services.Configure<OpenIdConfigurationOptions>(
    builder.Configuration.GetSection(OpenIdConfigurationOptions.SectionName));

var app = builder.Build();

// ------------------------------------------------------------
// Configure the HTTP request pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "OSC OpenIddict Web API v1");

        options.RoutePrefix = "swagger"; // default, explicit for clarity
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
