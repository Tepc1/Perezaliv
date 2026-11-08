using Microsoft.EntityFrameworkCore;
using PCRETAIL.Models;

namespace PCRETAIL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<PcConfiguration> PcConfigurations { get; set; }
        public DbSet<Service> Services { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed roles (optional)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Master" },
                new Role { Id = 3, Name = "Client" }
            );

            // User <-> Role
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // PcConfiguration -> User
            modelBuilder.Entity<PcConfiguration>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.PcConfigurations)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many between PcConfiguration and Component
            // привязка к существующей таблице ConfigurationComponents (ComponentsId, PcConfigurationsId)
            modelBuilder.Entity<PcConfiguration>()
                .HasMany(pc => pc.Components)
                .WithMany(c => c.PcConfigurations)
                .UsingEntity<Dictionary<string, object>>(
                    "ConfigurationComponents",
                    j => j
                        .HasOne<Component>()
                        .WithMany()
                        .HasForeignKey("ComponentsId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<PcConfiguration>() 
                        .WithMany()
                        .HasForeignKey("PcConfigurationsId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("ComponentsId", "PcConfigurationsId");
                    });

            // Service -> Client (User) and Service -> Master (User)
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Client)
                .WithMany(u => u.ClientServices)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.Master)
                .WithMany(u => u.MasterServices)
                .HasForeignKey(s => s.MasterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}