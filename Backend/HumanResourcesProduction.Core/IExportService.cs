using System.Threading.Tasks;

namespace HumanResourcesProduction.Services
{
    public interface IExportService
    {
        Task<byte[]> GenerateCutoffExcelAsync(int cutoffId);
        Task<(byte[] Content, string FileName)> GenerateCutoffCsvAsync(int cutoffId);
        Task<(byte[] Content, string FileName)> GenerateCutoffJsonAsync(int cutoffId);
        Task<(byte[] Content, string FileName)> GenerateCutoffXmlAsync(int cutoffId);
    }
}
