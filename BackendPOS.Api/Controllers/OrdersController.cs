using BackendPOS.Application.Common;
using BackendPOS.Application.DTOs.Orders;
using BackendPOS.Application.Services;
using BackendPOS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendPOS.Api.Controllers;

[Authorize(Roles = "Admin,Cashier")]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService) => _orderService = orderService;

    private ActorContext Actor()
    {
        var username = User.Identity?.Name;
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        return new ActorContext(username, role, ip, ua);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var id = await _orderService.CreateAsync(dto, Actor());
            return CreatedAtAction(nameof(GetById), new { id }, new { orderId = id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Order not found" });
        }
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayOrderDto dto)
    {
        try
        {
            var order = await _orderService.PayAsync(id, dto, Actor());
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Order not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var orders = await _orderService.GetAllAsync(status, from, to);
        return Ok(orders);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _orderService.CancelAsync(id, Actor());
            return Ok(new { message = "Order cancelled", orderId = id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Order not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}