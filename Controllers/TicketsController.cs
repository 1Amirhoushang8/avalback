using Microsoft.AspNetCore.Mvc;
using AvalBackend.Services;
using AvalBackend.Models;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AvalBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly JsonDataService _dataService;

    public TicketsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    
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
                results.Add($"First 200 chars: {json.Substring(0, Math.Min(200, json.Length))}");
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
                filePath = filePath,
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

    // GET: api/tickets
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? userId)
    {
        var db = await _dataService.ReadAsync();
        var tickets = db.Tickets.AsEnumerable();

        if (!string.IsNullOrEmpty(userId))
            tickets = tickets.Where(t => t.UserId == userId);

        return Ok(tickets);
    }

    // GET: api/tickets/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return NotFound(new { message = "تیکت یافت نشد" });

        return Ok(ticket);
    }

    // POST: api/tickets
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Ticket newTicket)
    {
        var db = await _dataService.ReadAsync();

        if (!db.Users.Any(u => u.Id == newTicket.UserId))
            return BadRequest(new { message = "کاربر نامعتبر است" });

        newTicket.Id = Guid.NewGuid().ToString().Substring(0, 8);
        newTicket.Status = "pending";
        

        db.Tickets.Add(newTicket);
        await _dataService.WriteAsync(db);

        return CreatedAtAction(nameof(GetById), new { id = newTicket.Id }, newTicket);
    }

    // PUT: api/tickets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Ticket updatedTicket)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return NotFound(new { message = "تیکت یافت نشد" });

        ticket.Title = updatedTicket.Title;
        ticket.ShortDetail = updatedTicket.ShortDetail;
        ticket.Description = updatedTicket.Description;
        ticket.Status = updatedTicket.Status;
        ticket.AdminResponse = updatedTicket.AdminResponse;
        

        await _dataService.WriteAsync(db);
        return Ok(ticket);
    }

    // DELETE: api/tickets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return NotFound(new { message = "تیکت یافت نشد" });

        db.Tickets.Remove(ticket);
        await _dataService.WriteAsync(db);

        return NoContent();
    }

    // GET: api/tickets/{id}/file – Download attached file
    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket?.File == null)
            return NotFound(new { message = "فایلی برای این تیکت موجود نیست" });

        try
        {
            
            var fileElement = (JsonElement)ticket.File;
            if (fileElement.ValueKind == JsonValueKind.Object &&
                fileElement.TryGetProperty("data", out var dataElement) &&
                fileElement.TryGetProperty("name", out var nameElement) &&
                fileElement.TryGetProperty("type", out var typeElement))
            {
                var base64WithPrefix = dataElement.GetString();
                var fileName = nameElement.GetString();
                var contentType = typeElement.GetString();

                if (string.IsNullOrEmpty(base64WithPrefix))
                    return BadRequest(new { message = "داده فایل خالی است" });

                
                var base64Data = base64WithPrefix.Contains(',')
                    ? base64WithPrefix.Substring(base64WithPrefix.IndexOf(',') + 1)
                    : base64WithPrefix;

                var fileBytes = Convert.FromBase64String(base64Data);
                return File(fileBytes, contentType ?? "application/octet-stream", fileName ?? "download");
            }

            return BadRequest(new { message = "ساختار فایل نامعتبر است" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "خطا در پردازش فایل", error = ex.Message });
        }
    }
}