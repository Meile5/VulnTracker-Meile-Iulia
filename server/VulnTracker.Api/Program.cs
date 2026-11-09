using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using VulnTracker.Api.Authorization;
using VulnTracker.Application.Interfaces;
using VulnTracker.Application.Services;
using VulnTracker.Infrastructure;
using VulnTracker.Infrastructure.Database;
using VulnTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<FindingService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<SlaService>();

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<ISlaRepository, SlaRepository>();

// --- Angular CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- Authentication ---
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var kc = builder.Configuration.GetSection("Authentication:Keycloak");

        options.Authority = kc["Authority"];
        // Only set in Docker: the container reaches Keycloak as keycloak:8080,
        // while the issuer stays localhost:8081. Locally this is left empty.
        if (!string.IsNullOrEmpty(kc["MetadataAddress"]))
            options.MetadataAddress = kc["MetadataAddress"];

        options.ClientId = kc["ClientId"];
        options.ClientSecret = kc["ClientSecret"];

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = false; // dev only (plain http)

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = "role";

        // OIDC cookies default to SameSite=None, which browsers reject over plain http
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            // API calls get 401 instead of a redirect to Keycloak
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.HandleResponse();
            }
            return Task.CompletedTask;
        };

        // Keycloak puts realm roles in the access token under realm_access.roles.
        // Copy them to "role" claims so [Authorize(Roles/Policy)] works.
        // (If you add a realm-role mapper with claim name "role", this is skipped.)
        options.Events.OnTokenValidated = ctx =>
        {
            if (ctx.Principal?.Identity is not ClaimsIdentity identity)
                return Task.CompletedTask;

            if (identity.HasClaim(c => c.Type == "role"))
                return Task.CompletedTask;

            var accessToken = ctx.TokenEndpointResponse?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
                return Task.CompletedTask;

            var jwt = new JsonWebToken(accessToken);
            if (jwt.TryGetPayloadValue<JsonElement>("realm_access", out var realmAccess) &&
                realmAccess.ValueKind == JsonValueKind.Object &&
                realmAccess.TryGetProperty("roles", out var roles) &&
                roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var value = role.GetString();
                    if (!string.IsNullOrEmpty(value))
                        identity.AddClaim(new Claim("role", value));
                }
            }
            return Task.CompletedTask;
        };
    });

// --- Authorization ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanViewFindings,  p => p.RequireRole(Roles.Analyst, Roles.Developer, Roles.PM, Roles.Auditor))
    .AddPolicy(Policies.CanCreateFinding, p => p.RequireRole(Roles.Analyst, Roles.PM))
    .AddPolicy(Policies.CanUpdateFinding, p => p.RequireRole(Roles.Analyst, Roles.Developer, Roles.PM))
    .AddPolicy(Policies.CanAssignFinding, p => p.RequireRole(Roles.Analyst, Roles.PM))
    .AddPolicy(Policies.CanSetSla,        p => p.RequireRole(Roles.PM))
    .AddPolicy(Policies.CanManageUsers,   p => p.RequireRole(Roles.PM))
    .AddPolicy(Policies.CanViewAuditLogs, p => p.RequireRole(Roles.Auditor, Roles.PM));

builder.Services.AddSingleton<IAuthorizationHandler, FindingAuthorizationHandler>();

// --- Rate limiting ---
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // 5 reports per minute per IP
    o.AddPolicy("public-form", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHttpClient<IKeycloakAdminClient, VulnTracker.Infrastructure.Keycloak.KeycloakAdminClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Angular");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
