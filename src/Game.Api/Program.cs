using System.Text;
using Game.Api.Middleware;
using Game.Application.Common.Interfaces;
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

// Доступ до HttpContext для CurrentUserService (RBAC/OBAC)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 2. Шар застосунку (Use Cases, Services)
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Контролери (MVC Web API Architecture)
builder.Services.AddControllers();

// 4. Налаштування JWT-автентифікації
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

builder.Services.AddAuthorization(options =>
{
    // Політика доступу лише для адміністраторів (RBAC)
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// 5. Документація Swagger з підтримкою Bearer-авторизації
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tactics Game Engine API",
        Version = "v1",
        Description = "Серверний REST API (MVC Controllers) для тактичної гри з підтримкою JWT-токенів та RBAC/OBAC."
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

// 6. Глобальний обробник винятків
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Обов'язковий порядок обробки JWT
app.UseAuthentication();
app.UseAuthorization();

// 7. Маршрутизація до MVC Контролерів
app.MapControllers();

app.Run();
