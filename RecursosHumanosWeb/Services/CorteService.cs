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

        public async Task<Corte> GetLastCorteAsync()
        {
            return await _context.Cortes
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
                var cortesExpirados = await _context.Cortes
                    .Where(c => c.Estatus && c.Termina.Date < DateTime.Now.Date)
                    .ToListAsync();

                foreach (var corteActivo in cortesExpirados)
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

                    nuevoCorte.Inicia = nuevoCorte.Inicia.Date;
                    nuevoCorte.Termina = nuevoCorte.Termina.Date.AddDays(1).AddTicks(-1);
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

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Inactivar Periodo
                    corte.Estatus = false;
                    corte.FechaModificacion = DateTime.Now;
                    corte.IdUsuarioModifica = userId;
                    _context.Update(corte);

                    // 2. Inactivar Permisos relacionados (Borrado lógico en cascada)
                    var permisos = await _context.Permisos
                        .Where(p => p.IdCorte == id && p.Estatus)
                        .ToListAsync();

                    if (permisos.Any())
                    {
                        foreach (var p in permisos)
                        {
                            p.Estatus = false; // Desactivar permiso
                            p.FechaModificacion = DateTime.Now;
                            p.IdUsuarioModifica = userId;
                        }
                        _context.UpdateRange(permisos);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la inactivación en cascada del periodo {Id}.", id);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> CloseCorteAsync(int id, int userId)
        {
            var corte = await _context.Cortes.FindAsync(id);
            if (corte == null || !corte.Estatus) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Cerrar Periodo
                    corte.Estatus = false;
                    corte.FechaModificacion = DateTime.Now;
                    corte.IdUsuarioModifica = userId;
                    _context.Update(corte);

                    // 2. Rechazar automáticamente permisos pendientes
                    var pendientes = await _context.Permisos
                        .Where(p => p.IdCorte == id && !p.Revisado && p.Estatus)
                        .ToListAsync();

                    if (pendientes.Any())
                    {
                        foreach (var p in pendientes)
                        {
                            p.Revisado = true;
                            p.Goce = false; // Sin goce por cierre manual
                            p.FechaAutorizacion = DateTime.Now;
                            p.IdUsuarioAutoriza = userId;
                            p.IdUsuarioModifica = userId;
                            p.FechaModificacion = DateTime.Now;
                        }
                        _context.UpdateRange(pendientes);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Corte {Id} cerrado manualmente por usuario {UserId}.", id, userId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el cierre manual del periodo {Id}.", id);
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
    }
}
