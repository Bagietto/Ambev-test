using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _mediator = Substitute.For<IMediator>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _mediator);
    }

    [Fact(DisplayName = "Given valid update sale data When updating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
        
        var existingSale = new Sale
        {
            Id = saleId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Old Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Old Branch",
            Items = new List<SaleItem>()
        };

        var mappedItem = new SaleItem
        {
            ProductId = command.Items[0].ProductId,
            ProductName = command.Items[0].ProductName,
            Quantity = command.Items[0].Quantity,
            UnitPrice = command.Items[0].UnitPrice
        };

        var result = new UpdateSaleResult
        {
            Id = saleId,
            TotalAmount = existingSale.TotalAmount
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<SaleItem>(Arg.Any<UpdateSaleItemCommand>()).Returns(mappedItem);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        // When
        var updateSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        updateSaleResult.Should().NotBeNull();
        updateSaleResult.Id.Should().Be(saleId);
        await _saleRepository.Received(1).UpdateAsync(existingSale, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale ID When updating sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given invalid update sale data When updating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new UpdateSaleCommand(); // Empty command will fail validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given update command with removed items When updating sale Then publishes ItemCancelledEvent")]
    public async Task Handle_RemovedItem_PublishesItemCancelledEvent()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId, 1); // Only 1 item remains in update command
        
        var removedProductId = Guid.NewGuid();
        var existingSale = new Sale
        {
            Id = saleId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = new List<SaleItem>()
            {
                new() { ProductId = command.Items[0].ProductId, ProductName = command.Items[0].ProductName, Quantity = 2, UnitPrice = 10m },
                new() { ProductId = removedProductId, ProductName = "Removed Product", Quantity = 1, UnitPrice = 20m }
            }
        };

        var mappedItem = new SaleItem
        {
            ProductId = command.Items[0].ProductId,
            ProductName = command.Items[0].ProductName,
            Quantity = command.Items[0].Quantity,
            UnitPrice = command.Items[0].UnitPrice
        };

        var result = new UpdateSaleResult { Id = saleId };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<SaleItem>(Arg.Any<UpdateSaleItemCommand>()).Returns(mappedItem);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(
            Arg.Is<ItemCancelledEvent>(e => e.SaleId == saleId && e.ProductId == removedProductId && e.ProductName == "Removed Product"), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact(DisplayName = "Given update command with cancelled status When updating sale Then publishes SaleCancelledEvent")]
    public async Task Handle_StatusTransitionToCancelled_PublishesSaleCancelledEvent()
    {
        // Given
        var saleId = Guid.NewGuid();
        var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
        command.Status = SaleStatus.Cancelled; // Setting status to Cancelled
        
        var existingSale = new Sale
        {
            Id = saleId,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Status = SaleStatus.Active, // Previous status was Active
            Items = new List<SaleItem>()
        };

        var mappedItem = new SaleItem
        {
            ProductId = command.Items[0].ProductId,
            ProductName = command.Items[0].ProductName,
            Quantity = command.Items[0].Quantity,
            UnitPrice = command.Items[0].UnitPrice
        };

        var result = new UpdateSaleResult { Id = saleId };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<SaleItem>(Arg.Any<UpdateSaleItemCommand>()).Returns(mappedItem);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(
            Arg.Is<SaleCancelledEvent>(e => e.Sale.Id == saleId), 
            Arg.Any<CancellationToken>()
        );
    }
}
