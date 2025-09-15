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
        public DbSet<TypeCdmt> TypeCdmt { get; set; }= null!;
        public DbSet<RowStatus> RowStatus { get; set; }= null!;

        public DbSet<Batch> Batches { get; set; }= null!;
        public DbSet<UserBatch> UserBatches { get; set; }= null!;
        public DbSet<Question> Questions { get; set; }
        public DbSet<Attendance> Attendance { get; set; }= null!;
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
        }
    }
}
