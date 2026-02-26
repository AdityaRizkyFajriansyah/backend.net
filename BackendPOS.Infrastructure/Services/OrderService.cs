using BackendPOS.Application.Common;
using BackendPOS.Application.DTOs.Orders;
using BackendPOS.Application.Services;
using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BackendPOS.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly PosDbContext _db;
    private readonly IAuditService _audit;

    public OrderService(PosDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Guid> CreateAsync(CreateOrderDto dto, ActorContext actor)
    {
        if (dto?.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Items is required");

        var productIds = dto.Items.Select(x => x.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new ArgumentException("One or more products not found");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNo = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var reqItem in dto.Items)
        {
            if (reqItem.Qty <= 0)
                throw new ArgumentException("Qty must be greater than 0");

            var product = products.First(p => p.Id == reqItem.ProductId);

            var unitPrice = product.Price;
            var subTotal = unitPrice * reqItem.Qty;

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Qty = reqItem.Qty,
                UnitPrice = unitPrice,
                SubTotal = subTotal
            });

            total += subTotal;
        }

        order.Total = total;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _audit.WriteAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "ORDER_CREATE",
            ActorUsername = actor.Username,
            ActorRole = actor.Role,
            EntityType = "Order",
            EntityId = order.Id.ToString(),
            IpAddress = actor.Ip,
            UserAgent = actor.UserAgent,
            DetailJson = $"{{\"orderNo\":\"{order.OrderNo}\",\"total\":{order.Total}}}"
        });

        return order.Id;
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new KeyNotFoundException("Order not found");

        return MapOrder(order);
    }

    public async Task<OrderResponseDto> PayAsync(Guid id, PayOrderDto dto, ActorContext actor)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new KeyNotFoundException("Order not found");

        if (order.Status != OrderStatus.Draft)
            throw new ArgumentException($"Order cannot be paid (status: {order.Status})");

        if (dto.Method == PaymentMethod.Cash && dto.Amount < order.Total)
            throw new ArgumentException("Cash amount is less than order total");

        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // Stock deduction (atomic)
            foreach (var item in order.Items)
            {
                var affected = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE products
                    SET stock = stock - {item.Qty}
                    WHERE id = {item.ProductId} AND stock >= {item.Qty};
                ");

                if (affected != 1)
                    throw new ArgumentException("insufficient stock for one or more products");
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Method = dto.Method,
                Amount = dto.Amount,
                Change = dto.Method == PaymentMethod.Cash ? dto.Amount - order.Total : null,
                Reference = dto.Method == PaymentMethod.Cash ? null : dto.Reference,
                PaidAtUtc = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            order.Status = OrderStatus.Paid;
            order.PaidAtUtc = payment.PaidAtUtc;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            await _audit.WriteAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = "ORDER_PAY",
                ActorUsername = actor.Username,
                ActorRole = actor.Role,
                EntityType = "Order",
                EntityId = order.Id.ToString(),
                IpAddress = actor.Ip,
                UserAgent = actor.UserAgent,
                DetailJson = $"{{\"amount\":{dto.Amount},\"method\":\"{dto.Method}\",\"total\":{order.Total}}}"
            });

            return MapOrder(order);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetAllAsync(OrderStatus? status, DateTime? from, DateTime? to)
    {
        var query = _db.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (from.HasValue)
            query = query.Where(o => o.CreatedAtUtc >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.CreatedAtUtc <= to.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task CancelAsync(Guid id, ActorContext actor)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new KeyNotFoundException("Order not found");

        if (order.Status != OrderStatus.Draft)
            throw new ArgumentException("Only Draft orders can be cancelled");

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();

        await _audit.WriteAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "ORDER_CANCEL",
            ActorUsername = actor.Username,
            ActorRole = actor.Role,
            EntityType = "Order",
            EntityId = id.ToString(),
            IpAddress = actor.Ip,
            UserAgent = actor.UserAgent
        });
    }

    private static OrderResponseDto MapOrder(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.Id,
            OrderNo = order.OrderNo,
            Status = order.Status.ToString(),
            Total = order.Total,
            PaidAtUtc = order.PaidAtUtc,
            Items = order.Items.Select(i => new OrderItemsResponseDto
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}