using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LearningProject.Models;

namespace LearningProject.Data
{
    public class LearningProjectContext : DbContext
    {
        public LearningProjectContext (DbContextOptions<LearningProjectContext> options)
            : base(options)
        {
        }
        public DbSet<LearningProject.Models.User> User { get; set; } = default!;
        public DbSet<LearningProject.Models.Departamente> Departamente { get; set; } = default!;
        public DbSet<LearningProject.Models.Roluri> Roluri { get; set; } = default!;
        public DbSet<LearningProject.Models.Claim> Claim { get; set; } = default!;
        public DbSet<LearningProject.Models.ErrorLog> ErrorLogs { get; set; } = default!;
        public DbSet<LearningProject.Models.Cereri> Cereri {  get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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


        }

    }
}
