using Microsoft.AspNetCore.Mvc;
using AvalWebBack.Services;

namespace AvalWebBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FinancialStatsController : ControllerBase
{
    private readonly JsonDataService _dataService;

    public FinancialStatsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var db = await _dataService.ReadAsync();

        
        return Ok(new
        {
            day = new
            {
                labels = db.FinancialStats.Day.Labels,
                requests = db.FinancialStats.Day.Requests,
                payments = db.FinancialStats.Day.Payments
            },
            week = new
            {
                labels = db.FinancialStats.Week.Labels,
                requests = db.FinancialStats.Week.Requests,
                payments = db.FinancialStats.Week.Payments
            },
            month = new
            {
                labels = db.FinancialStats.Month.Labels,
                requests = db.FinancialStats.Month.Requests,
                payments = db.FinancialStats.Month.Payments
            }
        });
    }
}