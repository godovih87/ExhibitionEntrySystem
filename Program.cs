using ExhibitionEntrySystem.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// База данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Сессии
builder.Services.AddSession();

// Локализация
var supportedCultures = new[] { new CultureInfo("ru-RU"), new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("ru-RU");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new[] { new CookieRequestCultureProvider() };
});

var app = builder.Build();

// Ошибки
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

// Статика и маршрутизация
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Локализация
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// Маршрут по умолчанию
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pavilion}/{action=Index}/{id?}"
);

app.Run();
