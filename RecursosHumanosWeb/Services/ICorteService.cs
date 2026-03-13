using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services
{
    public interface ICorteService
    {
        Task<Corte> GetActiveCorteAsync();
        Task<bool> HasPendingPermissionsAsync(int corteId);
        Task AutoCloseCorteIfNeededAsync();
        Task<bool> CreateCorteAsync(Corte corte, int userId);
        Task<bool> InactivateCorteAsync(int id, int userId);
        Task<bool> CloseCorteAsync(int id, int userId);
    }
}
