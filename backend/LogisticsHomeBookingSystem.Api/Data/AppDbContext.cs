using LogisticsHomeBookingSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsHomeBookingSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<VehicleAvailability> VehicleAvailabilities => Set<VehicleAvailability>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).HasMaxLength(30);
            entity.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.Property(d => d.Status).HasConversion<string>();
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(v => v.Status).HasConversion<string>();
            entity.HasOne(v => v.Driver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.Status).HasConversion<string>();
            entity.HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Driver)
                .WithMany()
                .HasForeignKey(b => b.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(b => b.Vehicle)
                .WithMany()
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VehicleAvailability>(entity =>
        {
            entity.HasOne(v => v.Vehicle)
                .WithMany(v => v.Availabilities)
                .HasForeignKey(v => v.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Status).HasConversion<string>();
            entity.HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.Property(p => p.BaseRate).HasPrecision(18, 2);
            entity.Property(p => p.PerKilometerRate).HasPrecision(18, 2);
            entity.Property(p => p.PerKgRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(l => l.Latitude).HasPrecision(9, 6);
            entity.Property(l => l.Longitude).HasPrecision(9, 6);
        });

        var customerId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var driverUserId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = customerId,
                FirstName = "Jane",
                LastName = "Customer",
                Email = "customer@example.com",
                PhoneNumber = "08012345678",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = providerId,
                FirstName = "Mary",
                LastName = "Operator",
                Email = "provider@example.com",
                PhoneNumber = "08023456789",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.LogisticsProvider,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = driverUserId,
                FirstName = "Daniel",
                LastName = "Driver",
                Email = "driver@example.com",
                PhoneNumber = "08034567890",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Driver,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = adminId,
                FirstName = "Aisha",
                LastName = "Admin",
                Email = "admin@example.com",
                PhoneNumber = "08045678901",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            });

        var truck1Id = Guid.NewGuid();
        var truck2Id = Guid.NewGuid();
        var driverId = Guid.NewGuid();

        modelBuilder.Entity<Driver>().HasData(new Driver
        {
            Id = driverId,
            UserId = driverUserId,
            LicenseNumber = "DL-9001",
            CurrentLocation = "Lekki",
            Status = DriverStatus.Available,
        });

        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle
            {
                Id = truck1Id,
                Name = "Ford Transit",
                Category = "Small Truck",
                RegistrationNumber = "LAG-214-AB",
                CapacityKg = 1200,
                CapacityVolumeM3 = 18,
                DailyRate = 240,
                Status = VehicleStatus.Available,
                DriverId = driverId
            },
            new Vehicle
            {
                Id = truck2Id,
                Name = "Mercedes Sprinter",
                Category = "Large Van",
                RegistrationNumber = "LAG-335-XT",
                CapacityKg = 1800,
                CapacityVolumeM3 = 24,
                DailyRate = 340,
                Status = VehicleStatus.Available,
                DriverId = driverId
            });

        modelBuilder.Entity<PricingRule>().HasData(
            new PricingRule
            {
                Id = Guid.NewGuid(),
                Category = "General",
                BaseRate = 120m,
                PerKilometerRate = 0.75m,
                PerKgRate = 0.85m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-30)
            });

        modelBuilder.Entity<Booking>().HasData(new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DriverId = driverId,
            VehicleId = truck1Id,
            PickupLocation = "Surulere",
            DeliveryLocation = "Ikeja",
            GoodsType = "Furniture",
            ApproximateLoadKg = 320,
            EstimatedCharge = 780m,
            FinalCharge = 780m,
            Status = BookingStatus.Assigned,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        });
    }
}
