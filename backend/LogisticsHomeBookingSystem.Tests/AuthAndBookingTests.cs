using LogisticsHomeBookingSystem.Api.Data;
using LogisticsHomeBookingSystem.Api.DTOs;
using LogisticsHomeBookingSystem.Api.Models;
using LogisticsHomeBookingSystem.Api.Repositories;
using LogisticsHomeBookingSystem.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LogisticsHomeBookingSystem.Tests;

public class AuthAndBookingTests
{
    [Fact]
    public async Task RegisterAsync_RejectsWeakPassword()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        var configuration = CreateConfiguration();
        var service = new AuthService(context, repository, configuration);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "08033333333",
                Password = "password"
            }));

        Assert.Contains("Password must", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_AuthenticatesKnownCustomer()
    {
        await using var context = CreateContext();
        var repository = new UserRepository(context);
        var configuration = CreateConfiguration();
        var service = new AuthService(context, repository, configuration);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Customer",
            Email = "jane.customer@example.com",
            PhoneNumber = "08012345679",
            Role = UserRole.Customer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "jane.customer@example.com",
            Password = "Password123!"
        });

        Assert.Equal(user.Email, response.Email);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task EstimateAsync_ReturnsPositiveQuote()
    {
        await using var context = CreateContext();
        var bookingRepository = new BookingRepository(context);
        var service = new BookingService(context, bookingRepository);

        var estimate = await service.EstimateAsync(new EstimateRequest
        {
            PickupLocation = "Surulere",
            DeliveryLocation = "Ikeja",
            GoodsType = "Furniture",
            ApproximateLoadKg = 320
        });

        Assert.True(estimate.EstimatedCharge > 0m);
        Assert.Contains("Surulere", estimate.Summary);
        Assert.Contains("Ikeja", estimate.Summary);
    }

    [Fact]
    public async Task CreateBookingAsync_AssignsVehicleWhenProvided()
    {
        await using var context = CreateContext();
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LicenseNumber = "DL-1001",
            CurrentLocation = "Lekki",
            Status = DriverStatus.Available
        };

        var customer = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Customer",
            LastName = "One",
            Email = "customer.one@example.com",
            PhoneNumber = "08099999999",
            Role = UserRole.Customer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Transit Van",
            Category = "Small Truck",
            RegistrationNumber = "LAG-777-ZZ",
            CapacityKg = 1500,
            CapacityVolumeM3 = 22,
            DailyRate = 300,
            Status = VehicleStatus.Available,
            Driver = driver,
            DriverId = driver.Id
        };

        context.Users.Add(customer);
        context.Drivers.Add(driver);
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new BookingService(context, new BookingRepository(context));

        var booking = await service.CreateBookingAsync(customer.Id, new BookingCreateRequest
        {
            PickupLocation = "Yaba",
            DeliveryLocation = "Ojota",
            GoodsType = "Electronics",
            ApproximateLoadKg = 250,
            VehicleId = vehicle.Id
        });

        Assert.Equal(BookingStatus.Assigned.ToString(), booking.Status);
        Assert.Equal(vehicle.Name, booking.VehicleName);
        Assert.Equal(customer.Id, booking.Id != Guid.Empty ? context.Bookings.First(b => b.Id == booking.Id).CustomerId : customer.Id);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "TestsKeyForLogisticsHomeBookingSystem123!",
            ["Jwt:Issuer"] = "LogisticsHomeBookingSystem",
            ["Jwt:Audience"] = "LogisticsHomeBookingSystem.Client"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
