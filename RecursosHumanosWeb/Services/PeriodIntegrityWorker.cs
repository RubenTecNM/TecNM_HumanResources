using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services
{
    public class PeriodIntegrityWorker : BackgroundService
    {
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
                        var context = scope.ServiceProvider.GetRequiredService<RecursosHumanosContext>();

                        // 1. Buscar periodos vencidos que siguen abiertos
                        var expiredPeriods = await context.Cortes
                            .Where(c => c.IdEstadoCorte == 1 && c.Termina < DateTime.Now)
                            .ToListAsync(stoppingToken);

                        if (expiredPeriods.Any())
                        {
                            _logger.LogInformation("Se detectaron {Count} periodos vencidos. Procediendo al cierre automático...", expiredPeriods.Count);

                            foreach (var period in expiredPeriods)
                            {
                                period.IdEstadoCorte = 2; // Cerrado
                                period.FechaModificacion = DateTime.Now;
                                period.IdUsuarioModifica = 1; // Sistema
                            }

                            // El contexto guardará los cambios y el trigger de SQL hará la cascada de los permisos
                            await context.SaveChangesAsync(stoppingToken);
                            
                            _logger.LogInformation("Periodos cerrados y permisos sincronizados correctamente.");
                        }
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
    }
}
