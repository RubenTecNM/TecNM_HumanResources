using System.Threading.Tasks;

namespace RecursosHumanosWeb.Services
{
    public interface IExportService
    {
        Task<byte[]> GenerateCorteExcelAsync(int corteId);
        Task<(byte[] Content, string FileName)> GenerateCorteCsvAsync(int corteId);
        Task<(byte[] Content, string FileName)> GenerateCorteJsonAsync(int corteId);
        Task<(byte[] Content, string FileName)> GenerateCorteXmlAsync(int corteId);
    }
}
