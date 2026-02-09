using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography; // Necesario para SHA256
using System.Text; // Necesario para Encoding

namespace RecursosHumanosWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecursosHumanosContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(RecursosHumanosContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Reset de contraseña solicitado para i22050326@monclova.tecnm.mx
            try {
                var correoReset = "i22050326@monclova.tecnm.mx";
                var userReset = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoReset);
                if (userReset != null) {
                    userReset.Clave = RecursosHumanosWeb.Helpers.PasswordHasher.HashSha256("axel");
                    _context.Update(userReset);
                    await _context.SaveChangesAsync();
                }
            } catch { /* Ignorar si falla el reset temporal */ }

            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Permiso");

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
                // 1. Encriptar la clave recibida del formulario usando el Helper unificado
                string claveEncriptada = RecursosHumanosWeb.Helpers.PasswordHasher.HashSha256(model.Clave);

                // 2. Buscar usuario que coincida con correo Y clave encriptada
                var usuario = await _context.Usuarios
                    .Where(u => u.Correo == model.Correo && u.Clave == claveEncriptada && u.Estatus == true)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
                    return View(model);
                }

                // 3. Crear los Claims para la sesión
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim("TipoUsuario", usuario.IdTipoUsuario.ToString())
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

                _logger.LogInformation("Usuario {Correo} inició sesión correctamente.", model.Correo);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Permiso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en el login para {Correo}", model.Correo);
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