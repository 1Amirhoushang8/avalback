using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;
using AvalWebBack.Models;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    // ========== DEBUG ENDPOINTS (unchanged) ==========
    [HttpGet("debug")]
    public IActionResult Debug()
    {
        var results = new List<string>();
        var currentDir = Directory.GetCurrentDirectory();
        results.Add($"Current Directory: {currentDir}");

        var dataFolder1 = Path.Combine(currentDir, "Data");
        results.Add($"Data folder (relative): {dataFolder1}");
        results.Add($"Data folder exists: {Directory.Exists(dataFolder1)}");

        var filePath1 = Path.Combine(dataFolder1, "db.json");
        results.Add($"db.json path: {filePath1}");
        results.Add($"db.json exists: {System.IO.File.Exists(filePath1)}");

        if (System.IO.File.Exists(filePath1))
        {
            var fileSize = new FileInfo(filePath1).Length;
            results.Add($"File size: {fileSize} bytes");

            try
            {
                var json = System.IO.File.ReadAllText(filePath1);
                results.Add($"First 200 chars: {json[..Math.Min(200, json.Length)]}");
            }
            catch (Exception ex)
            {
                results.Add($"Error reading file: {ex.Message}");
            }
        }

        var projectRoot = Path.Combine(currentDir, "..", "..", "..");
        results.Add($"Project root (guessed): {Path.GetFullPath(projectRoot)}");

        var dataFolder2 = Path.Combine(Path.GetFullPath(projectRoot), "Data");
        results.Add($"Project Data folder: {dataFolder2}");
        results.Add($"Project Data exists: {Directory.Exists(dataFolder2)}");

        var filePath2 = Path.Combine(dataFolder2, "db.json");
        results.Add($"Project db.json exists: {System.IO.File.Exists(filePath2)}");

        return Ok(results);
    }

    [HttpGet("debug-deserialize")]
    public IActionResult DebugDeserialize()
    {
        var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        var filePath = Path.Combine(dataFolder, "db.json");

        if (!System.IO.File.Exists(filePath))
        {
            dataFolder = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..")), "Data");
            filePath = Path.Combine(dataFolder, "db.json");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return Ok(new { error = "File not found anywhere", currentDir = Directory.GetCurrentDirectory() });
        }

        try
        {
            var json = System.IO.File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var db = JsonSerializer.Deserialize<Database>(json, options);

            return Ok(new
            {
                filePath,
                adminsCount = db?.Admins?.Count ?? 0,
                usersCount = db?.Users?.Count ?? 0,
                ticketsCount = db?.Tickets?.Count ?? 0,
                messagesCount = db?.Messages?.Count ?? 0,
                firstTicketTitle = db?.Tickets?.FirstOrDefault()?.Title ?? "none"
            });
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    // ========== BUSINESS ENDPOINTS (with ApiResponse<T> wrapper) ==========
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? userId)
    {
        var (success, data, error) = await _ticketService.GetAllTicketsAsync(userId);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));
        return Ok(ApiResponse<IEnumerable<Ticket>>.SuccessResponse(data!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (success, data, error) = await _ticketService.GetTicketByIdAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));
        return Ok(ApiResponse<Ticket>.SuccessResponse(data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ticket newTicket)
    {
        var (success, data, error) = await _ticketService.CreateTicketAsync(newTicket);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("Ticket {TicketId} created by user {UserId}", data!.Id, newTicket.UserId);
        return CreatedAtAction(nameof(GetById), new { id = data.Id },
            ApiResponse<Ticket>.SuccessResponse(data, "تیکت با موفقیت ایجاد شد"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Ticket updatedTicket)
    {
        var (success, data, error) = await _ticketService.UpdateTicketAsync(id, updatedTicket);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("Ticket {TicketId} updated", id);
        return Ok(ApiResponse<Ticket>.SuccessResponse(data!, "تیکت با موفقیت به‌روزرسانی شد"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _ticketService.DeleteTicketAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("Ticket {TicketId} deleted", id);
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "تیکت با موفقیت حذف شد"));
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        var (success, fileBytes, contentType, fileName, error) = await _ticketService.GetTicketFileAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        
        return File(fileBytes!, contentType ?? "application/octet-stream", fileName ?? "download");
    }
}