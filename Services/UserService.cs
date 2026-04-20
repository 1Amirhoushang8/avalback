using AvalWebBack.Models;

namespace AvalWebBack.Services;

public class UserService : IUserService
{
    private readonly JsonDataService _dataService;

    public UserService(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<(bool success, IEnumerable<User>? data, string? error)> GetAllUsersAsync()
    {
        var db = await _dataService.ReadAsync();
        return (true, db.Users, null);
    }

    public async Task<(bool success, User? data, string? error)> GetUserByIdAsync(string id)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return (false, null, "کاربر یافت نشد");
        return (true, user, null);
    }

    public async Task<(bool success, User? data, string? error)> CreateUserAsync(User newUser)
    {
        var db = await _dataService.ReadAsync();

        if (string.IsNullOrEmpty(newUser.Id))
            newUser.Id = Guid.NewGuid().ToString()[..8];

        if (db.Users.Any(u => u.SerialNumber == newUser.SerialNumber))
            return (false, null, "شماره فاکتور تکراری است");

        db.Users.Add(newUser);
        await _dataService.WriteAsync(db);

        return (true, newUser, null);
    }

    public async Task<(bool success, User? data, string? error)> UpdateUserAsync(string id, User updatedUser)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return (false, null, "کاربر یافت نشد");

        if (!string.IsNullOrEmpty(updatedUser.SerialNumber) &&
            db.Users.Any(u => u.SerialNumber == updatedUser.SerialNumber && u.Id != id))
        {
            return (false, null, "شماره فاکتور تکراری است");
        }

        user.FullName = updatedUser.FullName ?? user.FullName;
        user.PhoneNumber = updatedUser.PhoneNumber ?? user.PhoneNumber;
        user.SerialNumber = updatedUser.SerialNumber ?? user.SerialNumber;
        user.Service = updatedUser.Service ?? user.Service;
        user.Price = updatedUser.Price ?? user.Price;
        user.Status = updatedUser.Status ?? user.Status;
        user.PaymentType = updatedUser.PaymentType ?? user.PaymentType;
        user.MonthlyPayment = updatedUser.MonthlyPayment ?? user.MonthlyPayment;
        user.TotalMonths = updatedUser.TotalMonths ?? user.TotalMonths;

        await _dataService.WriteAsync(db);
        return (true, user, null);
    }

    public async Task<(bool success, string? error)> DeleteUserAsync(string id)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return (false, "کاربر یافت نشد");

        db.Users.Remove(user);
        await _dataService.WriteAsync(db);

        return (true, null);
    }
}