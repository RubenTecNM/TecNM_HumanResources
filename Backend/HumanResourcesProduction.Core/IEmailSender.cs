using System.Threading.Tasks;
using HumanResourcesProduction.Models.ViewModels.ResetPassword;

namespace HumanResourcesProduction.Models
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendTokenEmailAsync(RequestTokenEmailModel model);
    }
}
