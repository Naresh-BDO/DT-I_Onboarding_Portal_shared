using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using DT_I_Onboarding_Portal.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DT_I_Onboarding_Portal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions
            <ApplicationDbContext> opts) : base(opts) { }

        public DbSet<AppUser> Users { get; set; } = default!;
        public DbSet<AppRole> Roles { get; set; } = default!;
        public DbSet<AppUserRole> UserRoles { get; set; } = default!;
        public DbSet<NewJoiner> NewJoiners => Set<NewJoiner>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Server=GN401;Database=Bdo;Trusted_Connection=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary key for AppUserRole
            modelBuilder.Entity<AppUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configure relationships
            modelBuilder.Entity<AppUserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on Username
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            base.OnModelCreating(modelBuilder);

            // Unique-ish index to avoid duplicate entries for same person/date
            modelBuilder.Entity<NewJoiner>()
                .HasIndex(nj => new { nj.Email, nj.StartDate })
                .IsUnique();

            // make Email normalized comparison if needed
            modelBuilder.Entity<NewJoiner>()
                .Property(nj => nj.Email)
                .IsUnicode(false); // typical emails are ASCII; adjust as needed

        }

    }
}