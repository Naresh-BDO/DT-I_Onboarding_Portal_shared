
namespace DT_I_Onboarding_Portal.Core.Models.Dto
{
    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public DateTime Expires { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
