using Microsoft.AspNetCore.Mvc;
using AvalBackend.Services;
using AvalBackend.Models;
using System.Linq;

namespace AvalBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly JsonDataService _dataService;

    public UsersController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var db = await _dataService.ReadAsync();
        
        return Ok(db.Users);
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        return Ok(user);
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User newUser)
    {
        var db = await _dataService.ReadAsync();

        
        if (string.IsNullOrEmpty(newUser.Id))
            newUser.Id = Guid.NewGuid().ToString().Substring(0, 8);

       
        if (db.Users.Any(u => u.SerialNumber == newUser.SerialNumber))
            return BadRequest(new { message = "شماره فاکتور تکراری است" });

        db.Users.Add(newUser);
        await _dataService.WriteAsync(db);

        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] User updatedUser)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

       
        if (!string.IsNullOrEmpty(updatedUser.SerialNumber) &&
            db.Users.Any(u => u.SerialNumber == updatedUser.SerialNumber && u.Id != id))
        {
            return BadRequest(new { message = "شماره فاکتور تکراری است" });
        }

        
        user.FullName = updatedUser.FullName ?? user.FullName;
        user.PhoneNumber = updatedUser.PhoneNumber ?? user.PhoneNumber;
        user.SerialNumber = updatedUser.SerialNumber ?? user.SerialNumber;
        user.Service = updatedUser.Service ?? user.Service;
        user.Price = updatedUser.Price ?? user.Price;
        user.Status = updatedUser.Status ?? user.Status;
        user.PaymentType = updatedUser.PaymentType ?? user.PaymentType;
        user.MonthlyPayment = updatedUser.MonthlyPayment ?? user.MonthlyPayment;
        user.TotalMonths = updatedUser.TotalMonths ?? user.TotalMonths;

        await _dataService.WriteAsync(db);
        return Ok(user);
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var db = await _dataService.ReadAsync();
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        db.Users.Remove(user);
        await _dataService.WriteAsync(db);

        return NoContent();
    }
}