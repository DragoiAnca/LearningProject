using LearningProject.Models;
using LearningProject.Models.DraftModel;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Data
{
    public class LearningProjectContext : DbContext
    {
        public LearningProjectContext(DbContextOptions<LearningProjectContext> options)
            : base(options)
        {
        }
        public DbSet<LearningProject.Models.User> User { get; set; } = default!;
        public DbSet<LearningProject.Models.Departamente> Departamente { get; set; } = default!;
        public DbSet<LearningProject.Models.Roluri> Roluri { get; set; } = default!;
        public DbSet<LearningProject.Models.Claim> Claim { get; set; } = default!;
        public DbSet<LearningProject.Models.ErrorLog> ErrorLogs { get; set; } = default!;
        public DbSet<LearningProject.Models.Cereri> Cereri { get; set; } = default!;
        public DbSet<LearningProject.Models.V_employees> V_employees { get; set; } = default!;

        public DbSet<LearningProject.Models.Student> Students { get; set; }
        public DbSet<LearningProject.Models.Enrollment> Enrollments { get; set; }
        public DbSet<LearningProject.Models.Course> Courses { get; set; }

        public DbSet<LearningProject.Models.Signature> Signatures { get; set; }
        public DbSet<LearningProject.Models.DraftModel.CerereFile> CerereFile { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);

            // User → Roluri (many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.roluri)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.roluriID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete if desired

            // User → Departamente (many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Departamente)
                .WithMany()
                .HasForeignKey(u => u.id_departament)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cereri>()
            .HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cereri>()
                .HasOne(c => c.DeletedBy)
                .WithMany()
                .HasForeignKey(c => c.DeletedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
    .HasOne(u => u.Departamente)
    .WithMany()
    .HasForeignKey(u => u.id_departament)
    .IsRequired(false);

            modelBuilder.Entity<CerereFile>()
    .HasOne(f => f.Cerere)
    .WithMany(c => c.Files)
    .HasForeignKey(f => f.CereriId)
    .OnDelete(DeleteBehavior.Cascade);

            //view reference
            modelBuilder.Entity<V_employees>().HasNoKey().ToView("V_employees");

        }
    }
}
