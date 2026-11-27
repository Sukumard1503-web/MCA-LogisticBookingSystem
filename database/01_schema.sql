CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(256) NOT NULL UNIQUE,
    "PhoneNumber" VARCHAR(30),
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "Drivers" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "LicenseNumber" VARCHAR(100) NOT NULL,
    "CurrentLocation" VARCHAR(100),
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "Vehicles" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
    "Category" VARCHAR(100) NOT NULL,
    "RegistrationNumber" VARCHAR(50) NOT NULL,
    "CapacityKg" DECIMAL(18,2) NOT NULL,
    "CapacityVolumeM3" DECIMAL(18,2),
    "DailyRate" DECIMAL(18,2) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "DriverId" UUID REFERENCES "Drivers"("Id")
);

CREATE TABLE IF NOT EXISTS "Bookings" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL REFERENCES "Users"("Id"),
    "DriverId" UUID REFERENCES "Drivers"("Id"),
    "VehicleId" UUID REFERENCES "Vehicles"("Id"),
    "PickupLocation" VARCHAR(255) NOT NULL,
    "DeliveryLocation" VARCHAR(255) NOT NULL,
    "GoodsType" VARCHAR(100) NOT NULL,
    "ApproximateLoadKg" DECIMAL(18,2) NOT NULL,
    "EstimatedCharge" DECIMAL(18,2) NOT NULL,
    "FinalCharge" DECIMAL(18,2),
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "PricingRules" (
    "Id" UUID PRIMARY KEY,
    "Category" VARCHAR(100) NOT NULL,
    "BaseRate" DECIMAL(18,2) NOT NULL,
    "PerKilometerRate" DECIMAL(18,2) NOT NULL,
    "PerKgRate" DECIMAL(18,2) NOT NULL,
    "EffectiveFrom" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "EffectiveTo" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS "Payments" (
    "Id" UUID PRIMARY KEY,
    "BookingId" UUID NOT NULL REFERENCES "Bookings"("Id") ON DELETE CASCADE,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Provider" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "PaidAt" TIMESTAMPTZ
);
