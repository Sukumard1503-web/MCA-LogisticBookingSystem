namespace LogisticsHomeBookingSystem.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public decimal CapacityKg { get; set; }
    public decimal CapacityVolumeM3 { get; set; }
    public decimal DailyRate { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }
    public ICollection<VehicleAvailability> Availabilities { get; set; } = new List<VehicleAvailability>();
}

public class Driver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Available;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public User? Customer { get; set; }
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string GoodsType { get; set; } = string.Empty;
    public decimal ApproximateLoadKg { get; set; }
    public decimal EstimatedCharge { get; set; }
    public decimal? FinalCharge { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Submitted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Payment? Payment { get; set; }
}

public class VehicleAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public DateTime Date { get; set; }
    public bool IsAvailable { get; set; } = true;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class PricingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public decimal PerKilometerRate { get; set; }
    public decimal PerKgRate { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
}

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }
    public decimal Amount { get; set; }
    public string Provider { get; set; } = "Cash";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }
}
