using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels;
using HumanResourcesProduction.Services;
using System.Security.Claims;

namespace HumanResourcesProduction.Features.Auth
{
    [ApiController]
    [Route("Account")]
    public class AccountController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly ILoginAttemptTracker _loginAttemptTracker;
        private readonly IAntiforgery _antiforgery;

        public AccountController(
            HumanResourcesContext context,
            ILogger<AccountController> logger,
            ILoginAttemptTracker loginAttemptTracker,
            IAntiforgery antiforgery)
        {
            _context = context;
            _logger = logger;
            _loginAttemptTracker = loginAttemptTracker;
            _antiforgery = antiforgery;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            // Returns minimal HTML containing the __RequestVerificationToken input for authStore extraction
            var html = $@"<!DOCTYPE html><html><body><form id=""loginForm""><input type=""hidden"" name=""__RequestVerificationToken"" value=""{tokens.RequestToken}"" /></form></body></html>";
            return Content(html, "text/html");
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, title = "Error", message = "Datos de inicio de sesión incompletos." });
            }

            try
            {
                var loginKey = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;

                if (_loginAttemptTracker.IsLockedOut(loginKey))
                {
                    _logger.LogWarning("Intento de inicio de sesión bloqueado para {Email}.", model.Email);
                    return BadRequest(new { success = false, title = "Bloqueado", message = "Demasiados intentos fallidos. Inténtelo nuevamente más tarde." });
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
                    return BadRequest(new { success = false, title = "Error", message = "Email o contraseña incorrectos." });
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

                return Ok(new
                {
                    success = true,
                    title = "¡Éxito!",
                    message = "Sesión iniciada correctamente.",
                    redirectUrl = !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : "/"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el login para {Email}", model.Email);
                return StatusCode(500, new { success = false, title = "Error Interno", message = "Error interno al procesar el inicio de sesión." });
            }
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Sesión cerrada correctamente." });
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return StatusCode(403, new { success = false, title = "Acceso Denegado", message = "No tiene permisos para acceder a este recurso." });
        }

        // Returns the current cookie-authenticated user identity so the SPA can
        // read role claims (UserType) that drive menu visibility.
        [HttpGet("Me")]
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

            return Ok(new
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
    }
}
