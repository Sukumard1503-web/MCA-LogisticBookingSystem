using LogisticsHomeBookingSystem.Api.Models;

namespace LogisticsHomeBookingSystem.Api.DTOs;

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = nameof(UserRole.Customer);
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class BookingCreateRequest
{
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string GoodsType { get; set; } = string.Empty;
    public decimal ApproximateLoadKg { get; set; }
    public Guid? VehicleId { get; set; }
}

public class EstimateRequest
{
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string GoodsType { get; set; } = string.Empty;
    public decimal ApproximateLoadKg { get; set; }
}

public class BookingEstimateResponse
{
    public decimal EstimatedCharge { get; set; }
    public string Currency { get; set; } = "USD";
    public string Summary { get; set; } = string.Empty;
}

public class BookingResponse
{
    public Guid Id { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string GoodsType { get; set; } = string.Empty;
    public decimal ApproximateLoadKg { get; set; }
    public decimal EstimatedCharge { get; set; }
    public decimal? FinalCharge { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public string? VehicleName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
}

public class VehicleSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CapacityKg { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DriverSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PricingRuleDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public decimal PerKilometerRate { get; set; }
    public decimal PerKgRate { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
