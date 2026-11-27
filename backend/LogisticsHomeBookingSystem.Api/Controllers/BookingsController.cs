using System.Security.Claims;
using LogisticsHomeBookingSystem.Api.DTOs;
using LogisticsHomeBookingSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsHomeBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("estimate")]
    public async Task<ActionResult<BookingEstimateResponse>> Estimate([FromBody] EstimateRequest request)
    {
        var estimate = await _bookingService.EstimateAsync(request);
        return Ok(estimate);
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<List<VehicleSummaryDto>>> GetAvailableVehicles()
    {
        var vehicles = await _bookingService.GetAvailableVehiclesAsync();
        return Ok(vehicles);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<List<BookingResponse>>> GetMyBookings()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var bookings = await _bookingService.GetBookingsForCustomerAsync(userId);
        return Ok(bookings);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] BookingCreateRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var booking = await _bookingService.CreateBookingAsync(userId, request);
            return Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,LogisticsProvider")]
    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [Authorize(Roles = "Admin,LogisticsProvider")]
    [HttpPatch("{bookingId:guid}/status")]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request)
    {
        try
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(bookingId, request);
            return Ok(booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
