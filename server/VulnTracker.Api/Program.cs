using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using VulnTracker.Api.Authorization;
using VulnTracker.Application.Services;
using VulnTracker.Infrastructure;
using VulnTracker.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<FindingService>();

// --- Angular CORS---
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
            ctx.Response.StatusCode = 403; 
            return Task.CompletedTask;
        };
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var kc = builder.Configuration.GetSection("Authentication:Keycloak");
        options.Authority = kc["Authority"];
        options.ClientId = kc["ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Keycloak:ClientSecret"];

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = false; 

        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.TokenValidationParameters.RoleClaimType = "role";
        
        options.MapInboundClaims = false;
        
        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = 401; 
                ctx.HandleResponse();
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanViewFindings,  p => p.RequireRole(Roles.Analyst, Roles.Developer, Roles.PM, Roles.Auditor))
    .AddPolicy(Policies.CanCreateFinding, p => p.RequireRole(Roles.Analyst, Roles.PM))
    .AddPolicy(Policies.CanUpdateFinding, p => p.RequireRole(Roles.Analyst, Roles.Developer, Roles.PM))
    .AddPolicy(Policies.CanAssignFinding, p => p.RequireRole(Roles.Analyst, Roles.PM))
    .AddPolicy(Policies.CanSetSla,        p => p.RequireRole(Roles.PM))
    .AddPolicy(Policies.CanManageUsers,   p => p.RequireRole(Roles.PM))
    .AddPolicy(Policies.CanViewAuditLogs, p => p.RequireRole(Roles.Auditor));

builder.Services.AddSingleton<IAuthorizationHandler, FindingAuthorizationHandler>();

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

app.UseHttpsRedirection();

app.UseCors("Angular"); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

public partial class Program;