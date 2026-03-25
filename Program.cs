using Microsoft.AspNetCore.Authentication.Cookies;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios al contenedor
builder.Services.AddControllersWithViews();

// 2. CONFIGURACIÓN DE AUTENTICACIÓN
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CookieAuth";
    options.DefaultSignInScheme = "CookieAuth";
    options.DefaultChallengeScheme = "CookieAuth";
})
.AddCookie("CookieAuth", config =>
{
    config.Cookie.Name = "ServiceTower.Cookie";
    config.LoginPath = "/Account/Login";
    config.AccessDeniedPath = "/Account/Login";
    config.ExpireTimeSpan = TimeSpan.FromHours(8);
    config.Cookie.HttpOnly = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- CORRECCIÓN CRÍTICA DE ROTATIVA ---
// Usamos WebRootPath para apuntar directamente a wwwroot
IWebHostEnvironment env = app.Environment;
string rotativaRelativePath = "Rotativa";
RotativaConfiguration.Setup(env.WebRootPath, rotativaRelativePath);
// ---------------------------------------

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Mantenimientos}/{action=Index}/{id?}");

app.Run();