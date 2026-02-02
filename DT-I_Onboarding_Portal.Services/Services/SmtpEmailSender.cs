
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using DT_I_Onboarding_Portal.Core.enums;
using DT_I_Onboarding_Portal.Core.Models;

namespace DT_I_Onboarding_Portal.Services

{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task<SendEmailResult> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            // Basic configuration validation
            if (string.IsNullOrWhiteSpace(_options.Host) ||
                _options.Port <= 0 ||
                string.IsNullOrWhiteSpace(_options.FromAddress))
            {
                return new SendEmailResult
                {
                    Success = false,
                    ErrorType = EmailErrorType.ConfigurationError,
                    ErrorMessage = "SMTP configuration is incomplete (Host/Port/FromAddress).",
                };
            }

            try
            {
                var from = new MailAddress(_options.FromAddress, _options.FromDisplayName, Encoding.UTF8);
                MailAddress to;
                try
                {
                    to = new MailAddress(toEmail);
                }
                catch (FormatException fe)
                {
                    return new SendEmailResult
                    {
                        Success = false,
                        ErrorType = EmailErrorType.InvalidRecipientAddress,
                        ErrorMessage = "Recipient email format is invalid.",
                        ProviderMessage = fe.Message
                    };
                }


                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    UseDefaultCredentials = false,                       //  make sure your credentials are used
                    Credentials = new NetworkCredential(_options.Username, _options.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,         //  explicit
                    Timeout = 15000
                };


                using var message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    Priority = MailPriority.Normal
                };

                await client.SendMailAsync(message);

                return new SendEmailResult { Success = true };
            }
            catch (SmtpFailedRecipientException sfr)
            {
                return new SendEmailResult
                {
                    Success = false,
                    ErrorType = EmailErrorType.RecipientRejected,
                    ErrorMessage = "Recipient mailbox rejected the message (invalid inbox or policy).",
                    ProviderMessage = $"{sfr.StatusCode}: {sfr.Message}"
                };
            }
            catch (SmtpException se)
            {
                // Map common SMTP errors
                var lower = se.Message.ToLowerInvariant();
                var type =
                    lower.Contains("authentication") || lower.Contains("535") || lower.Contains("auth")
                        ? EmailErrorType.AuthenticationFailed
                    : lower.Contains("timed out") || lower.Contains("timeout")
                        ? EmailErrorType.Timeout
                    : lower.Contains("unavailable") || lower.Contains("cannot connect") || lower.Contains("connection")
                        ? EmailErrorType.SmtpConnectionFailed
                    : EmailErrorType.SmtpSendFailed;

                return new SendEmailResult
                {
                    Success = false,
                    ErrorType = type,
                    ErrorMessage = "SMTP operation failed.",
                    ProviderMessage = se.Message
                };
            }
            catch (TaskCanceledException tce)
            {
                return new SendEmailResult
                {
                    Success = false,
                    ErrorType = EmailErrorType.Timeout,
                    ErrorMessage = "SMTP send timed out.",
                    ProviderMessage = tce.Message
                };
            }
            catch (Exception ex)
            {
                return new SendEmailResult
                {
                    Success = false,
                    ErrorType = EmailErrorType.Unknown,
                    ErrorMessage = "Unexpected error while sending email.",
                    ProviderMessage = ex.Message
                };
            }
        }
    }
}
