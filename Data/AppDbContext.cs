using ExhibitionEntrySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ExhibitionEntrySystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Pass> Passes => Set<Pass>();
        public DbSet<PassEvent> PassEvents => Set<PassEvent>();
        public DbSet<Visitor> Visitors => Set<Visitor>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Pavilion> Pavilions => Set<Pavilion>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pass>()
                .Property(p => p.Status)
                .HasDefaultValue("Сформирован");

            modelBuilder.Entity<Pass>()
                .HasOne(p => p.User)
                .WithMany(u => u.Passes)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}