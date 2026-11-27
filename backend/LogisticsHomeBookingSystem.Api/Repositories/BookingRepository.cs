using LogisticsHomeBookingSystem.Api.Data;
using LogisticsHomeBookingSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisticsHomeBookingSystem.Api.Repositories;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();
    Task<List<Booking>> GetByCustomerAsync(Guid customerId);
    Task<Booking?> GetByIdAsync(Guid bookingId);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task<List<Vehicle>> GetAvailableVehiclesAsync();
    Task<List<Driver>> GetDriversAsync();
    Task<List<PricingRule>> GetPricingRulesAsync();
}

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Driver)
                .ThenInclude(d => d!.User)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByCustomerAsync(Guid customerId)
    {
        return await _context.Bookings
            .Where(b => b.CustomerId == customerId)
            .Include(b => b.Customer)
            .Include(b => b.Driver)
                .ThenInclude(d => d!.User)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Driver)
                .ThenInclude(d => d!.User)
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
    }

    public Task<List<Vehicle>> GetAvailableVehiclesAsync()
    {
        return _context.Vehicles
            .Where(v => v.Status == VehicleStatus.Available)
            .ToListAsync();
    }

    public Task<List<Driver>> GetDriversAsync()
    {
        return _context.Drivers
            .Include(d => d.User)
            .ToListAsync();
    }

    public Task<List<PricingRule>> GetPricingRulesAsync()
    {
        return _context.PricingRules
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync();
    }
}
