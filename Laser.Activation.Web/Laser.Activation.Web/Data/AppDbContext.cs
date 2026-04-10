using Laser.Activation.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Laser.Activation.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ActivationRecord> ActivationRecords => Set<ActivationRecord>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ActivationRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("ActivationRecord");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Users");
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("LoginLog");
        });
    }
}
