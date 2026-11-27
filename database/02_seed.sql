INSERT INTO users (id, first_name, last_name, email, phone_number, password_hash, role, created_at)
VALUES
('11111111-1111-1111-1111-111111111111', 'Jane', 'Customer', 'customer@example.com', '08012345678', 'hashed-password', 'Customer', NOW()),
('22222222-2222-2222-2222-222222222222', 'Mary', 'Operator', 'provider@example.com', '08023456789', 'hashed-password', 'LogisticsProvider', NOW()),
('33333333-3333-3333-3333-333333333333', 'Daniel', 'Driver', 'driver@example.com', '08034567890', 'hashed-password', 'Driver', NOW()),
('44444444-4444-4444-4444-444444444444', 'Aisha', 'Admin', 'admin@example.com', '08045678901', 'hashed-password', 'Admin', NOW());

INSERT INTO drivers (id, user_id, license_number, current_location, status, created_at)
VALUES ('55555555-5555-5555-5555-555555555555', '33333333-3333-3333-3333-333333333333', 'DL-9001', 'Lekki', 'Available', NOW());

INSERT INTO vehicles (id, name, category, registration_number, capacity_kg, capacity_volume_m3, daily_rate, status, driver_id)
VALUES
('66666666-6666-6666-6666-666666666666', 'Ford Transit', 'Small Truck', 'LAG-214-AB', 1200, 18, 240, 'Available', '55555555-5555-5555-5555-555555555555'),
('77777777-7777-7777-7777-777777777777', 'Mercedes Sprinter', 'Large Van', 'LAG-335-XT', 1800, 24, 340, 'Available', '55555555-5555-5555-5555-555555555555');

INSERT INTO pricing_rules (id, category, base_rate, per_kilometer_rate, per_kg_rate, effective_from)
VALUES ('88888888-8888-8888-8888-888888888888', 'General', 120, 0.75, 0.85, NOW());

INSERT INTO bookings (id, customer_id, driver_id, vehicle_id, pickup_location, delivery_location, goods_type, approximate_load_kg, estimated_charge, final_charge, status, created_at, updated_at)
VALUES ('99999999-9999-9999-9999-999999999999', '11111111-1111-1111-1111-111111111111', '55555555-5555-5555-5555-555555555555', '66666666-6666-6666-6666-666666666666', 'Surulere', 'Ikeja', 'Furniture', 320, 780, 780, 'Assigned', NOW(), NOW());
