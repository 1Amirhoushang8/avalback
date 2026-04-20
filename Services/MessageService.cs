using AvalWebBack.Models;

namespace AvalWebBack.Services;

public class MessageService : IMessageService
{
    private readonly JsonDataService _dataService;

    public MessageService(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<(bool success, IEnumerable<Message>? data, string? error)> GetMessagesByTicketAsync(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
            return (false, null, "شناسه تیکت الزامی است");

        var db = await _dataService.ReadAsync();
        var messages = db.Messages
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.Timestamp)
            .ToList();

        return (true, messages, null);
    }

    public async Task<(bool success, Message? data, string? error)> SendMessageAsync(Message newMessage)
    {
        var db = await _dataService.ReadAsync();

        var ticketExists = db.Tickets.Any(t => t.Id == newMessage.TicketId);
        if (!ticketExists)
            return (false, null, "تیکت مورد نظر یافت نشد");

        var senderExists = db.Users.Any(u => u.Id == newMessage.SenderId) ||
                          db.Admins.Any(a => a.Id == newMessage.SenderId);
        if (!senderExists)
            return (false, null, "فرستنده نامعتبر است");

        newMessage.Id = Guid.NewGuid().ToString()[..8];
        newMessage.Timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        newMessage.IsRead = false;

        db.Messages.Add(newMessage);
        await _dataService.WriteAsync(db);

        return (true, newMessage, null);
    }

    public async Task<(bool success, Message? data, string? error)> MarkAsReadAsync(string messageId)
    {
        var db = await _dataService.ReadAsync();
        var message = db.Messages.FirstOrDefault(m => m.Id == messageId);

        if (message == null)
            return (false, null, "پیام یافت نشد");

        message.IsRead = true;
        await _dataService.WriteAsync(db);

        return (true, message, null);
    }
}