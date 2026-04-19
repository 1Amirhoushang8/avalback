using Microsoft.AspNetCore.Mvc;
using AvalBackend.Services;
using AvalBackend.Models;
using System.Linq;

namespace AvalBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly JsonDataService _dataService;

    public MessagesController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTicket([FromQuery] string ticketId)
    {
        if (string.IsNullOrEmpty(ticketId))
            return BadRequest(new { message = "شناسه تیکت الزامی است" });

        var db = await _dataService.ReadAsync();
        var messages = db.Messages
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.Timestamp)
            .ToList();

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] Message newMessage)
    {
        var db = await _dataService.ReadAsync();

        var ticketExists = db.Tickets.Any(t => t.Id == newMessage.TicketId);
        if (!ticketExists)
            return BadRequest(new { message = "تیکت مورد نظر یافت نشد" });

        var senderExists = db.Users.Any(u => u.Id == newMessage.SenderId) ||
                          db.Admins.Any(a => a.Id == newMessage.SenderId);
        if (!senderExists)
            return BadRequest(new { message = "فرستنده نامعتبر است" });

        newMessage.Id = Guid.NewGuid().ToString().Substring(0, 8);
        newMessage.Timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        newMessage.IsRead = false;

        db.Messages.Add(newMessage);
        await _dataService.WriteAsync(db);

        return CreatedAtAction(nameof(GetByTicket), new { ticketId = newMessage.TicketId }, newMessage);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var db = await _dataService.ReadAsync();
        var message = db.Messages.FirstOrDefault(m => m.Id == id);

        if (message == null)
            return NotFound(new { message = "پیام یافت نشد" });

        message.IsRead = true;
        await _dataService.WriteAsync(db);

        return Ok(message);
    }
}