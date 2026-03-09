using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;


namespace BookGuide.API.Data
{
    public class BookGuideDbContext : DbContext
    {
        public BookGuideDbContext(DbContextOptions<BookGuideDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserBook> UserBooks => Set<UserBook>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Notification>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(240).IsRequired();
                e.Property(x => x.Message).HasMaxLength(600).IsRequired();
                e.Property(x => x.Type).HasMaxLength(100).IsRequired();
                e.Property(x => x.IsRead).HasDefaultValue(false);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.UserBook)
                 .WithMany()
                 .HasForeignKey(x => x.UserBookId)
                 .OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(x => new { x.UserId, x.IsRead });
                e.HasIndex(x => new { x.UserId, x.CreatedAt });
            });

            modelBuilder.Entity<Achievement>(e =>
            {
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Code).HasMaxLength(50);
                e.Property(x => x.Title).HasMaxLength(200);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.Icon).HasMaxLength(50);
            });

            modelBuilder.Entity<UserAchievement>(e =>
            {
                e.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Achievement)
                    .WithMany(a => a.Users)
                    .HasForeignKey(x => x.AchievementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        

modelBuilder.Entity<PasswordResetToken>()
    .HasIndex(x => x.TokenHash)
    .IsUnique();

        }
    }
}
