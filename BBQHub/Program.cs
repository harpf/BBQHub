using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Infrastructure.Identity;
using BBQHub.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog(options =>
    {
        options.SourceName = "BBQHubApp";
    });
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());


var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    // Diese Zeile f�hrt alle Migrationen aus, falls noch nicht angewendet
    await dbContext.Database.MigrateAsync();

    // Seed AdminUser (z.?B. Standardrolle + Admin-Konto)
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


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();