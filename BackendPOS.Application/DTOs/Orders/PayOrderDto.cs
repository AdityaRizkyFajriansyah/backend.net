using BackendPOS.Domain.Entities;

namespace BackendPOS.Application.DTOs.Orders;

public class PayOrderDto
{
    public PaymentMethod Method { get; set; }
    public Decimal Amount { get; set; }
    public string? Reference { get; set; }
}