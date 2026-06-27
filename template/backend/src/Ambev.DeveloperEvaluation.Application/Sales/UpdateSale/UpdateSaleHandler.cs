using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Application.Sales.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of UpdateSaleHandler.
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="mediator">The mediator instance</param>
    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IMediator mediator)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    /// <summary>
    /// Handles the UpdateSaleCommand request.
    /// </summary>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} was not found");

        // Take snapshot of previous state for events
        var previousItems = sale.Items.ToList();
        var previousStatus = sale.Status;

        // Map general properties
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
        sale.Status = command.Status;

        // Clear and reload items for reconciliation
        sale.Items.Clear();
        foreach (var itemCmd in command.Items)
        {
            var item = _mapper.Map<SaleItem>(itemCmd);
            sale.Items.Add(item);
        }

        // Recalculate totals and discounts
        sale.CalculateTotal();

        // Validate domain rules
        var domainValidationResult = sale.Validate();
        if (!domainValidationResult.IsValid)
        {
            var failures = domainValidationResult.Errors.Select(e => 
                new FluentValidation.Results.ValidationFailure(e.Error, e.Detail));
            throw new ValidationException(failures);
        }

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish events
        // 1. Check for item cancellations (removed items)
        foreach (var prevItem in previousItems)
        {
            if (!command.Items.Any(i => i.ProductId == prevItem.ProductId))
            {
                await _mediator.Publish(new ItemCancelledEvent(sale.Id, prevItem.ProductId, prevItem.ProductName), cancellationToken);
            }
        }

        // 2. Check for sale cancellation transition
        if (updatedSale.Status == SaleStatus.Cancelled && previousStatus != SaleStatus.Cancelled)
        {
            await _mediator.Publish(new SaleCancelledEvent(updatedSale), cancellationToken);
        }

        // 3. Publish modification event
        await _mediator.Publish(new SaleModifiedEvent(updatedSale), cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
