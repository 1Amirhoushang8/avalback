using Microsoft.AspNetCore.Mvc;
using AvalBackend.Services;
using AvalBackend.Models;
using System.Linq;

namespace AvalBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JsonDataService _dataService;

    public AuthController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var db = await _dataService.ReadAsync();

        var admin = db.Admins.FirstOrDefault(a =>
            a.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) &&
            a.Password == request.Password);

        if (admin != null)
        {
            var adminData = new
            {
                id = admin.Id,
                username = admin.Username,
                fullName = admin.FullName,
                roleKey = "ADMIN"
            };

            return Ok(new
            {
                user = adminData,
                token = GenerateFakeToken(admin.Id, "ADMIN")
            });
        }

        var user = db.Users.FirstOrDefault(u =>
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == request.Password);

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

            return Ok(new
            {
                user = userData,
                token = GenerateFakeToken(user.Id, userData.roleKey)
            });
        }

        return Unauthorized(new { message = "نام کاربری یا رمز عبور اشتباه است" });
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        var db = await _dataService.ReadAsync();

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length < 4)
            return BadRequest(new { message = "نام و نام خانوادگی باید حداقل ۴ کاراکتر و به زبان فارسی باشد" });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.FullName, @"^[\u0600-\u06FF\s]+$"))
            return BadRequest(new { message = "نام و نام خانوادگی باید فقط شامل حروف فارسی باشد" });

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.Username, @"^[A-Za-z0-9_]+$"))
            return BadRequest(new { message = "نام کاربری باید فقط شامل حروف انگلیسی، اعداد و زیرخط باشد" });

        if (request.Password.Length < 6)
            return BadRequest(new { message = "رمز عبور باید حداقل ۶ کاراکتر باشد" });

        if (db.Users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            return BadRequest(new { message = "این نام کاربری قبلاً انتخاب شده است" });

        if (db.Users.Any(u => u.Password == request.Password))
            return BadRequest(new { message = "این رمز عبور قبلاً استفاده شده است" });

        var newUser = new User
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8),
            Username = request.Username,
            Password = request.Password,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            SerialNumber = new Random().Next(10000000, 99999999).ToString(),
            Service = null,                // ✅ Prevent auto appearance in accounting list
            Price = null,                  // ✅ No default price
            Status = "درحال-انجام",
            PaymentType = "پرداخت-تکی",
            RoleKey = "USER",
            MonthlyPayment = null,
            TotalMonths = null
        };

        db.Users.Add(newUser);
        await _dataService.WriteAsync(db);

        return Ok(new { message = "ثبت نام با موفقیت انجام شد", userId = newUser.Id });
    }

    private string GenerateFakeToken(string userId, string role)
    {
        return $"fake-{role.ToLower()}-token-{userId}";
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SignupRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}