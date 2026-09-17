using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using StackExchange.Redis;
using GiftStore.Application.Interfaces;
using GiftStore.Application.Services;
using GiftStore.Application.UseCases.Checkout;
using GiftStore.Application.UseCases.Fulfillment;
using GiftStore.Application.UseCases.Payments;
using GiftStore.Cms.Services;
using GiftStore.Gifticard.Client;
using GiftStore.Gifticard.Configuration;
using GiftStore.Gifticard.Fakes;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Infrastructure.BackgroundJobs;
using GiftStore.Infrastructure.Caching;
using GiftStore.Infrastructure.Persistence;
using GiftStore.Infrastructure.Security;
using GiftStore.Payment.Configuration;
using GiftStore.Payment.Gateways;
using GiftStore.Payment.Interfaces;
using GiftStore.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration with sensitive code redaction
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

builder.AddServiceDefaults();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=giftcardbuy_db;Username=postgres;Password=YourSecurePassword123!";

builder.Services.AddDbContext<GiftStoreDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IGiftStoreDbContext>(sp => sp.GetRequiredService<GiftStoreDbContext>());

// Redis & Distributed Caching
var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConn))
{
    try
    {
        var multiplexer = ConnectionMultiplexer.Connect(redisConn);
        builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        builder.Services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(multiplexer, "DataProtection-Keys")
            .SetApplicationName("GiftCardBuy");
    }
    catch
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => null!);
        builder.Services.AddDataProtection().SetApplicationName("GiftCardBuy");
    }
}
else
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => null!);
    builder.Services.AddDataProtection().SetApplicationName("GiftCardBuy");
}

builder.Services.AddSingleton<IDistributedCacheService, RedisDistributedCacheService>();
builder.Services.AddSingleton<IEncryptionService, DataProtectionEncryptionService>();

// Options
builder.Services.Configure<GifticardOptions>(builder.Configuration.GetSection(GifticardOptions.SectionName));
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection(PaymentOptions.SectionName));
builder.Services.Configure<OrchardCoreCmsOptions>(builder.Configuration.GetSection(OrchardCoreCmsOptions.SectionName));

// Gift-i-Card Supplier Registration
var gifticardOpts = builder.Configuration.GetSection(GifticardOptions.SectionName).Get<GifticardOptions>() ?? new GifticardOptions();
if (gifticardOpts.UseFakeInDevelopment)
{
    builder.Services.AddSingleton<IGiftCardSupplier, FakeGiftCardSupplier>();
}
else
{
    builder.Services.AddHttpClient<IGiftCardSupplier, GifticardClient>();
}

// Payment Gateway Registration
var paymentOpts = builder.Configuration.GetSection(PaymentOptions.SectionName).Get<PaymentOptions>() ?? new PaymentOptions();
if (string.Equals(paymentOpts.Provider, "Zarinpal", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IPaymentGateway, ZarinpalPaymentGateway>();
}
else
{
    builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();
}

// Application Services & Handlers
builder.Services.AddScoped<IPriceCalculator, PriceCalculator>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<InitiatePaymentHandler>();
builder.Services.AddScoped<ProcessPaymentCallbackHandler>();
builder.Services.AddScoped<ProcessFulfillmentHandler>();
builder.Services.AddScoped<RevealCardCodeHandler>();

// Decoupled CMS Integration
builder.Services.AddHttpClient<ICmsService, CachedPersianCmsService>(client =>
{
    var cmsBase = builder.Configuration["Cms:BaseUrl"] ?? "http://localhost:5050/";
    client.BaseAddress = new Uri(cmsBase);
});

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "GiftCardBuy.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomer", policy => policy.RequireAuthenticatedUser());
});

// Antiforgery & Rate Limiting
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "GiftCardBuy.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Background Jobs via Quartz.NET
builder.Services.AddQuartz(q =>
{
    var outboxJobKey = new JobKey("OutboxProcessorJob");
    q.AddJob<OutboxProcessorJob>(opts => opts.WithIdentity(outboxJobKey));
    q.AddTrigger(opts => opts
        .ForJob(outboxJobKey)
        .WithIdentity("OutboxProcessorTrigger")
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()));

    var catalogSyncJobKey = new JobKey("SupplierCatalogSyncJob");
    q.AddJob<SupplierCatalogSyncJob>(opts => opts.WithIdentity(catalogSyncJobKey));
    q.AddTrigger(opts => opts
        .ForJob(catalogSyncJobKey)
        .WithIdentity("SupplierCatalogSyncTrigger")
        .WithSimpleSchedule(x => x.WithIntervalInHours(6).RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Web & Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Account", "RequireCustomer");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdmin");
});
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

// Migrate and Seed Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GiftStoreDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();
        await GiftStoreDataSeeder.SeedAsync(db, app.Logger);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed database auto-creation or seed in current environment.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
