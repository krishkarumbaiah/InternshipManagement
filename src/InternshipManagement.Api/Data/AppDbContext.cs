using InternshipManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Master tables
        public DbSet<TypeCdmt> TypeCdmt { get; set; } = null!;
        public DbSet<RowStatus> RowStatus { get; set; } = null!;

        // Existing entities
        public DbSet<Batch> Batches { get; set; } = null!;
        public DbSet<UserBatch> UserBatches { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Attendance> Attendance { get; set; } = null!;
        public DbSet<Meeting> Meetings { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<UserCourse> UserCourses { get; set; } = null!;
        public DbSet<OtpStore> Otps { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<Leave> Leaves { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserBatch mapping
            builder.Entity<UserBatch>()
                .HasOne(ub => ub.User)
                .WithMany()
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserBatch>()
                .HasOne(ub => ub.Batch)
                .WithMany(b => b.UserBatches)
                .HasForeignKey(ub => ub.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TypeCdmt>().ToTable("TypeCdmt");
            builder.Entity<RowStatus>().ToTable("RowStatus");

            // UserCourse mapping
            builder.Entity<UserCourse>()
                .HasIndex(uc => new { uc.UserId, uc.CourseId })
                .IsUnique();

            builder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(uc => uc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course → Batch mapping
            builder.Entity<UserCourse>()
                .HasOne(uc => uc.Batch)
                .WithMany(b => b.UserCourses)
                .HasForeignKey(uc => uc.BatchId)
                .OnDelete(DeleteBehavior.Restrict);



            // Leave mapping
            builder.Entity<Leave>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Notification → Meeting (cascade)
            builder.Entity<Notification>()
                .HasOne(n => n.Meeting)
                .WithMany()
                .HasForeignKey(n => n.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Notification → Batch (restrict to avoid multiple cascade paths)
            builder.Entity<Notification>()
                .HasOne(n => n.Batch)
                .WithMany()
                .HasForeignKey(n => n.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
