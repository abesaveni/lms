using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure;
using LiveExpert.API.Middleware;
using LiveExpert.API.Hubs;
using LiveExpert.API.Services;
using LiveExpert.API.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Reflection;
using FluentValidation;
using DotNetEnv;

// Load environment variables from .env file BEFORE creating the builder
// This ensures environment variables are available to the configuration system
// Try multiple paths: 
// 1. Backend root (2 levels up from API project): LMS-Backend/.env
// 2. Application root (3 levels up): LMS _ Application/.env
// 3. Current directory: LiveExpert.API/.env

var currentDir = Directory.GetCurrentDirectory();
var envPath1 = Path.Combine(currentDir, "..", "..", ".env"); // LMS-Backend/.env
var envPath2 = Path.Combine(currentDir, "..", "..", "..", ".env"); // LMS _ Application/.env
var envPath3 = Path.Combine(currentDir, ".env"); // Current directory

var envPaths = new[] 
{ 
    Path.GetFullPath(envPath1),
    Path.GetFullPath(envPath2),
    Path.GetFullPath(envPath3)
};

string? loadedEnvPath = null;
foreach (var envPath in envPaths)
{
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
        loadedEnvPath = envPath;
        Console.WriteLine($"✓ Loaded .env file from: {envPath}");
        break;
    }
}

if (loadedEnvPath == null)
{
    Console.WriteLine($"⚠ Warning: .env file not found. Tried: {string.Join(", ", envPaths)}");
    Console.WriteLine("⚠ Using default configuration values.");
}

// Verify JWT key is loaded
var jwtKeyFromEnv = Environment.GetEnvironmentVariable("JWT__KEY");
if (!string.IsNullOrEmpty(jwtKeyFromEnv))
{
    Console.WriteLine($"✓ JWT key loaded from environment (length: {jwtKeyFromEnv.Length})");
}
else
{
    Console.WriteLine("⚠ Warning: JWT__KEY not found in environment variables. Will use fallback.");
}

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
});

// .NET automatically converts environment variables with double underscores (__) to colons (:) in configuration
// So JWT__KEY becomes accessible as builder.Configuration["Jwt:Key"]

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/liveexpert-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add Infrastructure services (Database, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.Load("LiveExpert.Application"));
});

// Configure FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("LiveExpert.Application"));

// Configure CORS — in dev, allow any localhost port (Vite shifts ports dynamically)
builder.Services.AddCors(options =>
{
    void ApplyLocalhostPolicy(Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder p, bool includeProduction = false)
    {
        var origins = new List<string>();
        // Allow all common localhost ports used by Vite / React dev servers
        for (int port = 3000; port <= 9999; port++)
            origins.Add($"http://localhost:{port}");

        if (includeProduction)
        {
            origins.Add("https://liveexpert.ai");
            origins.Add("https://www.liveexpert.ai");
        }

        p.WithOrigins(origins.ToArray())
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()
         .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
         .WithExposedHeaders("*");
    }

    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
            ApplyLocalhostPolicy(policy);
        else
            ApplyLocalhostPolicy(policy, includeProduction: true);
    });

    options.AddPolicy("AllowAllDev",  policy => ApplyLocalhostPolicy(policy));
    options.AddPolicy("AllowAll",     policy => ApplyLocalhostPolicy(policy, includeProduction: true));
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LiveExpert.API",
        Version = "v1",
        Description = "LiveExpert Learning Management System API"
    });

    // Use full type names to avoid duplicate schema IDs
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

// Configure Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // Try multiple ways to get JWT key
        // .NET automatically converts JWT__KEY env var to Jwt:Key in configuration
        var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY")
            ?? builder.Configuration["Jwt:Key"]
            ?? builder.Configuration["JWT__KEY"]
            ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"; // Fallback for development
        
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            // Use fallback if key is too short
            jwtKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            Console.WriteLine($"⚠ Warning: JWT key is empty or too short. Using default key. Please set JWT__KEY in .env file.");
        }
        else
        {
            Console.WriteLine($"✓ JWT key loaded successfully (length: {jwtKey.Length})");
        }

        var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
            ?? builder.Configuration["JWT__ISSUER"]
            ?? Environment.GetEnvironmentVariable("JWT__ISSUER")
            ?? "LiveExpert.AI";
        
        var jwtAudience = builder.Configuration["Jwt:Audience"] 
            ?? builder.Configuration["JWT__AUDIENCE"]
            ?? Environment.GetEnvironmentVariable("JWT__AUDIENCE")
            ?? "LiveExpert.AI.Users";

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };

        // Configure SignalR authentication
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/hubs"))
                {
                    // Try query string first (for WebSocket connections and URL-embedded tokens)
                    // SignalR sends token as access_token in query string for negotiation
                    var accessTokenQuery = context.Request.Query["access_token"];
                    if (accessTokenQuery.Count > 0 && !string.IsNullOrWhiteSpace(accessTokenQuery[0]))
                    {
                        context.Token = accessTokenQuery[0];
                        return Task.CompletedTask;
                    }
                    
                    // Try Authorization header (for negotiation requests)
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        return Task.CompletedTask;
                    }
                    
                    // No token found - this will result in 401, which is expected for anonymous requests
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure SignalR
builder.Services.AddSignalR();

// Configure HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Health Checks
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<GeminiAIService>();
builder.Services.AddHttpClient<ClaudeAIService>();
builder.Services.AddScoped<ClaudeAIService>();
builder.Services.AddScoped<LMSAIService>();
builder.Services.AddScoped<ResumeService>();
builder.Services.AddSingleton<ResumePdfService>();
builder.Services.AddScoped<ChatbotService>();

// Background service: releases tutor earnings from Pending → Available after 3-day hold
builder.Services.AddHostedService<EarningsReleaseService>();


var app = builder.Build();

// Configure the HTTP request pipeline

// Trust X-Forwarded-Proto/Host from nginx so Request.Scheme is https in production
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

// CORS MUST be first - before any other middleware that might write responses
// Use explicit policy to support SignalR credentials
var corsPolicyName = app.Environment.IsDevelopment() ? "AllowAllDev" : "AllowAll";
app.UseCors(corsPolicyName);
Console.WriteLine($"✓ CORS: Enabled - Environment: {app.Environment.EnvironmentName}");

// Custom Middleware (Order matters!)
app.UseExceptionHandling(); // Catch all exceptions
app.UseRequestLogging(); // Log all requests
app.UseRateLimiting(); // Rate limiting
app.UseAuditLogging(); // Audit logging

app.UseSwagger();
app.UseSwaggerUI();

// Don't use HTTPS redirection in development as it can interfere with CORS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Enable serving static files from wwwroot
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();
app.UseTrialSubscription();

app.MapControllers();

// Map Health Check endpoint
app.MapHealthChecks("/health");

// Map SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat").RequireCors(corsPolicyName);
app.MapHub<NotificationHub>("/hubs/notifications").RequireCors(corsPolicyName);
app.MapHub<SessionHub>("/hubs/sessions").RequireCors(corsPolicyName);

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LiveExpert.Infrastructure.Data.ApplicationDbContext>();
        await LiveExpert.Infrastructure.Data.DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during database seeding");
    }
}

try
{
    Log.Information("Starting LiveExpert.AI API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
