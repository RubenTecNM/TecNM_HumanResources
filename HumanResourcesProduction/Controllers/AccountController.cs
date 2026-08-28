using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels;
using HumanResourcesProduction.Services;
using System.Security.Claims;
using System.Security.Cryptography; // Necesario para SHA256
using System.Text; // Necesario para Encoding

namespace HumanResourcesProduction.Controllers
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
                string claveEncriptada = HumanResourcesProduction.Helpers.PasswordHasher.HashSha256(model.Password);

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

                // La clave temporal ya fue usada: deja de ser visible para RH.
                if (!string.IsNullOrEmpty(user.TempPassword))
                {
                    user.TempPassword = null;
                    await _context.SaveChangesAsync();
                }

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

        // Returns the current cookie-authenticated user identity so the SPA can
        // read role claims (UserType) that drive menu visibility.
        [HttpGet]
        public async Task<IActionResult> Me()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Unauthorized();

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
            var typeClaim = User.FindFirstValue("UserType") ?? "0";
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            // Resuelto en BD (no claims) para reflejar cambios sin re-login.
            var info = await _context.Users.AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    Area = u.Area != null ? u.Area.Description : null,
                    Department = u.Department != null ? u.Department.Description : null,
                    Position = u.Position != null ? u.Position.Description : null
                })
                .FirstOrDefaultAsync();
            var isDocente = info != null && info.Department == "DOCENCIA";

            return Json(new
            {
                Id = int.TryParse(idClaim, out var id) ? id : 0,
                Name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Email = email,
                UserType = int.TryParse(typeClaim, out var type) ? type : 0,
                IsDocente = isDocente,
                Area = info?.Area ?? string.Empty,
                Department = info?.Department ?? string.Empty,
                Position = info?.Position ?? string.Empty
            });
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