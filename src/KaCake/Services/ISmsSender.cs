using System.Threading.Tasks;

namespace KaCake.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
