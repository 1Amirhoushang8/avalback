using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;
using AvalWebBack.Models;

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTicket([FromQuery] string ticketId)
    {
        var (success, data, error) = await _messageService.GetMessagesByTicketAsync(ticketId);

        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));

        return Ok(ApiResponse<IEnumerable<Message>>.SuccessResponse(data!));
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] Message newMessage)
    {
        var (success, data, error) = await _messageService.SendMessageAsync(newMessage);

        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse(error!));

        _logger.LogInformation("New message sent in ticket {TicketId}", newMessage.TicketId);
        return CreatedAtAction(nameof(GetByTicket), new { ticketId = data!.TicketId },
            ApiResponse<Message>.SuccessResponse(data, "پیام با موفقیت ارسال شد"));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var (success, data, error) = await _messageService.MarkAsReadAsync(id);

        if (!success)
            return NotFound(ApiResponse<object>.ErrorResponse(error!));

        return Ok(ApiResponse<Message>.SuccessResponse(data!));
    }
}