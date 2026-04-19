namespace AvalBackend.Models;

public class Message
{
    public string Id { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty; // "user" or "admin"
    public string Text { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}