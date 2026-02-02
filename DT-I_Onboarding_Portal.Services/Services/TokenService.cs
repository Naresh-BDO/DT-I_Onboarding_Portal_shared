using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DT_I_Onboarding_Portal.Core.Models;
using Microsoft.Extensions.Configuration;

//namespace DT_I_Onboarding_Portal.Server.Services
namespace DT_I_Onboarding_Portal.Services
{
    public class TokenService
    {
        private readonly IConfiguration _cfg;

        public TokenService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public (string token, DateTime expires) CreateAccessToken(AppUser user, string[] roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var issuer = _cfg["Jwt:Issuer"]!;
            var audience = _cfg["Jwt:Audience"]!;
            var expiresMinutes = int.TryParse(_cfg["Jwt:ExpireMinutes"], out var m) ? m : 60;
            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }
    }
}