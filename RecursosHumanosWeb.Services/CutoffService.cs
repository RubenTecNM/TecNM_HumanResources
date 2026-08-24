using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;

namespace RecursosHumanosWeb.Services
{
    public class CutoffService : ICutoffService
    {
        private readonly HumanResourcesContext _context;
        private readonly ILogger<CutoffService> _logger;

        public CutoffService(HumanResourcesContext context, ILogger<CutoffService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cutoff> GetActiveCutoffAsync()
        {
            return await _context.Cutoffs
                .Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open)
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Cutoff> GetLastCutoffAsync()
        {
            return await _context.Cutoffs
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingPermissionsAsync(int cutoffId)
        {
            return await _context.Permissions.AnyAsync(p => p.CutoffId == cutoffId && p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending && p.Status);
        }

        public async Task AutoCloseCutoffIfNeededAsync()
        {
            try
            {
                var cortesExpirados = await _context.Cutoffs
                    .Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open && c.EndDate.Date < DateTime.Now.Date)
                    .ToListAsync();

                int rootId = 1; // Root User ID (firma las acciones automáticas del sistema)

                foreach (var activeCutoff in cortesExpirados)
                {
                    // Usar contexto para activar auditoría de sistema
                    _context._auditOriginOverride = (int)OriginTypeEnum.AutomaticSystem;

                    // 1. Rechazar permissions pendientes
                    var pendientes = await _context.Permissions
                        .Where(p => p.CutoffId == activeCutoff.Id && p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending && p.Status)
                        .ToListAsync();

                    if (pendientes.Any())
                    {
                        foreach (var p in pendientes)
                        {
                            p.PermissionStatusTypeId = (int)PermissionStatusEnum.Rejected;
                            p.WithPay = false; // Rechazo automático
                            p.AuthorizingUserId = rootId;
                            p.AuthorizedAt = DateTime.Now;
                            p.UpdatedByUserId = rootId;
                            p.UpdatedAt = DateTime.Now;
                        }
                        _context.UpdateRange(pendientes);
                    }

                    // 2. Cerrar Cutoff
                    activeCutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Closed;
                    activeCutoff.UpdatedAt = DateTime.Now;
                    activeCutoff.UpdatedByUserId = rootId;

                    _context.Update(activeCutoff);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cutoff {Id} cerrado automáticamente por expiración.", activeCutoff.Id);
                }

                // 2do barrido: pendientes en cortes YA cerrados o invalidados.
                // Solo puede existir por datos externos al código (SQL directo,
                // registros previos a la cascada): se invalidan para hacerlos
                // visibles como anomalía, sin autorizante.
                var huerfanos = await _context.Permissions
                    .Where(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending
                             && p.Status
                             && (p.Cutoff!.CutoffStatusTypeId == (int)CutoffStatusEnum.Closed
                              || p.Cutoff!.CutoffStatusTypeId == (int)CutoffStatusEnum.Invalidated))
                    .ToListAsync();

                if (huerfanos.Any())
                {
                    _context._auditOriginOverride = (int)OriginTypeEnum.AutomaticSystem;

                    foreach (var p in huerfanos)
                    {
                        p.PermissionStatusTypeId = (int)PermissionStatusEnum.InvalidatedByPeriod;
                        p.WithPay = false;
                        p.UpdatedByUserId = rootId;
                        p.UpdatedAt = DateTime.Now;
                    }
                    _context.UpdateRange(huerfanos);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Se invalidaron {Count} permisos pendientes de cortes ya cerrados.", huerfanos.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el cierre automático de corte.");
            }
        }

        public async Task<bool> CreateCutoffAsync(Cutoff newCutoff, int userId)
        {
            var activeCutoff = await GetActiveCutoffAsync();
            DateTime ahora = DateTime.Now;

            // Transacción para asegurar consistencia
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (activeCutoff != null)
                    {
                        if (await HasPendingPermissionsAsync(activeCutoff.Id))
                        {
                            return false; // No se puede crear si hay pendientes
                        }

                        activeCutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Closed;
                        activeCutoff.UpdatedAt = ahora;
                        activeCutoff.UpdatedByUserId = userId;
                        _context.Update(activeCutoff);
                    }

                    // Validación de traslape con cualquier cutoff existente
                    bool traslapa = await _context.Cutoffs.AnyAsync(c => 
                        (newCutoff.StartDate >= c.StartDate && newCutoff.StartDate <= c.EndDate) ||
                        (newCutoff.EndDate >= c.StartDate && newCutoff.EndDate <= c.EndDate) ||
                        (c.StartDate >= newCutoff.StartDate && c.StartDate <= newCutoff.EndDate));

                    if (traslapa)
                    {
                        return false; // El periodo se encima con uno existente
                    }

                    newCutoff.StartDate = newCutoff.StartDate.Date;
                    newCutoff.EndDate = newCutoff.EndDate.Date.AddDays(1).AddTicks(-1);
                    newCutoff.CreatedByUserId = userId;
                    newCutoff.UpdatedByUserId = userId;
                    newCutoff.CreatedAt = ahora;
                    newCutoff.UpdatedAt = ahora;
                    newCutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Open;

                    _context.Add(newCutoff);
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

        public async Task<bool> InactivateCutoffAsync(int id, int userId)
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Invalidar Periodo
                    cutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Invalidated;
                    cutoff.UpdatedAt = DateTime.Now;
                    cutoff.UpdatedByUserId = userId;
                    _context.Update(cutoff);

                    // 2. Invalidar Permissions relacionados (Cambio de estado masivo)
                    var permissions = await _context.Permissions
                        .Where(p => p.CutoffId == id && p.Status)
                        .ToListAsync();

                    if (permissions.Any())
                    {
                        foreach (var p in permissions)
                        {
                            p.PermissionStatusTypeId = (int)PermissionStatusEnum.InvalidatedByPeriod;
                            p.Status = false; // Borrado lógico como solicitó el user
                            p.UpdatedAt = DateTime.Now;
                            p.UpdatedByUserId = userId;
                        }
                        _context.UpdateRange(permissions);
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

        public async Task<bool> CloseCutoffAsync(int id, int userId)
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null || cutoff.CutoffStatusTypeId != (int)CutoffStatusEnum.Open) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Cerrar Periodo
                    cutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Closed;
                    cutoff.UpdatedAt = DateTime.Now;
                    cutoff.UpdatedByUserId = userId;
                    _context.Update(cutoff);

                    // 2. Rechazar automáticamente permissions pendientes
                    var pendientes = await _context.Permissions
                        .Where(p => p.CutoffId == id && p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending && p.Status)
                        .ToListAsync();

                    if (pendientes.Any())
                    {
                        foreach (var p in pendientes)
                        {
                            p.PermissionStatusTypeId = (int)PermissionStatusEnum.Rejected;
                            p.WithPay = false; // Sin goce por cierre manual
                            p.AuthorizedAt = DateTime.Now;
                            p.AuthorizingUserId = userId;
                            p.UpdatedByUserId = userId;
                            p.UpdatedAt = DateTime.Now;
                        }
                        _context.UpdateRange(pendientes);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation("Cutoff {Id} cerrado manualmente por user {UserId}.", id, userId);
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
