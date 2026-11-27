using LogisticsHomeBookingSystem.Api.DTOs;
using LogisticsHomeBookingSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsHomeBookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,LogisticsProvider")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        return Ok(await _adminService.GetDashboardAsync());
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<List<VehicleSummaryDto>>> GetVehicles()
    {
        return Ok(await _adminService.GetVehiclesAsync());
    }

    [HttpPost("vehicles")]
    public async Task<ActionResult<VehicleSummaryDto>> CreateVehicle([FromBody] VehicleSummaryDto vehicle)
    {
        return Ok(await _adminService.CreateVehicleAsync(vehicle));
    }

    [HttpGet("drivers")]
    public async Task<ActionResult<List<DriverSummaryDto>>> GetDrivers()
    {
        return Ok(await _adminService.GetDriversAsync());
    }

    [HttpPost("drivers")]
    public async Task<ActionResult<DriverSummaryDto>> CreateDriver([FromBody] DriverSummaryDto driver, [FromQuery] Guid userId)
    {
        return Ok(await _adminService.CreateDriverAsync(driver, userId));
    }

    [HttpGet("pricing")]
    public async Task<ActionResult<List<PricingRuleDto>>> GetPricingRules()
    {
        return Ok(await _adminService.GetPricingAsync());
    }

    [HttpPost("pricing")]
    public async Task<ActionResult<PricingRuleDto>> CreatePricingRule([FromBody] PricingRuleDto rule)
    {
        return Ok(await _adminService.CreatePricingRuleAsync(rule));
    }
}
