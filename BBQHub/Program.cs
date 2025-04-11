using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Infrastructure.Identity;
using BBQHub.Application.Common.Interfaces;
using BBQHub.Application.Juroren.Services;
using BBQHub.Hubs;
using QuestPDF.Infrastructure;
using BBQHub.Infrastructure.PDF;
using BBQHub.Domain.Entities;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

#pragma warning disable CA1416 // Validate platform compatibility
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog(options =>
    {
        options.SourceName = "BBQHubApp";
    });
}
#pragma warning restore CA1416

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = true
        })
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<SeoOptions>(builder.Configuration.GetSection("SEO"));

builder.Services.AddTransient<PDFRegistrationFormular>();

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped<IJurorService, JurorService>();

builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true; // Zeigt den Banner
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});



var app = builder.Build();

app.MapHub<EventMonitorHub>("/eventMonitorHub");

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    // Diese Zeile führt alle Migrationen aus, falls noch nicht angewendet
    await dbContext.Database.MigrateAsync();

    // Seed AdminUser (z. B. Standardrolle + Admin-Konto)
    await SeedData.EnsureAdminUserAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCookiePolicy();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    await next();
});

app.MapGet("/api/validate/juryid/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var exists = await db.Juroren.AnyAsync(j => j.JuryId == id);
    return Results.Json(new { exists });
});

app.MapGet("/api/validate/email/{email}", async (string email, ApplicationDbContext db) =>
{
    var exists = await db.Juroren.AnyAsync(j => j.Email == email);
    return Results.Json(new { exists });
});
app.UseStaticFiles();

app.UseHttpsRedirection();

//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Content-Security-Policy",
//        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
//    await next();
//});

app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Powered-By");
    await next();
});

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});


app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();