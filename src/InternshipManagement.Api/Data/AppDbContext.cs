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



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Existing configs
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
        }
    }
}
