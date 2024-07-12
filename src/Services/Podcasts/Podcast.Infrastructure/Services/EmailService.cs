using Microsoft.Extensions.Configuration;
using System.Reflection;
using Podcast.Infrastructure.Services.Interfaces;
using Azure.Communication.Email;
using Azure;

namespace Podcast.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _baseUrl;
        
        private readonly string _physicalAddress;

        private const string _logoRelativePath = "/favicon.png";
        private string LogoUrl
        {
            get => _baseUrl + _logoRelativePath;
        }

        private const string _templateFileRelativePath = "template.generic.en.html";
        private static string TemplateFileAbsolutePath
        {
            get => Path.Combine(
                path1: Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                path2: _templateFileRelativePath
            );
        }

        private readonly EmailClient _emailClient;

        private const string _logoPathPlaceholder = "{LogoPath}";
        private const string _baseUrlPlaceholder = "{BaseUrl}";
        private const string _callToActionPlaceholder = "{CallToAction}";
        private const string _linkToActionPlaceholder = "{LinkToAction}";
        private const string _actionStyle = "{ActionStyle}";
        private const string _messagePlaceholder = "{Message}";
        private const string _addressPlaceholder = "{Address}";

        public EmailService(IConfiguration configuration)
        {
            var config = configuration.GetSection("AppSettings:Email") ??
                throw new NullReferenceException("'AppSettings:Email' not found.");
            var applicationUrlConfig = configuration.GetSection("AppSettings:ApplicationUrl") ??
                throw new NullReferenceException("'AppSettings:ApplicationUrl' not found.");
#if DEBUG || LOCALDEV
            _baseUrl = applicationUrlConfig["development"] ??
                throw new NullReferenceException("Application Url 'AppSettings:ApplicationUrl:development' not found.");
#else
            _baseUrl = applicationUrlConfig["production"] ??
                throw new NullReferenceException("Application Url 'AppSettings:ApplicationUrl:production' not found.");
#endif
            _physicalAddress = config["PhysicalAddress"] ??
                throw new NullReferenceException("Physical Address 'AppSettings:Email:PhysicalAddress' not found.");

            string connectionString = config["ConnectionString"] ??
                throw new NullReferenceException("ConnectionString 'AppSettings:Email:ConnectionString' not found.");
            _emailClient = new EmailClient(connectionString);
        }

        public async Task<bool> SendPasswordRecoveryEmailAsync(string toEmail, string token)
        {
            string link = string.Format("{0}/auth/reset-password?email={1}&token={2}", _baseUrl, toEmail, token);
            const string message = $@"
                <p style='margin: 0;'>
                  You have requested a password reset on the GPT Bot App. Please click the link below to reset your password. If you did not requested this or you did by accident, please ignore this message. 
                </p>";
            string htmlContent = MergeHtmlContent(message, link, "Reset Password");

            return await SendEmailAsync(toEmail, "Reset your password", htmlContent);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string confirmationUrl)
        {
            string link = $"{_baseUrl}{confirmationUrl}";
            const string message = $@"
                <p style='margin: 0;'>
                  Hi and welcome to AISleepod;
                </p>";
            string htmlContent = MergeHtmlContent(message, link, "Open AISleepod");

            //return await SendEmailAsync(toEmail, "Welcome to AISleepod", htmlContent);
            SendEmail(toEmail, "Welcome to AISleepod", htmlContent);
            return await Task.FromResult(true);
        }

        private string MergeHtmlContent(string message, string linkToAction = "", string callToAction = "")
        {
            try
            {
                string content = File.ReadAllText(TemplateFileAbsolutePath);
                content = content.Replace(_baseUrlPlaceholder, _baseUrl);
                content = content.Replace(_logoPathPlaceholder, LogoUrl);
                content = content.Replace(_messagePlaceholder, message);
                content = content.Replace(_addressPlaceholder, _physicalAddress);
                if (!string.IsNullOrEmpty(linkToAction) && !string.IsNullOrEmpty(callToAction))
                {
                    content = content.Replace(_linkToActionPlaceholder, linkToAction);
                    content = content.Replace(_callToActionPlaceholder, callToAction);
                }
                else
                {
                    content = content.Replace(_actionStyle, "display:none;");
                }

                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while merging html with content: {ex.Message}");
                return string.Empty;
            }
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                EmailSendOperation emailSendOperation = _emailClient.Send(
                    WaitUntil.Completed,
                    senderAddress: "DoNotReply@ba96fc62-534e-4ede-b8bd-92a70c667804.azurecomm.net",
                    recipientAddress: toEmail,
                    subject: subject,
                    htmlContent: body,
                    plainTextContent: body);

                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }

        public Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                EmailSendOperation emailSendOperation = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: "DoNotReply@ba96fc62-534e-4ede-b8bd-92a70c667804.azurecomm.net",
                recipientAddress: toEmail,
                subject: subject,
                htmlContent: body,
                plainTextContent: body);

                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");

                return Task.FromResult(emailSendOperation.Value.Status == EmailSendStatus.Succeeded);
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");

                return Task.FromResult(false);
            }
        }
    }
}
