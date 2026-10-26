using HumanResourcesProduction.Models;

namespace HumanResourcesProduction.Services
{
    public interface ICutoffService
    {
        Task<Cutoff> GetActiveCutoffAsync();
        Task<Cutoff> GetLastCutoffAsync();
        Task<bool> HasPendingPermissionsAsync(int cutoffId);
        Task AutoCloseCutoffIfNeededAsync();
        Task<bool> CreateCutoffAsync(Cutoff cutoff, int userId);
        Task<bool> CloseCutoffAsync(int id, int userId);
    }
}
