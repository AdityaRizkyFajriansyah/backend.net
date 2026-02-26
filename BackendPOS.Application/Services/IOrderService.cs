using BackendPOS.Application.Common;
using BackendPOS.Application.DTOs.Orders;
using BackendPOS.Domain.Entities;

namespace BackendPOS.Application.Services;

public interface IOrderService
{
    Task<Guid> CreateAsync(CreateOrderDto dto, ActorContext actor);
    Task<OrderResponseDto> GetByIdAsync(Guid id);
    Task<OrderResponseDto> PayAsync(Guid id, PayOrderDto dto, ActorContext actor);
    Task<List<OrderResponseDto>> GetAllAsync(OrderStatus? status, DateTime? from, DateTime? to);
    Task CancelAsync(Guid id, ActorContext actor);
}