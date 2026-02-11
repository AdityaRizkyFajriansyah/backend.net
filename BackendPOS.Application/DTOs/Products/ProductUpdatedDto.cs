namespace BackendPOS.Application.DTOs.Products;

public class ProductUpdatedDto
{
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public int stock { get; set; }
    public bool IsActive { get; set; }
}