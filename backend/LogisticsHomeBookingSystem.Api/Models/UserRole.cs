namespace LogisticsHomeBookingSystem.Api.Models;

public enum UserRole
{
    Customer,
    LogisticsProvider,
    Driver,
    Admin
}

public enum BookingStatus
{
    Submitted,
    Assigned,
    InTransit,
    Delivered,
    Completed,
    Cancelled,
    Rejected
}

public enum VehicleStatus
{
    Available,
    Busy,
    Maintenance
}

public enum DriverStatus
{
    Available,
    OnTrip,
    OffDuty
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed
}
