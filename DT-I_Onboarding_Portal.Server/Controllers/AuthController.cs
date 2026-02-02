using System.Security.Claims;
using DT_I_Onboarding_Portal.Core.Models.Dto;
using DT_I_Onboarding_Portal.Data.Stores;
using DT_I_Onboarding_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DT_I_Onboarding_Portal.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EfUserStore _userStore;
        private readonly TokenService _tokenService;

        public AuthController(EfUserStore userStore, TokenService tokenService)
        {
            _userStore = userStore;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userStore.ValidateCredentialsAsync(req.Username, req.Password);
            if (user == null) return Unauthorized(new { message = "Invalid credentials" });

            var roles = _userStore.GetRoles(user);
            var (token, expires) = _tokenService.CreateAccessToken(user, roles);

            var resp = new LoginResponse
            {
                Token = token,
                Expires = expires,
                Roles = roles
            };

            return Ok(resp);
        }

        [HttpGet("whoami")]
        [Authorize(Roles = "Admin")] // Ensures only authenticated users can access
        public async Task<IActionResult> WhoAmI()
        {
            // User.Identity.IsAuthenticated is guaranteed to be true due to [Authorize]
            var username = User.Identity?.Name;

            // Collect all possible role claims
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            return Ok(new { username, roles });
        }
    }
}