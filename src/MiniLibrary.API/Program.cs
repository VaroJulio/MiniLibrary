using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniLibrary.API.Configuration;
using MiniLibrary.API.Middleware;
using MiniLibrary.API.Services;
using MiniLibrary.Application;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS — allow the frontend origin(s) with credentials (HttpOnly cookies)
// Supports comma-separated origins in App:FrontendUrl for multiple environments
var frontendUrlConfig = builder.Configuration["App:FrontendUrl"] ?? "http://localhost:3000";
var allowedOrigins = frontendUrlConfig
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(url => url.TrimEnd('/'))
    .ToArray();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-CSRF-TOKEN", "X-Correlation-Id");
    });
});

// HTTP context accessor and current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Configure Authentication: JWT Bearer (default) + Google + Microsoft OAuth
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret must be configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MiniLibrary";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MiniLibrary";

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // Transient cookie used only to carry the external login info during the OAuth flow
    options.Cookie.Name = ".MiniLibrary.ExternalAuth";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };

    // Read JWT from HttpOnly cookie when no Authorization header is present
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // If no Authorization header, try the access_token cookie
            if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                var accessToken = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Only register OAuth providers if credentials are configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var msClientId = builder.Configuration["Authentication:Microsoft:ClientId"];

if (!string.IsNullOrEmpty(googleClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
        options.SaveTokens = true;
        // Force the correct public URL for the OAuth callback (Docker port mapping issue)
        var publicUrl = builder.Configuration["App:PublicUrl"] ?? "http://localhost:5000";
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            // Replace the redirect_uri with the correct public URL
            var uri = context.RedirectUri;
            // The default redirect_uri may have wrong host/port, fix it
            var signinPath = "/signin-google";
            var correctRedirectUri = publicUrl + signinPath;
            uri = System.Text.RegularExpressions.Regex.Replace(
                uri,
                @"redirect_uri=[^&]+",
                $"redirect_uri={Uri.EscapeDataString(correctRedirectUri)}");
            // Always show account selector so user can switch accounts
            uri += "&prompt=select_account";
            context.Response.Redirect(uri);
            return Task.CompletedTask;
        };
    });
}

if (!string.IsNullOrEmpty(msClientId))
{
    authBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = msClientId;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? string.Empty;
        options.SaveTokens = true;
        // Force correct public URL and always show account selector
        var publicUrl = builder.Configuration["App:PublicUrl"] ?? "http://localhost:5000";
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var uri = context.RedirectUri;
            var signinPath = "/signin-microsoft";
            var correctRedirectUri = publicUrl + signinPath;
            uri = System.Text.RegularExpressions.Regex.Replace(
                uri,
                @"redirect_uri=[^&]+",
                $"redirect_uri={Uri.EscapeDataString(correctRedirectUri)}");
            uri += "&prompt=select_account";
            context.Response.Redirect(uri);
            return Task.CompletedTask;
        };
    });
}

// Configure role-based authorization policies (Req 7.4)
builder.Services.AddAuthorizationPolicies();

// Custom authorization result handler for RFC 7807 ProblemDetails responses (Req 6.6)
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI with JWT bearer auth scheme and XML comments
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiniLibrary API",
        Version = "v1",
        Description = "Library management system API with AI-powered search and recommendations."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments from API project
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Apply pending EF Core migrations on startup (idempotent — skips if already applied)
// Skipped when using non-relational providers (e.g., InMemory in integration tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MiniLibrary.Infrastructure.Data.AppDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();

        // Data migration: rename badge types from Spanish to English (idempotent)
        await db.Database.ExecuteSqlRawAsync(@"
            UPDATE [Badges] SET [BadgeType] = 'FirstLoan' WHERE [BadgeType] = 'PrimerPrestamo';
            UPDATE [Badges] SET [BadgeType] = 'NoviceReader' WHERE [BadgeType] = 'LectorNovato';
            UPDATE [Badges] SET [BadgeType] = 'AvidReader' WHERE [BadgeType] = 'LectorAvido';
            UPDATE [Badges] SET [BadgeType] = 'ExpertReader' WHERE [BadgeType] = 'LectorExperto';
            UPDATE [Badges] SET [BadgeType] = 'Centenarian' WHERE [BadgeType] = 'Centenario';
            UPDATE [Badges] SET [BadgeType] = 'LiteraryCritic' WHERE [BadgeType] = 'CriticoLiterario';
            UPDATE [Badges] SET [BadgeType] = 'CommunityVoice' WHERE [BadgeType] = 'VozDeLaComunidad';
            UPDATE [Badges] SET [BadgeType] = 'Explorer' WHERE [BadgeType] = 'Explorador';
            UPDATE [Badges] SET [BadgeType] = 'Polymath' WHERE [BadgeType] = 'Polimata';
            UPDATE [Badges] SET [BadgeType] = 'Punctual' WHERE [BadgeType] = 'Puntual';
            UPDATE [Badges] SET [BadgeType] = 'ReaderOfTheMonth' WHERE [BadgeType] = 'LectorDelMes';
        ");
    }
}

// Configure the HTTP request pipeline

// Forward headers from reverse proxy (Docker port mapping, nginx)
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Correlation ID must be first so all subsequent middleware/handlers have access
app.UseMiddleware<CorrelationIdMiddleware>();

// Error handling wraps everything below it
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniLibrary API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseMiddleware<MiniLibrary.API.Middleware.CsrfProtectionMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint for Docker and load balancers
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .ExcludeFromDescription();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
