using LogisticsHomeBookingSystem.Api.Data;
using LogisticsHomeBookingSystem.Api.DTOs;
using LogisticsHomeBookingSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsHomeBookingSystem.Api.Services;

public interface IAdminService
{
    Task<object> GetDashboardAsync();
    Task<List<VehicleSummaryDto>> GetVehiclesAsync();
    Task<List<DriverSummaryDto>> GetDriversAsync();
    Task<List<PricingRuleDto>> GetPricingAsync();
    Task<VehicleSummaryDto> CreateVehicleAsync(VehicleSummaryDto vehicle);
    Task<DriverSummaryDto> CreateDriverAsync(DriverSummaryDto driver, Guid userId);
    Task<PricingRuleDto> CreatePricingRuleAsync(PricingRuleDto rule);
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetDashboardAsync()
    {
        var bookingCount = await _context.Bookings.CountAsync();
        var activeVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Available);
        var activeDrivers = await _context.Drivers.CountAsync(d => d.Status == DriverStatus.Available);
        var totalRevenue = await _context.Bookings.SumAsync(b => b.FinalCharge ?? b.EstimatedCharge);

        return new
        {
            Summary = new
            {
                TotalBookings = bookingCount,
                AvailableVehicles = activeVehicles,
                AvailableDrivers = activeDrivers,
                Revenue = totalRevenue
            },
            Bookings = await _context.Bookings
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    Customer = b.Customer != null ? $"{b.Customer.FirstName} {b.Customer.LastName}" : "Unknown",
                    b.GoodsType,
                    b.Status,
                    b.EstimatedCharge,
                    b.CreatedAt
                })
                .ToListAsync()
        };
    }

    public async Task<List<VehicleSummaryDto>> GetVehiclesAsync()
    {
        return await _context.Vehicles
            .Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                Name = v.Name,
                Category = v.Category,
                CapacityKg = v.CapacityKg,
                DailyRate = v.DailyRate,
                Status = v.Status.ToString()
            })
            .ToListAsync();
    }

    public async Task<List<DriverSummaryDto>> GetDriversAsync()
    {
        return await _context.Drivers
            .Include(d => d.User)
            .Select(d => new DriverSummaryDto
            {
                Id = d.Id,
                FullName = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Driver",
                LicenseNumber = d.LicenseNumber,
                CurrentLocation = d.CurrentLocation,
                Status = d.Status.ToString()
            })
            .ToListAsync();
    }

    public async Task<List<PricingRuleDto>> GetPricingAsync()
    {
        return await _context.PricingRules
            .Select(p => new PricingRuleDto
            {
                Id = p.Id,
                Category = p.Category,
                BaseRate = p.BaseRate,
                PerKilometerRate = p.PerKilometerRate,
                PerKgRate = p.PerKgRate,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo
            })
            .ToListAsync();
    }

    public async Task<VehicleSummaryDto> CreateVehicleAsync(VehicleSummaryDto vehicle)
    {
        var entity = new Vehicle
        {
            Name = vehicle.Name,
            Category = vehicle.Category,
            CapacityKg = vehicle.CapacityKg,
            DailyRate = vehicle.DailyRate,
            Status = Enum.TryParse<VehicleStatus>(vehicle.Status, true, out var status) ? status : VehicleStatus.Available
        };

        _context.Vehicles.Add(entity);
        await _context.SaveChangesAsync();

        vehicle.Id = entity.Id;
        return vehicle;
    }

    public async Task<DriverSummaryDto> CreateDriverAsync(DriverSummaryDto driver, Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var entity = new Driver
        {
            UserId = userId,
            User = user,
            LicenseNumber = driver.LicenseNumber,
            CurrentLocation = driver.CurrentLocation,
            Status = Enum.TryParse<DriverStatus>(driver.Status, true, out var status) ? status : DriverStatus.Available
        };

        _context.Drivers.Add(entity);
        await _context.SaveChangesAsync();

        driver.Id = entity.Id;
        return driver;
    }

    public async Task<PricingRuleDto> CreatePricingRuleAsync(PricingRuleDto rule)
    {
        var entity = new PricingRule
        {
            Category = rule.Category,
            BaseRate = rule.BaseRate,
            PerKilometerRate = rule.PerKilometerRate,
            PerKgRate = rule.PerKgRate,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo
        };

        _context.PricingRules.Add(entity);
        await _context.SaveChangesAsync();

        rule.Id = entity.Id;
        return rule;
    }
}
