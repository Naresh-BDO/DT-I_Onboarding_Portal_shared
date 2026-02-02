using DT_I_Onboarding_Portal.Core.enums;
namespace DT_I_Onboarding_Portal.Services
{
    public sealed class SendEmailResult
    {
        public bool Success { get; init; }
        public EmailErrorType ErrorType { get; init; } = EmailErrorType.None;
        public string? ErrorMessage { get; init; }
        public string? ProviderMessage { get; init; } // raw exception or SMTP status
    }

    public interface IEmailSender
    {
        Task<SendEmailResult> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
