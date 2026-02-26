namespace BackendPOS.Application.DTOs.Orders;

public class OrderResponseDto
{
    public Guid OrderId{ get; set; } 
    public string OrderNo { get; set; } ="";
    public string Status { get; set; } = "";
    public decimal Total { get; set;}
    public DateTime? PaidAtUtc { get; set;}
    public List<OrderItemsResponseDto> Items { get; set; } = new();
}

public class OrderItemsResponseDto
{
    public Guid ProductId { get; set; }
    public int Qty { get; set;}
    public decimal UnitPrice { get; set;}
    public decimal Subtotal { get; set; }
}