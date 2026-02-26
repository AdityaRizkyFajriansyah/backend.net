using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Api.Controllers;

[Authorize(Roles =  "Admin")]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly PosDbContext _db;
    public ReportsController(PosDbContext db) => _db = db;

    [HttpGet("sales/daily")]
    public async Task<IActionResult> DailySales([FromQuery] DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var total = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid &&
                        o.PaidAtUtc >= start &&
                        o.PaidAtUtc < end)
            .SumAsync(o => o.Total);
        
        return Ok(new { date = start, totalSales = total});
    }
}