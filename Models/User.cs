namespace AvalBackend.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? RoleKey { get; set; } 
    public string SerialNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Price { get; set; }
    public string Service { get; set; } = string.Empty;
    public string? PaymentType { get; set; }
    public string? MonthlyPayment { get; set; }
    public int? TotalMonths { get; set; }
    public string Status { get; set; } = "درحال-انجام"; 
}