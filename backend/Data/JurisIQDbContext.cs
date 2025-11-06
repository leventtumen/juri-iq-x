using Microsoft.EntityFrameworkCore;
using JurisIQ.Backend.Models;

namespace JurisIQ.Backend.Data
{
    public class JurisIQDbContext : DbContext
    {
        public JurisIQDbContext(DbContextOptions<JurisIQDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentKeyword> DocumentKeywords { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<DocumentView> DocumentViews { get; set; }
        public DbSet<SearchHistory> SearchHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            // UserDevice configurations
            modelBuilder.Entity<UserDevice>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.DeviceId }).IsUnique();
                entity.Property(e => e.FirstLoginAt).HasDefaultValueSql("datetime('now')");
            });

            // FailedLoginAttempt configurations
            modelBuilder.Entity<FailedLoginAttempt>(entity =>
            {
                entity.Property(e => e.AttemptAt).HasDefaultValueSql("datetime('now')");
            });

            // Document configurations
            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt);
                entity.HasIndex(e => e.FileType);
                entity.HasIndex(e => e.CourtName);
                entity.HasIndex(e => e.CaseNumber);
                entity.HasIndex(e => e.DecisionDate);
            });

            // DocumentKeyword configurations
            modelBuilder.Entity<DocumentKeyword>(entity =>
            {
                entity.HasIndex(e => new { e.DocumentId, e.Keyword }).IsUnique();
                entity.Property(e => e.RelevanceScore).HasDefaultValue(0.0);
                entity.Property(e => e.Frequency).HasDefaultValue(1);
            });

            // Bookmark configurations
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.DocumentId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            // DocumentView configurations
            modelBuilder.Entity<DocumentView>(entity =>
            {
                entity.Property(e => e.ViewedAt).HasDefaultValueSql("datetime('now')");
                entity.HasIndex(e => new { e.DocumentId, e.UserId });
                entity.HasIndex(e => e.ViewedAt);
            });

            // SearchHistory configurations
            modelBuilder.Entity<SearchHistory>(entity =>
            {
                entity.Property(e => e.SearchedAt).HasDefaultValueSql("datetime('now')");
                entity.HasIndex(e => new { e.UserId, e.SearchedAt });
            });

            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass!2345"),
                    FirstName = "Admin",
                    LastName = "User",
                    IsAdmin = true,
                    SubscriptionType = SubscriptionType.Pro,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}