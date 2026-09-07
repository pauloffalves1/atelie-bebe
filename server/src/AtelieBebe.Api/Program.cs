using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using AtelieBebe.Api.Endpoints;
using AtelieBebe.Api.Health;
using AtelieBebe.Api.Middleware;
using AtelieBebe.Application;
using AtelieBebe.Infrastructure;
using AtelieBebe.Infrastructure.Persistence;
using AtelieBebe.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuração 'Jwt' ausente em appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(JwtTokenGenerator.AdminRole))
    .AddPolicy("CustomerOnly", policy => policy.RequireRole(JwtTokenGenerator.CustomerRole));

const string CorsPolicyName = "AtelieBebeCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// Nginx (production) sits in front of Kestrel on the same host — without this, every request's
// RemoteIpAddress would be Nginx's own loopback address, making the per-IP rate limiter below
// useless (it would lump every real visitor into a single bucket instead of one per client).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

// Brute-force protection on the handful of endpoints where a wrong secret guess is the attack
// (password login, password-reset token, account-deletion password confirmation) — 5 attempts per
// minute per client IP *per endpoint*, no queueing (the 6th attempt in the window is rejected
// immediately with 429). The path is part of the partition key so, e.g., failed admin-login
// attempts never also lock a customer out of their own unrelated login or coupon check.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Request.Path}",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Under /api/ so it rides the same Nginx proxy rule as everything else in production — a bare
// /health would fall through to the Angular SPA's static-file fallback instead of reaching Kestrel.
app.MapHealthChecks("/api/health");

var uploadsPath = builder.Configuration["Uploads:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = builder.Configuration["Uploads:PublicPath"] ?? "/api/uploads",
});

app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapDashboardEndpoints();
app.MapContactEndpoints();
app.MapCustomerEndpoints();
app.MapSiteImageEndpoints();
app.MapGalleryEndpoints();
app.MapPaymentEndpoints();
app.MapSitemapEndpoints();
app.MapReviewEndpoints();
app.MapCouponEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapFakePaymentEndpoints();
}

await DbInitializer.InitializeAsync(app.Services);

app.Run();
