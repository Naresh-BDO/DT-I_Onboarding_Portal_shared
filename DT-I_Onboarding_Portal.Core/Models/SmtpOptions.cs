namespace DT_I_Onboarding_Portal.Core.Models
{
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        // Auth
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // From
        public string FromAddress { get; set; } = "no-reply@example.com";
        public string FromDisplayName { get; set; } = "Onboarding Team";
    }
}
