using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Podcast.Infrastructure.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordRecoveryEmailAsync(string toEmail, string token);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string confirmationUrl);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
