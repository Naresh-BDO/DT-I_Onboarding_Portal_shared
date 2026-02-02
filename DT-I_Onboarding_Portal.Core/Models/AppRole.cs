using System.Collections.Generic;

namespace DT_I_Onboarding_Portal.Core.Models
{
    public class AppRole
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    }
}