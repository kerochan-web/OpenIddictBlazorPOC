using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OSC.OpenIddict.AuthorizationServer.DbContext;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OSC.OpenIddict.AuthorizationServer.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
// Ensures the database schema is up to date based on your EF Migrations
await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            // ----------------------------
            // Roles
            // ----------------------------
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new ApplicationRole { Name = "Member" });

            // ----------------------------
            // Admin User
            // ----------------------------
            var admin = await userManager.FindByEmailAsync("admin@local.test");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin@local.test",
                    Email = "admin@local.test",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ----------------------------
            // Member User
            // ----------------------------
            var member = await userManager.FindByEmailAsync("member@local.test");
            if (member == null)
            {
                member = new ApplicationUser
                {
                    UserName = "member@local.test",
                    Email = "member@local.test",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(member, "Member123!");
                await userManager.AddToRoleAsync(member, "Member");
            }

            // ----------------------------
            // OpenIddict Scopes
            // ----------------------------
if (await scopeManager.FindByNameAsync("api") == null)
{
    var descriptor = new OpenIddictScopeDescriptor
    {
        Name = "api",
        DisplayName = "Main API access",
    };
    descriptor.Resources.Add("resource_server");
    await scopeManager.CreateAsync(descriptor);
}

            // ADD THESE BLOCKS:
            if (await scopeManager.FindByNameAsync("profile") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "profile",
                    DisplayName = "Profile access",
                });
            }

            if (await scopeManager.FindByNameAsync("email") == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "email",
                    DisplayName = "Email access",
                });
            }

<<<<<<< HEAD
            // ----------------------------
            // OpenIddict Client (Blazor / MAUI)
            // ----------------------------
            if (await appManager.FindByClientIdAsync("blazor-client") == null)
            {
                await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "blazor-client",
                    ClientType = "public",
                    //THREE PARAGRAPHS BELOW TO BE UNCOMMENTED                    
                    //ClientType = ClientTypes.Public,
                    //ConsentType = ConsentTypes.Implicit,

                    RedirectUris =
                    {
                        new Uri("https://localhost:7109/signin-oidc")
                    },

                    PostLogoutRedirectUris =
                    {
                        new Uri("https://localhost:7109/signout-callback-oidc")
                    },
=======
if (await scopeManager.FindByNameAsync(Scopes.OpenId) == null)
{
    await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
    {
        Name = Scopes.OpenId,
        DisplayName = "OpenID scope"
    });
}

// ----------------------------
// OpenIddict Client (Blazor / MAUI)
// ----------------------------
const string blazorClientId = "blazor-client";

// Find existing client to ensure we always start fresh
var existingApp = await appManager.FindByClientIdAsync(blazorClientId);
if (existingApp != null)
{
    await appManager.DeleteAsync(existingApp);
}

// Create the client fresh every time
await appManager.CreateAsync(new OpenIddictApplicationDescriptor
{
    ClientId = blazorClientId,
    ClientType = "public",                  
    DisplayName = "Blazor Client App",
>>>>>>> 65876b0 (Make OpenIddict client seeding deterministic)

    RedirectUris =
    {
        new Uri("https://localhost:7109/signin-oidc"), 
        new Uri("https://localhost:7856/authentication/login-callback") 
    },

<<<<<<< HEAD
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,//allow access to the authorization endpoint
                        Permissions.Endpoints.Token,//allow access to the token endpoint
                        Permissions.GrantTypes.AuthorizationCode,//allow authorization code flow
                        Permissions.GrantTypes.RefreshToken,//allow refresh tokens
                        Permissions.GrantTypes.Password,// DEV ONLY
                        Permissions.ResponseTypes.Code,//allow response type code
                        Permissions.Prefixes.Scope + Scopes.OpenId,//mandatory
                        Permissions.Prefixes.Scope + Scopes.Profile,//optional
                        Permissions.Prefixes.Scope + Scopes.Email,//optional
                        Permissions.Prefixes.Scope + Scopes.OfflineAccess,//for refresh tokens
                        Permissions.Prefixes.Scope + "api"//custom api scope
                    },
=======
    PostLogoutRedirectUris =
    {
        new Uri("https://localhost:7109/signout-callback-oidc"),
        new Uri("https://localhost:7856/")
    },
>>>>>>> 65876b0 (Make OpenIddict client seeding deterministic)

    Permissions =
    {
        Permissions.Endpoints.Authorization,
        Permissions.Endpoints.Token,
        Permissions.Endpoints.EndSession,
        Permissions.GrantTypes.AuthorizationCode,
        Permissions.GrantTypes.RefreshToken,
        Permissions.GrantTypes.Password, // DEV ONLY
        Permissions.ResponseTypes.Code,
        Permissions.Prefixes.Scope + Scopes.OpenId,
        Permissions.Prefixes.Scope + Scopes.Profile,
        Permissions.Prefixes.Scope + Scopes.Email,
        Permissions.Prefixes.Scope + Scopes.OfflineAccess,
        Permissions.Prefixes.Scope + "api"
    },

    Requirements =
    {
        Requirements.Features.ProofKeyForCodeExchange
    }
});

// Fail-fast check: Verify the client was actually created
if (await appManager.FindByClientIdAsync(blazorClientId) == null)
{
    throw new InvalidOperationException("CRITICAL: Failed to recreate the 'blazor-client' during startup.");
}

// ----------------------------
// Fail-Fast Validation
// ----------------------------
var blazorClient = await appManager.FindByClientIdAsync("blazor-client");
if (blazorClient == null)
{
    throw new InvalidOperationException("CRITICAL: The 'blazor-client' could not be seeded. Application startup aborted.");
}

        }
    }
}