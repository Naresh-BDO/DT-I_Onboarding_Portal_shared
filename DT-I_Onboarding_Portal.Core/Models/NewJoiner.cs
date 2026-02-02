
using System.ComponentModel.DataAnnotations;

namespace DT_I_Onboarding_Portal.Core.Models
{
    public class NewJoiner
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Department { get; set; }

        [StringLength(120)]
        public string? ManagerName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Email sending audit
        public DateTime? WelcomeEmailSentAtUtc { get; set; }
        [StringLength(400)]
        public string? LastSendError { get; set; }
    }
}
