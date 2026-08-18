using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecursosHumanosWeb.Helpers;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels.ResetPassword;
using RecursosHumanosWeb.Services;
using RecursosHumanosWeb.Settings;

namespace RecursosHumanosWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PasswordResetApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<PasswordResetApiController> _logger;
        private readonly TimeSpan _tokenLifetime;

        public PasswordResetApiController(
            HumanResourcesContext context,
            IMemoryCache cache,
            IEmailSender emailSender,
            IOptions<SmtpSettings> smtpOptions,
            ILogger<PasswordResetApiController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            _emailSender = emailSender;
            _smtpSettings = smtpOptions.Value;
            _logger = logger;
            _tokenLifetime = TimeSpan.FromMinutes(configuration.GetValue("ResetPassword:TokenLifetimeMinutes", 20));
        }

        public class RequestTokenDto
        {
            [Required(ErrorMessage = "El correo electrónico es requerido.")]
            [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
            public string Email { get; set; } = string.Empty;
        }

        public class ValidateTokenDto
        {
            [Required]
            public string Selector { get; set; } = string.Empty;
            [Required]
            public string Verifier { get; set; } = string.Empty;
        }

        public class ConfirmResetDto
        {
            [Required]
            public string Selector { get; set; } = string.Empty;
            [Required]
            public string Verifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "La nueva contraseña es requerida.")]
            [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("request-token")]
        public async Task<IActionResult> RequestToken([FromBody] RequestTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error de Validación",
                    Message = "Por favor verifica los datos ingresados.",
                    Icon = "error"
                });
            }

            var emailNormalized = dto.Email.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized && u.Status);

            if (user == null)
            {
                return Ok(new AlertResponseDTO
                {
                    Success = true,
                    Title = "Solicitud Recibida",
                    Message = "Si la cuenta existe, se ha enviado un enlace de restablecimiento al correo proporcionado.",
                    Icon = "info"
                });
            }

            var selector = GenerateRandomString(12);
            var verifier = GenerateRandomString(32);
            var tokenHash = ComputeSha256Hash(verifier);

            var token = new ResetPasswordToken
            {
                Selector = selector,
                TokenHash = tokenHash,
                Used = false,
                RequestingUserId = user.Id,
                CreatedByUserId = user.Id,
                ExpiresAt = DateTime.Now.Add(_tokenLifetime)
            };

            _context.ResetPasswordTokens.Add(token);
            await _context.SaveChangesAsync();

            var frontendBaseUrl = $"{Request.Scheme}://{Request.Host}";
            var resetLink = $"{frontendBaseUrl}/reset-password?sel={selector}&tok={verifier}";

            var emailModel = new RequestTokenEmailModel
            {
                Email = dto.Email,
                Selector = selector,
                Verifier = verifier,
                Link = resetLink,
                ExpirationMinutes = (int)_tokenLifetime.TotalMinutes
            };

            var sent = await _emailSender.SendTokenEmailAsync(emailModel);
            if (!sent)
            {
                return StatusCode(500, new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error de Envío",
                    Message = "No se pudo enviar el correo de recuperación. Intente más tarde.",
                    Icon = "error"
                });
            }

            return Ok(new AlertResponseDTO
            {
                Success = true,
                Title = "Correo Enviado",
                Message = "Si la cuenta existe, se ha enviado un enlace de restablecimiento al correo proporcionado.",
                Icon = "success"
            });
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenDto dto)
        {
            if (string.IsNullOrEmpty(dto.Selector) || string.IsNullOrEmpty(dto.Verifier))
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Token Inválido",
                    Message = "Los parámetros del token son requeridos.",
                    Icon = "error"
                });
            }

            var tokenEntry = await _context.ResetPasswordTokens
                .Where(t => t.Selector == dto.Selector && !t.Used && t.Status)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (tokenEntry == null || tokenEntry.ExpiresAt < DateTime.Now)
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Token Expirado",
                    Message = "El enlace de restablecimiento ha expirado o no es válido.",
                    Icon = "error"
                });
            }

            var providedHash = ComputeSha256Hash(dto.Verifier);
            if (!string.Equals(providedHash, tokenEntry.TokenHash, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Token Inválido",
                    Message = "El código de verificación no coincide.",
                    Icon = "error"
                });
            }

            var user = await _context.Users.FindAsync(tokenEntry.RequestingUserId);

            return Ok(new
            {
                Success = true,
                Message = "El token es válido.",
                Email = user?.Email ?? string.Empty
            });
        }

        [HttpPost("confirm-reset")]
        public async Task<IActionResult> ConfirmReset([FromBody] ConfirmResetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error de Validación",
                    Message = "La contraseña debe tener al menos 8 caracteres.",
                    Icon = "error"
                });
            }

            var tokenEntry = await _context.ResetPasswordTokens
                .Where(t => t.Selector == dto.Selector && !t.Used && t.Status)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (tokenEntry == null || tokenEntry.ExpiresAt < DateTime.Now)
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Token Inválido",
                    Message = "El token es inválido o ha expirado.",
                    Icon = "error"
                });
            }

            var providedHash = ComputeSha256Hash(dto.Verifier);
            if (!string.Equals(providedHash, tokenEntry.TokenHash, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Token Inválido",
                    Message = "El token de verificación no coincide.",
                    Icon = "error"
                });
            }

            var user = await _context.Users.FindAsync(tokenEntry.RequestingUserId);
            if (user == null)
            {
                return NotFound(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Usuario No Encontrado",
                    Message = "No se encontró el usuario asociado al token.",
                    Icon = "error"
                });
            }

            user.PasswordHash = PasswordHasher.HashSha256(dto.NewPassword);
            tokenEntry.Used = true;
            tokenEntry.UpdatedByUserId = user.Id;
            tokenEntry.UpdatedAt = DateTime.Now;

            _context.Update(user);
            _context.Update(tokenEntry);
            await _context.SaveChangesAsync();

            return Ok(new AlertResponseDTO
            {
                Success = true,
                Title = "Contraseña Actualizada",
                Message = "Tu contraseña ha sido actualizada correctamente. Ya puedes iniciar sesión.",
                Icon = "success"
            });
        }

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
