using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services
{
    public class PeriodIntegrityWorker : BackgroundService
    {
        private static readonly TimeSpan TokenRetentionPeriod = TimeSpan.FromDays(30);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PeriodIntegrityWorker> _logger;

        public PeriodIntegrityWorker(IServiceProvider serviceProvider, ILogger<PeriodIntegrityWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando el Guardián de Integridad de Periodos...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var cutoffService = scope.ServiceProvider.GetRequiredService<ICutoffService>();
                        await cutoffService.AutoCloseCutoffIfNeededAsync();

                        await CleanupExpiredResetTokensAsync(scope, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error en el Guardián de Integridad.");
                }

                // Esperar 1 hora para mantenimiento regular
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CleanupExpiredResetTokensAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<HumanResourcesContext>();
            var retentionLimit = DateTime.Now.Subtract(TokenRetentionPeriod);

            var expiredTokens = await context.ResetPasswordTokens
                .Where(t => (!t.Status || t.Used || t.ExpiresAt < DateTime.Now) && t.CreatedAt < retentionLimit)
                .ToListAsync(stoppingToken);

            if (expiredTokens.Count == 0) return;

            context.ResetPasswordTokens.RemoveRange(expiredTokens);
            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Se limpiaron {Count} tokens de restablecimiento vencidos o usados.", expiredTokens.Count);
        }
    }
}
