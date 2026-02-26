using BackendPOS.Application.DTOs.Orders;
using BackendPOS.Domain.Entities;
using FluentValidation;

namespace BackendPOS.Application.Validators.Orders;

public class PayOrderValidator : AbstractValidator<PayOrderDto>
{
    public PayOrderValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);
        
        RuleFor(x => x.Reference)
            .NotEmpty()
            .When(x => x.Method != PaymentMethod.Cash)
            .WithMessage("Reference is required for non-cash payments");

    }
}