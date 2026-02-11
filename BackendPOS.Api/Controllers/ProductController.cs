using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsControlller : ControllerBase
{
    private readonly PosDbContext _db;
    public ProductsControlller(PosDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _db.Products.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product req)
    {
        _db.Products.Add(req);
        await _db.SaveChangesAsync();
        return Ok(req);
    }
}