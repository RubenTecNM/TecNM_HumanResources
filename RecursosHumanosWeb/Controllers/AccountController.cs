using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels;
using RecursosHumanosWeb.Services;
using System.Security.Claims;
using System.Security.Cryptography; // Necesario para SHA256
using System.Text; // Necesario para Encoding

namespace RecursosHumanosWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly ILoginAttemptTracker _loginAttemptTracker;

        public AccountController(HumanResourcesContext context, ILogger<AccountController> logger, ILoginAttemptTracker loginAttemptTracker)
        {
            _context = context;
            _logger = logger;
            _loginAttemptTracker = loginAttemptTracker;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Permission");


            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var loginKey = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;

                if (_loginAttemptTracker.IsLockedOut(loginKey))
                {
                    _logger.LogWarning("Intento de inicio de sesión bloqueado para {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "Demasiados intentos fallidos. Inténtelo nuevamente más tarde.");
                    return View(model);
                }

                // 1. Encriptar la clave recibida del formulario usando el Helper unificado
                string claveEncriptada = RecursosHumanosWeb.Helpers.PasswordHasher.HashSha256(model.Password);

                // 2. Buscar user que coincida con correo Y clave encriptada
                var user = await _context.Users
                    .Where(u => u.Email == model.Email && u.PasswordHash == claveEncriptada && u.Status == true)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _loginAttemptTracker.RegisterFailure(loginKey);
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
                    return View(model);
                }

                _loginAttemptTracker.Reset(loginKey);

                // 3. Crear los Claims para la sesión
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserType", user.UserTypeId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                _logger.LogInformation("User {Email} inició sesión correctamente.", model.Email);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Permission");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el login para {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Error interno al procesar el inicio de sesión");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Función auxiliar para encriptar en SHA256
        private string ConvertirSha256(string texto)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Computar el hash a partir de los bytes del texto
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(texto));

                // Convertir el array de bytes a una cadena hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}