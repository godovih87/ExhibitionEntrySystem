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
        public DbSet<SecurityLogin> SecurityLogins => Set<SecurityLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pass>()
                .Property(p => p.Status)
                .HasDefaultValue("Сформирован");
        }
    }
}
