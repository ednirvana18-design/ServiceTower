using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Supabase;

namespace ServiceTowerWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _url;
        private readonly string _key;

        public AccountController(IConfiguration config)
        {
            // Extraemos las llaves de appsettings.json
            _url = config["Supabase:Url"] ?? "";
            _key = config["Supabase:Key"] ?? "";
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                // 1. Inicializamos el cliente de Supabase
                var options = new SupabaseOptions { AutoConnectRealtime = true };
                var client = new Supabase.Client(_url, _key, options);
                await client.InitializeAsync();

                // 2. Intentamos iniciar sesión con Supabase Auth (el mismo de tu app móvil)
                var session = await client.Auth.SignIn(email, password);

                if (session != null && session.User != null)
                {
                    // 3. Creamos los "Claims" (datos del usuario para la sesión web)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, session.User.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, session.User.Id ?? ""),
                        new Claim("SupabaseToken", session.AccessToken ?? "")
                    };

                    // USAMOS "CookieAuth" para que coincida con Program.cs
                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // Mantiene la sesión iniciada al cerrar el navegador
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    // 4. Firmamos la entrada en el servidor web
                    await HttpContext.SignInAsync(
                        "CookieAuth",
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Mantenimientos");
                }
            }
            catch (Exception)
            {
                // Si Supabase rechaza las credenciales, mandamos el error a la vista
                ViewBag.Error = "Correo o contraseña incorrectos en Supabase.";
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Cerramos sesión usando el mismo nombre de esquema
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}