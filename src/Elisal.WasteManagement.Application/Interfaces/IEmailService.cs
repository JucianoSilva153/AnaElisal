using System.Threading.Tasks;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendCredentialsEmailAsync(string to, string name, string password);
    Task SendResetPasswordEmailAsync(string to, string name, string token);
}
