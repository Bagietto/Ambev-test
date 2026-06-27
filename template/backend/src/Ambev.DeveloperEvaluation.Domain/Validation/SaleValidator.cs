using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for the Sale entity.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("Customer ID must not be empty.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("Customer Name must not be empty.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("Branch ID must not be empty.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("Branch Name must not be empty.");

        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(sale => sale.Items)
            .SetValidator(new SaleItemValidator());
    }
}
