using System.Text;
using LogisticsHomeBookingSystem.Api.Data;
using LogisticsHomeBookingSystem.Api.Middleware;
using LogisticsHomeBookingSystem.Api.Models;
using LogisticsHomeBookingSystem.Api.Repositories;
using LogisticsHomeBookingSystem.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Logistics Home Booking System API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the bearer token.",
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var connectionString = NormalizeDatabaseConnectionString(ResolveDatabaseConnectionString(builder.Configuration));
var usePostgres = IsRealDatabaseConnectionString(connectionString);

if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("LogisticsHomeBookingSystem"));
}

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"] ?? "LogisticsHomeBookingSystem.Dev.Secret.Key";
var issuer = jwtSettings["Issuer"] ?? "LogisticsHomeBookingSystem";
var audience = jwtSettings["Audience"] ?? "LogisticsHomeBookingSystem.Client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("LogisticsProvider", policy => policy.RequireRole("LogisticsProvider"));
    options.AddPolicy("Driver", policy => policy.RequireRole("Driver"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await EnsureDatabaseReadyAsync(db, usePostgres);
}

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    databaseProvider = usePostgres ? "PostgreSQL" : "InMemory",
    timestampUtc = DateTime.UtcNow
}));

app.MapControllers();

app.Run();

static string ResolveDatabaseConnectionString(ConfigurationManager configuration)
{
    var candidateSources = new[]
    {
        Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING"),
        Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"),
        Environment.GetEnvironmentVariable("DATABASE_URL"),
        configuration.GetConnectionString("DefaultConnection"),
        configuration["ConnectionStrings:DefaultConnection"]
    };

    foreach (var candidate in candidateSources)
    {
        if (IsRealDatabaseConnectionString(candidate))
        {
            return candidate!;
        }
    }

    return candidateSources.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate)) ?? string.Empty;
}

static string NormalizeDatabaseConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return string.Empty;
    }

    var trimmed = connectionString.Trim();
    if (trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(trimmed);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var databaseName = uri.AbsolutePath.Trim('/');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = databaseName,
                Username = username,
                Password = password,
                SslMode = SslMode.Require
            };

            return builder.ConnectionString;
        }
        catch
        {
            return trimmed;
        }
    }

    return trimmed;
}

static bool IsRealDatabaseConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    var trimmed = connectionString.Trim();
    var placeholderFragments = new[]
    {
        "replace-with",
        "your_supabase",
        "your-postgres",
        "changeme",
        "example.com",
        "postgresql://username:password@host:5432/database",
        "postgres://username:password@host:5432/database"
    };

    var hasConnectionShape = trimmed.Contains("Host=", StringComparison.OrdinalIgnoreCase)
        || trimmed.Contains("host=", StringComparison.OrdinalIgnoreCase)
        || trimmed.Contains("postgresql://", StringComparison.OrdinalIgnoreCase)
        || trimmed.Contains("postgres://", StringComparison.OrdinalIgnoreCase);

    if (!hasConnectionShape)
    {
        return false;
    }

    return placeholderFragments.All(fragment => !trimmed.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}

static async Task EnsureDatabaseReadyAsync(AppDbContext db, bool usePostgres)
{
    if (!usePostgres)
    {
        db.Database.EnsureCreated();
        return;
    }

    try
    {
        var hasUsersTable = await TableExistsAsync(db, "Users");
        if (!hasUsersTable)
        {
            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var schemaPath = Path.Combine(repoRoot, "database", "01_schema.sql");
            if (File.Exists(schemaPath))
            {
                var script = File.ReadAllText(schemaPath);
                foreach (var statement in SplitSqlStatements(script))
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        await db.Database.ExecuteSqlRawAsync(statement);
                    }
                }
            }
            else
            {
                db.Database.EnsureCreated();
            }
        }

        if (!await db.Users.AnyAsync())
        {
            var customerId = Guid.NewGuid();
            var providerId = Guid.NewGuid();
            var driverUserId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var driverId = Guid.NewGuid();
            var truck1Id = Guid.NewGuid();
            var truck2Id = Guid.NewGuid();
            var pricingRuleId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var users = new[]
            {
                new User { Id = customerId, FirstName = "Jane", LastName = "Customer", Email = "customer@example.com", PhoneNumber = "08012345678", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), Role = UserRole.Customer, CreatedAt = DateTime.UtcNow },
                new User { Id = providerId, FirstName = "Mary", LastName = "Operator", Email = "provider@example.com", PhoneNumber = "08023456789", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), Role = UserRole.LogisticsProvider, CreatedAt = DateTime.UtcNow },
                new User { Id = driverUserId, FirstName = "Daniel", LastName = "Driver", Email = "driver@example.com", PhoneNumber = "08034567890", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), Role = UserRole.Driver, CreatedAt = DateTime.UtcNow },
                new User { Id = adminId, FirstName = "Aisha", LastName = "Admin", Email = "admin@example.com", PhoneNumber = "08045678901", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"), Role = UserRole.Admin, CreatedAt = DateTime.UtcNow }
            };

            db.Users.AddRange(users);
            db.Drivers.Add(new Driver { Id = driverId, UserId = driverUserId, LicenseNumber = "DL-9001", CurrentLocation = "Lekki", Status = DriverStatus.Available });
            db.Vehicles.AddRange(
                new Vehicle { Id = truck1Id, Name = "Ford Transit", Category = "Small Truck", RegistrationNumber = "LAG-214-AB", CapacityKg = 1200, CapacityVolumeM3 = 18, DailyRate = 240, Status = VehicleStatus.Available, DriverId = driverId },
                new Vehicle { Id = truck2Id, Name = "Mercedes Sprinter", Category = "Large Van", RegistrationNumber = "LAG-335-XT", CapacityKg = 1800, CapacityVolumeM3 = 24, DailyRate = 340, Status = VehicleStatus.Available, DriverId = driverId });
            db.PricingRules.Add(new PricingRule { Id = pricingRuleId, Category = "General", BaseRate = 120m, PerKilometerRate = 0.75m, PerKgRate = 0.85m, EffectiveFrom = DateTime.UtcNow.AddDays(-30) });
            db.Bookings.Add(new Booking { Id = bookingId, CustomerId = customerId, DriverId = driverId, VehicleId = truck1Id, PickupLocation = "Surulere", DeliveryLocation = "Ikeja", GoodsType = "Furniture", ApproximateLoadKg = 320, EstimatedCharge = 780m, FinalCharge = 780m, Status = BookingStatus.Assigned, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow.AddDays(-2) });
            await db.SaveChangesAsync();
        }
    }
    catch (Exception)
    {
        db.Database.EnsureCreated();
    }
}

static async Task<bool> TableExistsAsync(AppDbContext db, string tableName)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
    {
        await connection.OpenAsync();
    }

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @tableName)";
    var parameter = command.CreateParameter();
    parameter.ParameterName = "@tableName";
    parameter.Value = tableName;
    command.Parameters.Add(parameter);

    var result = await command.ExecuteScalarAsync();
    return result is bool boolResult && boolResult;
}

static IEnumerable<string> SplitSqlStatements(string script)
{
    var statements = new List<string>();
    var current = new StringBuilder();
    var inSingleQuote = false;
    var inDoubleQuote = false;

    foreach (var ch in script)
    {
        if (ch == '\'' && !inDoubleQuote)
        {
            inSingleQuote = !inSingleQuote;
            current.Append(ch);
            continue;
        }

        if (ch == '"' && !inSingleQuote)
        {
            inDoubleQuote = !inDoubleQuote;
            current.Append(ch);
            continue;
        }

        if (ch == ';' && !inSingleQuote && !inDoubleQuote)
        {
            var statement = current.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(statement))
            {
                statements.Add(statement);
            }
            current.Clear();
            continue;
        }

        current.Append(ch);
    }

    var trailing = current.ToString().Trim();
    if (!string.IsNullOrWhiteSpace(trailing))
    {
        statements.Add(trailing);
    }

    return statements;
}
