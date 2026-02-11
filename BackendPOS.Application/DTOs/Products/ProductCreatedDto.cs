namespace BackendPOS.Application.DTOs.Products;

public class ProductCreatedDto
{
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}