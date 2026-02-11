namespace BackendPOS.Application.DTOs.Products;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive{ get; set; }
}