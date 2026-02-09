using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services
{
    public class CorteService : ICorteService
    {
        private readonly RecursosHumanosContext _context;
        private readonly ILogger<CorteService> _logger;

        public CorteService(RecursosHumanosContext context, ILogger<CorteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Corte> GetActiveCorteAsync()
        {
            return await _context.Cortes
                .Where(c => c.Estatus)
                .OrderByDescending(c => c.Termina)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingPermissionsAsync(int corteId)
        {
            return await _context.Permisos.AnyAsync(p => p.IdCorte == corteId && !p.Revisado && p.Estatus);
        }

        public async Task AutoCloseCorteIfNeededAsync()
        {
            try
            {
                var corteActivo = await GetActiveCorteAsync();

                if (corteActivo != null && corteActivo.Termina < DateTime.Now)
                {
                    // Usar contexto para activar auditoría de sistema
                    _context._auditOrigenOverride = (int)TipoOrigenEnum.SistemaAutomatico;
                    int rootId = 1; // Root User ID

                    // 1. Manejar permisos pendientes (Rechazo automático)
                    var pendientes = await _context.Permisos
                        .Where(p => p.IdCorte == corteActivo.Id && !p.Revisado && p.Estatus)
                        .ToListAsync();

                    if (pendientes.Any())
                    {
                        foreach (var p in pendientes)
                        {
                            p.Revisado = true;
                            p.Goce = false; // Rechazo automático
                            p.IdUsuarioAutoriza = rootId;
                            p.FechaAutorizacion = DateTime.Now;
                            p.IdUsuarioModifica = rootId;
                            p.FechaModificacion = DateTime.Now;
                            // Nota: No sobrescribimos el motivo original del usuario.
                        }
                        _context.UpdateRange(pendientes);
                    }

                    // 2. Cerrar Corte
                    corteActivo.Estatus = false;
                    corteActivo.FechaModificacion = DateTime.Now;
                    corteActivo.IdUsuarioModifica = rootId;
                        
                    _context.Update(corteActivo);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Corte {Id} cerrado automáticamente por expiración.", corteActivo.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el cierre automático de corte.");
            }
        }

        public async Task<bool> CreateCorteAsync(Corte nuevoCorte, int userId)
        {
            var corteActivo = await GetActiveCorteAsync();
            DateTime ahora = DateTime.Now;

            // Transacción para asegurar consistencia
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (corteActivo != null)
                    {
                        if (await HasPendingPermissionsAsync(corteActivo.Id))
                        {
                            return false; // No se puede crear si hay pendientes
                        }

                        corteActivo.Estatus = false;
                        corteActivo.FechaModificacion = ahora;
                        corteActivo.IdUsuarioModifica = userId;
                        _context.Update(corteActivo);
                    }

                    nuevoCorte.IdUsuarioCrea = userId;
                    nuevoCorte.IdUsuarioModifica = userId;
                    nuevoCorte.FechaCreacion = ahora;
                    nuevoCorte.FechaModificacion = ahora;
                    nuevoCorte.Estatus = true;

                    _context.Add(nuevoCorte);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear nuevo corte.");
                    throw;
                }
            }
        }

        public async Task<bool> InactivateCorteAsync(int id, int userId)
        {
            var corte = await _context.Cortes.FindAsync(id);
            if (corte == null) return false;

            if (await HasPendingPermissionsAsync(id)) return false;

            corte.Estatus = false;
            corte.FechaModificacion = DateTime.Now;
            corte.IdUsuarioModifica = userId;
            
            _context.Update(corte);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
