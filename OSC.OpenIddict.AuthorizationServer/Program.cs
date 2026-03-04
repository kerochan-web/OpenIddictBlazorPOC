using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Abstractions;
using OSC.OpenIddict.AuthorizationServer.Data;
using OSC.OpenIddict.AuthorizationServer.DbContext;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Add services to the container
// ------------------------------------------------------------

builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// ------------------------------------------------------------
// Database (REQUIRED for OpenIddict)
// ------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()
);

    // REQUIRED: registers OpenIddict EF Core entities
    options.UseOpenIddict();
});


// ------------------------------------------------------------
// ASPNET Core Identity
// ------------------------------------------------------------

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();


// ------------------------------------------------------------
// OpenIddict
// ------------------------------------------------------------
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetEndSessionEndpointUris("/connect/logout")
               .SetUserInfoEndpointUris("/connect/userinfo");

        options.AllowPasswordFlow() // DEV ONLY
               .AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        //TODO: add If statement to handle this properly
        // DEV ONLY: replace in production
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
           .EnableAuthorizationEndpointPassthrough()
           .EnableTokenEndpointPassthrough()
           .EnableEndSessionEndpointPassthrough()
           .EnableUserInfoEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ------------------------------------------------------------
// Configure the HTTP request pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapDefaultControllerRoute();

//execute database seeding
try 
{
    await DatabaseSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    // This provides a clear error in the console/logs why the app didn't start
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database seeding failed. The application is shutting down.");
    throw; 
}

app.Run();


//TODO: implement this logic on apis that need to validate access tokens
//// API Resource Validation
//services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

//services.AddAuthorization(options => {
//    options.AddPolicy("AdminOnly", policy =>
//        policy.RequireRole("Admin"));
//});
