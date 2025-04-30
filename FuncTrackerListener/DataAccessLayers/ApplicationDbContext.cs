using EntityFramework.Exceptions.SqlServer;
using Listener.Models;
using Microsoft.EntityFrameworkCore;

namespace trackerlistener.DataAccessLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<TrackerEvent> TrackerEvents { get; set; }
    public virtual DbSet<Vehicle> Vehicles { get; set; }
    public virtual DbSet<Driver> Drivers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseExceptionProcessor();
        //optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackerEvent>(entity =>
        {
            entity.ToTable("TrackerEvent");
            entity.HasKey(e => e.id);

            entity.Property(e => e.id).ValueGeneratedOnAdd();
            entity.Property(e => e.name)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicle");
            entity.HasKey(e => e.vehicle_id);

            entity.Property(e => e.vehicle_id).ValueGeneratedOnAdd();
            entity.Property(e => e.vehicle_name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.vehicle_numberplate)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.gps_phone)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.createdat)
                .IsRequired();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Drivers");
            entity.HasKey(e => e.driver_id);

            entity.Property(e => e.driver_id).ValueGeneratedOnAdd();
            entity.Property(e => e.driver_name)
                .IsRequired()
                .HasMaxLength(255);
        });

    }
}