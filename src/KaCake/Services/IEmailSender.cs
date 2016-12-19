using System.Threading.Tasks;

namespace KaCake.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
