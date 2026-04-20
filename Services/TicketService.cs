using System.Text.Json;
using AvalWebBack.Models;

namespace AvalWebBack.Services;

public class TicketService : ITicketService
{
    private readonly JsonDataService _dataService;

    public TicketService(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<(bool success, IEnumerable<Ticket>? data, string? error)> GetAllTicketsAsync(string? userId)
    {
        var db = await _dataService.ReadAsync();
        var tickets = db.Tickets.AsEnumerable();

        if (!string.IsNullOrEmpty(userId))
            tickets = tickets.Where(t => t.UserId == userId);

        return (true, tickets, null);
    }

    public async Task<(bool success, Ticket? data, string? error)> GetTicketByIdAsync(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return (false, null, "تیکت یافت نشد");

        return (true, ticket, null);
    }

    public async Task<(bool success, Ticket? data, string? error)> CreateTicketAsync(Ticket newTicket)
    {
        var db = await _dataService.ReadAsync();

        if (!db.Users.Any(u => u.Id == newTicket.UserId))
            return (false, null, "کاربر نامعتبر است");

        newTicket.Id = Guid.NewGuid().ToString()[..8];
        newTicket.Status = "pending";

        db.Tickets.Add(newTicket);
        await _dataService.WriteAsync(db);

        return (true, newTicket, null);
    }

    public async Task<(bool success, Ticket? data, string? error)> UpdateTicketAsync(string id, Ticket updatedTicket)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return (false, null, "تیکت یافت نشد");

        ticket.Title = updatedTicket.Title;
        ticket.ShortDetail = updatedTicket.ShortDetail;
        ticket.Description = updatedTicket.Description;
        ticket.Status = updatedTicket.Status;
        ticket.AdminResponse = updatedTicket.AdminResponse;
        // File is intentionally not updated via this generic PUT

        await _dataService.WriteAsync(db);
        return (true, ticket, null);
    }

    public async Task<(bool success, string? error)> DeleteTicketAsync(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return (false, "تیکت یافت نشد");

        db.Tickets.Remove(ticket);
        await _dataService.WriteAsync(db);

        return (true, null);
    }

    public async Task<(bool success, byte[]? fileBytes, string? contentType, string? fileName, string? error)> GetTicketFileAsync(string id)
    {
        var db = await _dataService.ReadAsync();
        var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);

        if (ticket?.File == null)
            return (false, null, null, null, "فایلی برای این تیکت موجود نیست");

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
                    return (false, null, null, null, "داده فایل خالی است");

                var base64Data = base64WithPrefix.Contains(',')
                    ? base64WithPrefix[(base64WithPrefix.IndexOf(',') + 1)..]
                    : base64WithPrefix;

                var fileBytes = Convert.FromBase64String(base64Data);
                return (true, fileBytes, contentType, fileName, null);
            }

            return (false, null, null, null, "ساختار فایل نامعتبر است");
        }
        catch (Exception ex)
        {
            return (false, null, null, null, $"خطا در پردازش فایل: {ex.Message}");
        }
    }
}