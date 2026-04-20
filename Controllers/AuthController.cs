using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;
using AvalWebBack.Models.DTOs;

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, data, error) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(data);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        // Model validation is automatic because of [ApiController]
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, userId, error) = await _authService.RegisterAsync(request);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "ثبت نام با موفقیت انجام شد", userId });
    }
}