using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EMSWebApi.Domain.Entities;
using EMSWebApi.Domain.Identity;

namespace EMSWebApi.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Notice> Notices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LeaveApplication>()
                .HasOne(la => la.Applicant)
                .WithMany()
                .HasForeignKey(la => la.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaveApplication>()
                .HasOne(la => la.ApprovedByUser)
                .WithMany()
                .HasForeignKey(la => la.ApprovedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LeaveApplication>()
                .HasOne(la => la.LeaveType)
                .WithMany(lt => lt.LeaveApplications)
                .HasForeignKey(la => la.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notice>().HasIndex(n => n.CreatedAt);
            builder.Entity<Notice>().HasIndex(n => n.ExpiresAt);
        }
    }
}
