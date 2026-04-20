using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;
using AvalWebBack.Models.DTOs;
using AvalWebBack.Models; // For ApiResponse<T>

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, data, error) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success)
            return Unauthorized(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("User {Username} logged in successfully", request.Username);
        return Ok(ApiResponse<object>.SuccessResponse(data!));
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse(ModelState));

        var (success, userId, error) = await _authService.RegisterAsync(request);

        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("New user registered with ID: {UserId}", userId);
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "ثبت نام با موفقیت انجام شد",
            userId
        }));
    }
}