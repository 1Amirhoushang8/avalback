using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;
using AvalWebBack.Models;

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var (success, data, error) = await _userService.GetAllUsersAsync();
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));
        return Ok(ApiResponse<IEnumerable<User>>.SuccessResponse(data!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (success, data, error) = await _userService.GetUserByIdAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));
        return Ok(ApiResponse<User>.SuccessResponse(data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User newUser)
    {
        var (success, data, error) = await _userService.CreateUserAsync(newUser);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("User {UserId} created", data!.Id);
        return CreatedAtAction(nameof(GetById), new { id = data.Id },
            ApiResponse<User>.SuccessResponse(data, "کاربر با موفقیت ایجاد شد"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] User updatedUser)
    {
        var (success, data, error) = await _userService.UpdateUserAsync(id, updatedUser);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("User {UserId} updated", id);
        return Ok(ApiResponse<User>.SuccessResponse(data!, "کاربر با موفقیت به‌روزرسانی شد"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _userService.DeleteUserAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("User {UserId} deleted", id);
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "کاربر با موفقیت حذف شد"));
    }
}