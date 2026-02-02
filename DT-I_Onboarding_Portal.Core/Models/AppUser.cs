namespace DT_I_Onboarding_Portal.Core.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    }
}
