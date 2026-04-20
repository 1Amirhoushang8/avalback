using AvalWebBack.Models;

namespace AvalWebBack.Services;

public interface ITicketService
{
    Task<(bool success, IEnumerable<Ticket>? data, string? error)> GetAllTicketsAsync(string? userId);
    Task<(bool success, Ticket? data, string? error)> GetTicketByIdAsync(string id);
    Task<(bool success, Ticket? data, string? error)> CreateTicketAsync(Ticket newTicket);
    Task<(bool success, Ticket? data, string? error)> UpdateTicketAsync(string id, Ticket updatedTicket);
    Task<(bool success, string? error)> DeleteTicketAsync(string id);
    Task<(bool success, byte[]? fileBytes, string? contentType, string? fileName, string? error)> GetTicketFileAsync(string id);
}