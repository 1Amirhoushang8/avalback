using AvalWebBack.Models;

namespace AvalWebBack.Services;

public interface IUserService
{
    Task<(bool success, IEnumerable<User>? data, string? error)> GetAllUsersAsync();
    Task<(bool success, User? data, string? error)> GetUserByIdAsync(string id);
    Task<(bool success, User? data, string? error)> CreateUserAsync(User newUser);
    Task<(bool success, User? data, string? error)> UpdateUserAsync(string id, User updatedUser);
    Task<(bool success, string? error)> DeleteUserAsync(string id);
}