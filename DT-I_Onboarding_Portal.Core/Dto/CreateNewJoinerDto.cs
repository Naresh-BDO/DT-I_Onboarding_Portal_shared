
using System.ComponentModel.DataAnnotations;
namespace DT_I_Onboarding_Portal.Core.Models.Dto
{
    public class CreateNewJoinerDto
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Department { get; set; }

        [StringLength(120)]
        public string? ManagerName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }  // Local date; we’ll store as UTC date
    }
}
