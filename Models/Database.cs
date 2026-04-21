using AvalWebBack.Models;

namespace AvalWebBack.Models;

public class Database
{
    public List<Admin> Admins { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public FinancialStats FinancialStats { get; set; } = new();
}