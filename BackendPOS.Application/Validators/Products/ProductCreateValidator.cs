using BackendPOS.Application.DTOs.Products;
using FluentValidation;

namespace BackendPOS.Application.Validators.Products;

public class ProductCreateValidator : AbstractValidator<ProductCreatedDto>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(150);
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50);
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price must be greater than 0");
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
        
    }
}