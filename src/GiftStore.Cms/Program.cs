var builder = WebApplication.CreateBuilder(args);

// Orchard Core Decoupled Headless CMS Host
builder.Services.AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    .ConfigureServices(tenantServices =>
    {
        // Custom multi-tenant or headless configurations if needed
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseOrchardCore();

app.Run();
