using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using SleekClothing.Data;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Ajouter la configuration pour la culture marocaine
var cultureInfo = new CultureInfo("fr-MA");
cultureInfo.NumberFormat.CurrencySymbol = "DH";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(connectionString));

// Ajouter la prise en charge des sessions
builder.Services.AddSession();

// Ajouter la configuration pour l'identité
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation().AddRazorPagesOptions(options =>
{
    options.Conventions.AllowAnonymousToPage("/management"); // Permettre l'accès à la pag
});

// Ajouter la politique d'autorisation pour les administrateurs
builder.Services.AddAuthorization(options => options.AddPolicy("RequireAdmins", policy => policy.RequireRole("Admin")));

var app = builder.Build();

// Utiliser la culture configurée pour les requêtes
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Activer les sessions dans le pipeline HTTP
app.UseSession();

app.MapRazorPages();

app.Run();
