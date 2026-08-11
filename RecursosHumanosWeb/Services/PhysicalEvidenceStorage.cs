namespace RecursosHumanosWeb.Services;

public sealed class PhysicalEvidenceStorage : IEvidenceStorage
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public PhysicalEvidenceStorage(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    private string StoragePath
    {
        get
        {
            var configured = _configuration["Evidences:UploadsPath"];
            if (!string.IsNullOrWhiteSpace(configured)) return configured;
            return Path.Combine(_environment.ContentRootPath, "App_Data", "Evidences");
        }
    }

    public async Task<string?> SaveAsync(IFormFile file, int userId)
    {
        if (file.Length <= 0) return null;

        var directory = StoragePath;
        Directory.CreateDirectory(directory);

        var name = $"{userId}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var fullPath = Path.Combine(directory, name);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        return name;
    }

    public void Delete(string? evidence)
    {
        if (string.IsNullOrWhiteSpace(evidence)) return;

        var path = ResolvePhysicalPath(evidence);
        if (path != null)
        {
            File.Delete(path);
        }
    }

    public string? ResolvePhysicalPath(string? evidence)
    {
        if (string.IsNullOrWhiteSpace(evidence)) return null;

        var filename = Path.GetFileName(evidence.Replace('\\', '/'));
        if (string.IsNullOrEmpty(filename)) return null;

        var path = Path.Combine(StoragePath, filename);
        return File.Exists(path) ? path : null;
    }
}
