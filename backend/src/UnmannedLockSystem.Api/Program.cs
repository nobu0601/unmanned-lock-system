using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UnmannedLockSystem.Api.Adapters.Payment;
using UnmannedLockSystem.Api.Adapters.SmartLock;
using UnmannedLockSystem.Api.Auth;
using UnmannedLockSystem.Api.Configuration;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<LiffSettings>(builder.Configuration.GetSection("Liff"));

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];
if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(databaseUrl))
{
    // Convert Railway's DATABASE_URL (postgresql://user:pass@host:port/db) to Npgsql format
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));

// HttpClient for LINE API
builder.Services.AddHttpClient();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPassService, PassService>();
builder.Services.AddScoped<IQrTokenService, QrTokenService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Adapters
if (builder.Configuration.GetValue<bool>("UseMockPayment"))
    builder.Services.AddScoped<IPaymentProviderAdapter, MockPaymentAdapter>();
else
    builder.Services.AddScoped<IPaymentProviderAdapter, StripePaymentAdapter>();

builder.Services.AddScoped<ISmartLockAdapter, MockSmartLockAdapter>();

// Authentication
builder.Services.AddAuthentication()
    .AddScheme<LiffAuthOptions, LiffAuthHandler>("Liff", _ => { })
    .AddScheme<DeviceAuthOptions, DeviceAuthHandler>("Device", _ => { })
    .AddJwtBearer("Admin", options =>
    {
        var adminSecret = builder.Configuration["Jwt:AdminSecret"] ?? "dev-admin-secret-key-minimum-32-chars!!";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "unmanned-lock-system",
            ValidateAudience = true,
            ValidAudience = "admin-dashboard",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Authorization policies
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Customer", p => p.AddAuthenticationSchemes("Liff").RequireAuthenticatedUser());
    o.AddPolicy("Device", p => p.AddAuthenticationSchemes("Device").RequireAuthenticatedUser());
    o.AddPolicy("Admin", p => p.AddAuthenticationSchemes("Admin").RequireRole("admin"));
});

// Rate Limiting
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("DeviceScan", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });
    o.RejectionStatusCode = 429;
});

// CORS
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowFrontends", p =>
    {
        var origins = builder.Configuration["Cors:Origins"]?.Split(",")
            ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:5175" };
        p.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Unmanned Lock System API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization or LINE Access Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityDefinition("DeviceKey", new OpenApiSecurityScheme
    {
        Description = "Device API Key",
        Name = "X-Device-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityDefinition("MockLineUser", new OpenApiSecurityScheme
    {
        Description = "Mock LINE User ID (dev only)",
        Name = "X-Mock-Line-User-Id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
});

var app = builder.Build();

// Auto-migrate and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db);
}

// Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontends");
app.UseRateLimiter();

// Serve frontend static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback for each frontend app
app.MapFallback(context =>
{
    var path = context.Request.Path.Value ?? "";
    if (path.StartsWith("/liff"))
        context.Response.Redirect("/liff/index.html");
    else if (path.StartsWith("/device"))
        context.Response.Redirect("/device/index.html");
    else if (path.StartsWith("/admin"))
        context.Response.Redirect("/admin/index.html");
    else
    {
        context.Response.ContentType = "text/html";
        return context.Response.SendFileAsync(
            Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html"));
    }
    return Task.CompletedTask;
});

app.Run();
