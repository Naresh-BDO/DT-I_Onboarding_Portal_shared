using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DT_I_Onboarding_Portal.Core.Models;

namespace DT_I_Onboarding_Portal.Data.Stores
{
    /// <summary>
    /// Lightweight user store that reads users and roles from EF Core.
    /// Handles password hashing and credential validation.
    /// </summary>
    public class EfUserStore
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<AppUser> _hasher;

        public EfUserStore(ApplicationDbContext db, IPasswordHasher<AppUser> hasher)
        {
            _db = db;
            _hasher = hasher;

            // Seed demo users when DB is empty (development convenience)
            if (!_db.Roles.Any() && !_db.Users.Any())
            {
                SeedDefaults();
            }
        }

        /// <summary>
        /// Seeds default roles and users for development/testing.
        /// </summary>
        private void SeedDefaults()
        {
            var adminRole = new AppRole { Name = "Admin" };
            var userRole = new AppRole { Name = "User" };
            _db.Roles.AddRange(adminRole, userRole);
            _db.SaveChanges();

            var admin = new AppUser
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = _hasher.HashPassword(null!, "AdminPass123!")
            };

            var user = new AppUser
            {
                Username = "user",
                Email = "user@example.com",
                PasswordHash = _hasher.HashPassword(null!, "UserPass123!")
            };

            _db.Users.AddRange(admin, user);
            _db.SaveChanges();

            _db.UserRoles.AddRange(
                new AppUserRole { UserId = admin.Id, RoleId = adminRole.Id },
                new AppUserRole { UserId = admin.Id, RoleId = userRole.Id },
                new AppUserRole { UserId = user.Id, RoleId = userRole.Id }
            );
            _db.SaveChanges();
        }

        /// <summary>
        /// Validates user credentials by verifying the hashed password.
        /// </summary>
        public async Task<AppUser?> ValidateCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var user = await _db.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        /// <summary>
        /// Gets all roles assigned to a user.
        /// </summary>
        public string[] GetRoles(AppUser user)
        {
            return user.UserRoles?
                .Select(ur => ur.Role?.Name ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray() ?? Array.Empty<string>();
        }
    }
}