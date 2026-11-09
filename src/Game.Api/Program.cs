using Game.Api.DTOs;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Підключення EF Core із SQLite
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Тестовий ендпоінт
app.MapGet("/api/health", () => new { Status = "Online", Message = "Tactics Game Server is running!" })
   .WithName("HealthCheck");

// Реальний ендпоінт реєстрації з базою даних
app.MapPost("/api/auth/register", async (RegisterRequest request, GameDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { Message = "Email та пароль є обов'язковими." });
    }

    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    // Перевірка, чи існує вже такий користувач
    var existingUser = await dbContext.UserAccounts
        .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

    if (existingUser != null)
    {
        return Results.Conflict(new { Message = "Користувач із такою електронною поштою вже існує." });
    }

    // Просте хешування пароля для демонстрації (у продакшені краще використовувати BCrypt або Argon2)
    var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Password)));

    // Створення доменної сутності
    var user = new UserAccount(normalizedEmail, passwordHash);

    // Збереження в базу даних
    dbContext.UserAccounts.Add(user);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new
    {
        Message = "Користувач успішно зареєстрований і збережений у базі даних!",
        UserId = user.Id,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    });
})
.WithName("RegisterUser");

app.Run();