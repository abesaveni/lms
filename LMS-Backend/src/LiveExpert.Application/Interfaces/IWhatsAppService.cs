using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveExpert.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, List<string> parameters);
        Task<bool> SendOTPAsync(string phoneNumber, string otp);
        Task<bool> SendBulkMessageAsync(List<string> phoneNumbers, string message);
    }
}