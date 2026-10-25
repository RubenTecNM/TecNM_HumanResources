using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels.ResetPassword;
using HumanResourcesProduction.Models.DTOs;
using HumanResourcesProduction.Helpers;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using HumanResourcesProduction.Settings;

namespace HumanResourcesProduction.Features.Auth
{
    public class ResetPasswordController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<ResetPasswordController> _logger;
        private readonly TimeSpan _tokenLifetime;

        public ResetPasswordController(
            HumanResourcesContext context,
            IMemoryCache cache,
            IEmailSender emailSender,
            IOptions<SmtpSettings> smtpOptions,
            ILogger<ResetPasswordController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _emailSender = emailSender;
            _smtpSettings = smtpOptions.Value;
            _logger = logger;
            _tokenLifetime = TimeSpan.FromMinutes(configuration.GetValue("ResetPassword:TokenLifetimeMinutes", 20));
        }

        [HttpGet]
        public IActionResult RequestToken()
        {
            return View(new RequestTokenViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Accept DTO via normal model binding (form posts / FormData). Avoid [FromBody] because multipart/form-data
        // requests (forms) will cause a 415 when ASP.NET cannot select an input formatter.
        public async Task<IActionResult> RequestToken(Models.DTOs.RequestTokenRequest? dto)
        {
            // Support both JSON (AJAX) and form posts: if dto is null, try bind from form
            if (dto == null)
            {
                var form = new RequestTokenViewModel();
                await TryUpdateModelAsync(form);
                dto = new Models.DTOs.RequestTokenRequest { Email = form.Email };
            }

            // Validate
            if (string.IsNullOrWhiteSpace(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
            {
                var bad = new AlertResponseDTO { Success = false, Title = "Error", Message = "Email inválido.", Icon = "error" };
                return Json(bad);
            }

            var emailNormalized = dto.Email.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized && u.Status);
            // Do not reveal whether email exists

            if (user == null)
            {
                var resp = new AlertResponseDTO { Success = true, Title = "Enviado", Message = "Si la cuenta existe, se ha enviado un enlace de restablecimiento al correo proporcionado.", Icon = "info" };
                return Json(resp);
            }

            // Generate selector + verifier
            var selector = GenerateRandomString(12);
            var verifier = GenerateRandomString(32);
            var tokenHash = ComputeSha256Hash(verifier);

            var token = new ResetPasswordToken
            {
                Selector = selector,
                TokenHash = tokenHash,
                Used = false,
                RequestingUserId = user.Id,
                CreatedByUserId = user.Id,   // BaseEntity.CreatedByUserId es int (no nullable); el solicitante crea su propio token
                ExpiresAt = DateTime.Now.Add(_tokenLifetime)
            };

            _context.ResetPasswordTokens.Add(token);
            await _context.SaveChangesAsync();

            var link = Url.Action(nameof(ResetPassword), "ResetPassword", new { sel = selector, tok = verifier }, Request.Scheme)!;

            var emailModel = new RequestTokenEmailModel
            {
                Email = dto.Email,
                Selector = selector,
                Verifier = verifier,
                Link = link,
                ExpirationMinutes = (int)_tokenLifetime.TotalMinutes
            };

            var sent = await _emailSender.SendTokenEmailAsync(emailModel);
            if (!sent)
            {
                var fail = new AlertResponseDTO { Success = false, Title = "Error", Message = "No se pudo enviar el correo. Intente más tarde.", Icon = "error" };
                return Json(fail);
            }

            var ok = new AlertResponseDTO { Success = true, Title = "Enviado", Message = "Si la cuenta existe, se ha enviado un enlace de restablecimiento al correo proporcionado.", Icon = "success", RedirectUrl = Url.Action("Login", "Account") };
            return Json(ok);
        }

        [HttpGet]
        public IActionResult ConfirmToken()
        {
            var vm = new ConfirmTokenViewModel
            {
                Email = TempData["ResetEmail"] as string
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult ResetPassword(string sel, string tok)
        {
            if (string.IsNullOrEmpty(sel) || string.IsNullOrEmpty(tok))
            {
                return RedirectToAction(nameof(RequestToken));
            }

            // Validate selector and token quickly to populate email and avoid ModelState invalid on POST
            var tokenEntry = _context.ResetPasswordTokens
                .Where(t => t.Selector == sel && t.Used == false && t.Status == true)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            if (tokenEntry == null)
            {
                TempData["ResetPasswordMessage"] = "Token inválido o no encontrado.";
                return RedirectToAction(nameof(RequestToken));
            }

            if (tokenEntry.ExpiresAt < DateTime.Now)
            {
                TempData["ResetPasswordMessage"] = "Token expirado.";
                return RedirectToAction(nameof(RequestToken));
            }

            // Compare hash of provided tok
            var providedHash = ComputeSha256Hash(tok);
            if (!string.Equals(providedHash, tokenEntry.TokenHash, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ResetPasswordMessage"] = "Token inválido.";
                return RedirectToAction(nameof(RequestToken));
            }

            // Populate Email for the view model to satisfy validation and show user (non-sensitive)
            var user = _context.Users.Find(tokenEntry.RequestingUserId);
            var vm = new ResetPasswordViewModel
            {
                Selector = sel,
                Token = tok,
                Email = user?.Email ?? string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(m => m.Value!.Errors.Any())
                    .ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                // Log únicamente los nombres de los campos en error, nunca los valores enviados.
                _logger.LogWarning("ResetPassword POST - ModelState invalid. Campos en error: {Fields}", errors.Keys.ToArray());

                var bad = new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error",
                    Message = "Datos inválidos",
                    Icon = "error",
                    Errors = errors
                };

                return Json(bad);
            }

            // Find token by selector
            var tokenEntry = await _context.ResetPasswordTokens
                .Where(t => t.Selector == model.Selector && t.Used == false && t.Status == true)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (tokenEntry == null)
            {
                var bad = new AlertResponseDTO { Success = false, Title = "Error", Message = "Token inválido o ya usado.", Icon = "error" };
                return Json(bad);
            }

            if (tokenEntry.ExpiresAt < DateTime.Now)
            {
                var bad = new AlertResponseDTO { Success = false, Title = "Error", Message = "Token expirado.", Icon = "error" };
                return Json(bad);
            }

            var providedHash = ComputeSha256Hash(model.Token);
            if (!string.Equals(providedHash, tokenEntry.TokenHash, StringComparison.OrdinalIgnoreCase))
            {
                var bad = new AlertResponseDTO { Success = false, Title = "Error", Message = "Token inválido.", Icon = "error" };
                return Json(bad);
            }

            // Token valid -> update user password
            var user = await _context.Users.FindAsync(tokenEntry.RequestingUserId);
            if (user == null)
            {
                var bad = new AlertResponseDTO { Success = false, Title = "Error", Message = "User no encontrado.", Icon = "error" };
                return Json(bad);
            }

            user.PasswordHash = PasswordHasher.HashSha256(model.NewPassword);
            tokenEntry.Used = true;
            tokenEntry.UpdatedByUserId = user.Id;
            tokenEntry.UpdatedAt = DateTime.Now;

            _context.Update(user);
            _context.Update(tokenEntry);
            await _context.SaveChangesAsync();

            var successMessage = "Contraseña actualizada correctamente.";

            var ok = new AlertResponseDTO { Success = true, Title = "Éxito", Message = successMessage, Icon = "success", RedirectUrl = Url.Action("Login", "Account") };
            return Json(ok);
        }

        // Helpers
        private static string GenerateRandomString(int bytes)
        {
            var b = new byte[bytes];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(b);
            return Convert.ToBase64String(b).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var x in hash)
                sb.Append(x.ToString("X2"));
            return sb.ToString();
        }
    }
}
