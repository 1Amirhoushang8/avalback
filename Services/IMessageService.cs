using AvalWebBack.Models;

namespace AvalWebBack.Services;

public interface IMessageService
{
    Task<(bool success, IEnumerable<Message>? data, string? error)> GetMessagesByTicketAsync(string ticketId);
    Task<(bool success, Message? data, string? error)> SendMessageAsync(Message newMessage);
    Task<(bool success, Message? data, string? error)> MarkAsReadAsync(string messageId);
}