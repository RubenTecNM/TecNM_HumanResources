using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services
{
    public interface ICorteService
    {
        Task<Cutoff> GetActiveCorteAsync();
        Task<Cutoff> GetLastCorteAsync();
        Task<bool> HasPendingPermissionsAsync(int corteId);
        Task AutoCloseCorteIfNeededAsync();
        Task<bool> CreateCorteAsync(Cutoff cutoff, int userId);
        Task<bool> InactivateCorteAsync(int id, int userId);
        Task<bool> CloseCorteAsync(int id, int userId);
    }
}
