using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly PosDbContext _db;
    public ProductsController(PosDbContext db) => _db = db;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Products.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        
        return Ok(items);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product req)
    {
        _db.Products.Add(req);
        await _db.SaveChangesAsync();
        return Ok(req);
    }
}