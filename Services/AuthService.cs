using System.Linq;
using AvalWebBack.Models;
using AvalWebBack.Models.DTOs;

namespace AvalWebBack.Services;

public class AuthService : IAuthService
{
    private readonly JsonDataService _dataService;

    public AuthService(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<(bool success, object? data, string? error)> LoginAsync(string username, string password)
    {
        var db = await _dataService.ReadAsync();

        // Check admins
        var admin = db.Admins.FirstOrDefault(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            a.Password == password);

        if (admin != null)
        {
            var userData = new
            {
                id = admin.Id,
                username = admin.Username,
                fullName = admin.FullName,
                roleKey = "ADMIN"
            };
            var token = GenerateFakeToken(admin.Id, "ADMIN");
            return (true, new { user = userData, token }, null);
        }

        // Check regular users
        var user = db.Users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user != null)
        {
            var userData = new
            {
                id = user.Id,
                username = user.Username,
                fullName = user.FullName,
                roleKey = user.RoleKey ?? "USER",
                phoneNumber = user.PhoneNumber,
                serialNumber = user.SerialNumber
            };
            var token = GenerateFakeToken(user.Id, userData.roleKey);
            return (true, new { user = userData, token }, null);
        }

        return (false, null!, "نام کاربری یا رمز عبور اشتباه است");
    }

    public async Task<(bool success, string? userId, string? error)> RegisterAsync(SignupRequest request)
    {
        var db = await _dataService.ReadAsync();

        if (db.Users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            return (false, null, "این نام کاربری قبلاً انتخاب شده است");

        if (db.Users.Any(u => u.Password == request.Password))
            return (false, null, "این رمز عبور قبلاً استفاده شده است");

        var newUser = new User
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8),
            Username = request.Username,
            Password = request.Password,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            SerialNumber = new Random().Next(10000000, 99999999).ToString(),
            Service = null,
            Price = null,
            Status = "درحال-انجام",
            PaymentType = "پرداخت-تکی",
            RoleKey = "USER",
            MonthlyPayment = null,
            TotalMonths = null
        };

        db.Users.Add(newUser);
        await _dataService.WriteAsync(db);
        return (true, newUser.Id, null);
    }

    private string GenerateFakeToken(string userId, string role)
    {
        return $"fake-{role.ToLower()}-token-{userId}";
    }
}