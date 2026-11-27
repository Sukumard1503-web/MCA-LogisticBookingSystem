using LogisticsHomeBookingSystem.Api.Data;
using LogisticsHomeBookingSystem.Api.DTOs;
using LogisticsHomeBookingSystem.Api.Models;
using LogisticsHomeBookingSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsHomeBookingSystem.Api.Services;

public interface IBookingService
{
    Task<BookingEstimateResponse> EstimateAsync(EstimateRequest request);
    Task<List<VehicleSummaryDto>> GetAvailableVehiclesAsync();
    Task<List<BookingResponse>> GetBookingsForCustomerAsync(Guid customerId);
    Task<List<BookingResponse>> GetAllBookingsAsync();
    Task<BookingResponse> CreateBookingAsync(Guid customerId, BookingCreateRequest request);
    Task<BookingResponse> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusRequest request);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IBookingRepository _bookingRepository;

    public BookingService(AppDbContext context, IBookingRepository bookingRepository)
    {
        _context = context;
        _bookingRepository = bookingRepository;
    }

    public Task<BookingEstimateResponse> EstimateAsync(EstimateRequest request)
    {
        var baseRate = 120m;
        var perKgRate = 0.85m;
        var perKmRate = 0.75m;
        var estimatedDistanceKm = GetDistanceEstimate(request.PickupLocation, request.DeliveryLocation);
        var estimatedCharge = baseRate + (request.ApproximateLoadKg * perKgRate) + (estimatedDistanceKm * perKmRate);

        var response = new BookingEstimateResponse
        {
            EstimatedCharge = Math.Round(estimatedCharge, 2),
            Summary = $"{request.GoodsType} shipment from {request.PickupLocation} to {request.DeliveryLocation} ({estimatedDistanceKm} km)"
        };

        return Task.FromResult(response);
    }

    public async Task<List<VehicleSummaryDto>> GetAvailableVehiclesAsync()
    {
        var vehicles = await _bookingRepository.GetAvailableVehiclesAsync();
        return vehicles.Select(v => new VehicleSummaryDto
        {
            Id = v.Id,
            Name = v.Name,
            Category = v.Category,
            CapacityKg = v.CapacityKg,
            DailyRate = v.DailyRate,
            Status = v.Status.ToString()
        }).ToList();
    }

    public async Task<List<BookingResponse>> GetBookingsForCustomerAsync(Guid customerId)
    {
        var bookings = await _bookingRepository.GetByCustomerAsync(customerId);
        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<List<BookingResponse>> GetAllBookingsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return bookings.Select(MapBookingToResponse).ToList();
    }

    public async Task<BookingResponse> CreateBookingAsync(Guid customerId, BookingCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PickupLocation) || string.IsNullOrWhiteSpace(request.DeliveryLocation))
        {
            throw new InvalidOperationException("Pickup and delivery locations are required.");
        }

        var estimate = await EstimateAsync(new EstimateRequest
        {
            PickupLocation = request.PickupLocation,
            DeliveryLocation = request.DeliveryLocation,
            GoodsType = request.GoodsType,
            ApproximateLoadKg = request.ApproximateLoadKg
        });

        var customer = await _context.Users.FindAsync(customerId);
        if (customer is null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        var booking = new Booking
        {
            CustomerId = customerId,
            PickupLocation = request.PickupLocation,
            DeliveryLocation = request.DeliveryLocation,
            GoodsType = request.GoodsType,
            ApproximateLoadKg = request.ApproximateLoadKg,
            EstimatedCharge = estimate.EstimatedCharge,
            FinalCharge = estimate.EstimatedCharge,
            Status = BookingStatus.Submitted,
            VehicleId = request.VehicleId,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.VehicleId.HasValue)
        {
            var vehicle = await _context.Vehicles.Include(v => v.Driver)
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value);
            if (vehicle is null || vehicle.Status != VehicleStatus.Available)
            {
                throw new InvalidOperationException("Selected vehicle is no longer available.");
            }
            if (vehicle.CapacityKg < booking.ApproximateLoadKg)
            {
                throw new InvalidOperationException("Selected vehicle does not have enough capacity for this load.");
            }
            if (vehicle.Driver is null || vehicle.Driver.Status != DriverStatus.Available)
            {
                throw new InvalidOperationException("Selected vehicle does not have an available driver.");
            }

            booking.VehicleId = vehicle.Id;
            booking.DriverId = vehicle.DriverId;
            booking.Status = BookingStatus.Assigned;
            vehicle.Status = VehicleStatus.Busy;
            vehicle.Driver.Status = DriverStatus.OnTrip;
        }

        await _bookingRepository.AddAsync(booking);
        await _context.SaveChangesAsync();

        var saved = await _bookingRepository.GetByIdAsync(booking.Id);
        return MapBookingToResponse(saved!);
    }

    public async Task<BookingResponse> UpdateBookingStatusAsync(Guid bookingId, UpdateBookingStatusRequest request)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (!Enum.TryParse<BookingStatus>(request.Status, true, out var newStatus) || !Enum.IsDefined(newStatus))
        {
            throw new InvalidOperationException("Select a valid booking status.");
        }

        var allowedNextStatuses = booking.Status switch
        {
            BookingStatus.Submitted => new[] { BookingStatus.Assigned, BookingStatus.Rejected, BookingStatus.Cancelled },
            BookingStatus.Assigned => new[] { BookingStatus.InTransit, BookingStatus.Cancelled },
            BookingStatus.InTransit => new[] { BookingStatus.Delivered },
            BookingStatus.Delivered => new[] { BookingStatus.Completed },
            _ => Array.Empty<BookingStatus>()
        };
        if (newStatus != booking.Status && !allowedNextStatuses.Contains(newStatus))
        {
            throw new InvalidOperationException($"A booking cannot move from {booking.Status} to {newStatus}.");
        }

        Vehicle? selectedVehicle = null;
        if (request.VehicleId.HasValue)
        {
            selectedVehicle = await _context.Vehicles.Include(v => v.Driver)
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value);
            if (selectedVehicle is null)
            {
                throw new InvalidOperationException("Selected vehicle was not found.");
            }
            if (selectedVehicle.CapacityKg < booking.ApproximateLoadKg)
            {
                throw new InvalidOperationException("Selected vehicle does not have enough capacity for this load.");
            }
            if (selectedVehicle.Status != VehicleStatus.Available && selectedVehicle.Id != booking.VehicleId)
            {
                throw new InvalidOperationException("Selected vehicle is no longer available.");
            }
            if (selectedVehicle.Driver is null || selectedVehicle.Driver.Status != DriverStatus.Available)
            {
                throw new InvalidOperationException("Selected vehicle does not have an available driver.");
            }

            booking.VehicleId = selectedVehicle.Id;
            booking.DriverId = selectedVehicle.DriverId;
        }

        if (newStatus == BookingStatus.Assigned && !booking.VehicleId.HasValue)
        {
            throw new InvalidOperationException("Choose a vehicle before assigning this booking.");
        }

        if (newStatus == BookingStatus.Assigned && booking.VehicleId.HasValue && selectedVehicle is null)
        {
            selectedVehicle = await _context.Vehicles.Include(v => v.Driver)
                .FirstOrDefaultAsync(v => v.Id == booking.VehicleId.Value);
            if (selectedVehicle is null || selectedVehicle.Status != VehicleStatus.Available
                || selectedVehicle.Driver is null || selectedVehicle.Driver.Status != DriverStatus.Available)
            {
                throw new InvalidOperationException("The assigned vehicle or driver is no longer available.");
            }
            booking.DriverId = selectedVehicle.DriverId;
        }

        if (request.DriverId.HasValue)
        {
            throw new InvalidOperationException("Driver assignment is determined by the selected vehicle.");
        }

        booking.Status = newStatus;
        if (newStatus == BookingStatus.Assigned && booking.VehicleId.HasValue)
        {
            var assignedVehicle = selectedVehicle ?? await _context.Vehicles.Include(v => v.Driver)
                .FirstAsync(v => v.Id == booking.VehicleId.Value);
            assignedVehicle.Status = VehicleStatus.Busy;
            if (assignedVehicle.Driver is not null)
            {
                assignedVehicle.Driver.Status = DriverStatus.OnTrip;
            }
        }
        else if (newStatus == BookingStatus.InTransit && booking.DriverId.HasValue)
        {
            if (booking.VehicleId.HasValue)
            {
                var vehicle = await _context.Vehicles.FindAsync(booking.VehicleId.Value);
                if (vehicle is not null)
                {
                    vehicle.Status = VehicleStatus.Busy;
                }
            }
            var driver = await _context.Drivers.FindAsync(booking.DriverId.Value);
            if (driver is not null)
            {
                driver.Status = DriverStatus.OnTrip;
            }
        }
        else if (newStatus is BookingStatus.Delivered or BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.Rejected)
        {
            if (booking.VehicleId.HasValue)
            {
                var vehicle = await _context.Vehicles.FindAsync(booking.VehicleId.Value);
                if (vehicle is not null)
                {
                    vehicle.Status = VehicleStatus.Available;
                }
            }
            if (booking.DriverId.HasValue)
            {
                var driver = await _context.Drivers.FindAsync(booking.DriverId.Value);
                if (driver is not null)
                {
                    driver.Status = DriverStatus.Available;
                }
            }
        }

        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);
        await _context.SaveChangesAsync();
        return MapBookingToResponse(booking);
    }

    private static int GetDistanceEstimate(string origin, string destination)
    {
        var match = 0;
        if (!string.IsNullOrWhiteSpace(origin))
        {
            match += origin.Trim().Length % 12;
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            match += destination.Trim().Length % 14;
        }

        return Math.Max(15, 25 + match);
    }

    private static BookingResponse MapBookingToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            PickupLocation = booking.PickupLocation,
            DeliveryLocation = booking.DeliveryLocation,
            GoodsType = booking.GoodsType,
            ApproximateLoadKg = booking.ApproximateLoadKg,
            EstimatedCharge = booking.EstimatedCharge,
            FinalCharge = booking.FinalCharge ?? booking.EstimatedCharge,
            Status = booking.Status.ToString(),
            CustomerName = booking.Customer is null ? "Customer" : $"{booking.Customer.FirstName} {booking.Customer.LastName}".Trim(),
            DriverName = booking.Driver is null ? null : $"{booking.Driver.User?.FirstName} {booking.Driver.User?.LastName}".Trim(),
            VehicleName = booking.Vehicle?.Name,
            CreatedAt = booking.CreatedAt
        };
    }
}
