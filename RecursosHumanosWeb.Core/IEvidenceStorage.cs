namespace RecursosHumanosWeb.Services;

public interface IEvidenceStorage
{
    Task<string?> SaveAsync(IFormFile file, int userId);
    void Delete(string? evidence);
    string? ResolvePhysicalPath(string? evidence);
}
