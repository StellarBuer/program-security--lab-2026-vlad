using System.Security.Claims;
using System.Text;
using Game.Api.DTOs;
using Game.Api.Middleware;
using Game.Application.Common.Interfaces;
using Game.Application.Identity.Commands;
using Game.Application.Identity.Services;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Repositories;
using Game.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Інфраструктура (База даних, Репозиторії, Безпека)
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// 2. Шар застосунку (Use Cases, Services)
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Налаштування JWT-автентифікації
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured in appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TacticsGameServer",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TacticsGameClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 4. Документація Swagger з підтримкою Bearer-авторизації
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tactics Game Engine API",
        Version = "v1",
        Description = "Серверний REST API для тактичної гри з підтримкою JWT-токенів."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть валідний JWT токен у форматі: Bearer {токен}"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// 5. Глобальний обробник винятків
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Обов'язковий порядок для обробки JWT
app.UseAuthentication();
app.UseAuthorization();

// ======================== ENDPOINTS ========================

// Діагностичний ендпоінт
app.MapGet("/api/health", () => new { Status = "Online", Message = "Tactics Game Server is running!" })
   .WithName("HealthCheck");

// Реєстрація нового користувача
app.MapPost("/api/auth/register", async (RegisterRequest request, IAuthService authService, CancellationToken ct) =>
{
    var command = new RegisterUserAC(request.Email, request.Password);
    var userDto = await authService.RegisterAsync(command, ct);

    return Results.Ok(new
    {
        Message = "Користувач успішно зареєстрований і збережений у базі даних!",
        User = userDto
    });
})
.WithName("RegisterUser");

// Автентифікація користувача з видачею JWT-токена
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService, CancellationToken ct) =>
{
    var command = new LoginUserAC(request.Email, request.Password);
    var authResult = await authService.LoginAsync(command, ct);

    return Results.Ok(new
    {
        Message = "Автентифікація успішна!",
        Token = authResult.Token,
        User = authResult.User
    });
})
.WithName("LoginUser");

// Захищений ендпоінт: профіль поточного користувача за його JWT токеном
app.MapGet("/api/auth/me", async (ClaimsPrincipal claimsUser, IAuthService authService, CancellationToken ct) =>
{
    var userIdString = claimsUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }

    var profile = await authService.GetProfileAsync(userId, ct);
    return Results.Ok(profile);
})
.RequireAuthorization()
.WithName("GetCurrentUserProfile");

app.Run();
