using AvalWebBack.Models.DTOs;

namespace AvalWebBack.Services;

public interface IAuthService
{
    
    Task<(bool success, object? data, string? error)> LoginAsync(string username, string password);

    
    Task<(bool success, string? userId, string? error)> RegisterAsync(SignupRequest request);
}