using aspteamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace aspteamAPI.context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets for each table
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<JobSeekerAccount> JobSeekerAccounts { get; set; }
        public DbSet<CompanyAccount> CompanyAccounts { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique index for Follow 
            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.JobSeekerId, f.CompanyId })
                .IsUnique();

            // -------------------
            // Relationships
            // -------------------

            // JobSeekerAccount → User
            modelBuilder.Entity<JobSeekerAccount>()
                .HasOne(j => j.User)
                .WithOne(u => u.JobSeekerAccount)
                .HasForeignKey<JobSeekerAccount>(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CompanyAccount → User
            modelBuilder.Entity<CompanyAccount>()
                .HasOne(c => c.User)
                .WithOne(u => u.CompanyAccount)
                .HasForeignKey<CompanyAccount>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job → CompanyAccount
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.PostedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // Follow → JobSeekerAccount & CompanyAccount
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.JobSeeker)
                .WithMany(js => js.Follows)
                .HasForeignKey(f => f.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);  // prevent cycle

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Company)
                .WithMany(c => c.Followers)
                .HasForeignKey(f => f.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);  // prevent cycle

            // JobApplication → Job, JobSeekerAccount, CV
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Applicant)
                .WithMany(js => js.Applications)
                .HasForeignKey(ja => ja.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);  // prevent cascade paths

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Restrict);  // prevent cascade paths

            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Cv)
                .WithMany(c => c.Applications)
                .HasForeignKey(ja => ja.CvId)
                .OnDelete(DeleteBehavior.Restrict);  // safe

            // Evaluation → JobApplication, CV
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.JobApplication)
                .WithOne(ja => ja.Evaluation)
                .HasForeignKey<Evaluation>(e => e.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade); // safe one-to-one

            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Cv)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.CvId)
                .OnDelete(DeleteBehavior.Restrict); // avoid cascade path


            // Fix decimal precision for salary ranges
            modelBuilder.Entity<Job>()
                .Property(j => j.MinSalaryRange)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Job>()
                .Property(j => j.MaxSalaryRange)
                .HasPrecision(18, 2);




            modelBuilder.Entity<BlacklistedToken>(entity =>
            {
                entity.HasIndex(e => e.TokenId).IsUnique();
                entity.HasIndex(e => e.ExpiresAt);
            });

            // Configure PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => new { e.UserId, e.IsUsed });

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            base.OnModelCreating(modelBuilder);

        }





    }
    }

