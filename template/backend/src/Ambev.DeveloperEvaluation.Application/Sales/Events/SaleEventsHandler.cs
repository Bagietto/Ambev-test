using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Handler for processing and logging sale domain/application events.
/// </summary>
public class SaleEventsHandler : 
    INotificationHandler<SaleCreatedEvent>,
    INotificationHandler<SaleModifiedEvent>,
    INotificationHandler<SaleCancelledEvent>,
    INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<SaleEventsHandler> _logger;

    public SaleEventsHandler(ILogger<SaleEventsHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event: SaleCreated - ID: {SaleId}, Number: {SaleNumber}, Customer: {CustomerName}, Total: {TotalAmount}",
            notification.Sale.Id, notification.Sale.SaleNumber, notification.Sale.CustomerName, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event: SaleModified - ID: {SaleId}, Number: {SaleNumber}, Customer: {CustomerName}, Total: {TotalAmount}",
            notification.Sale.Id, notification.Sale.SaleNumber, notification.Sale.CustomerName, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event: SaleCancelled - ID: {SaleId}, Number: {SaleNumber}, Customer: {CustomerName}",
            notification.Sale.Id, notification.Sale.SaleNumber, notification.Sale.CustomerName);
        return Task.CompletedTask;
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event: ItemCancelled - SaleID: {SaleId}, ProductID: {ProductId}, ProductName: {ProductName}",
            notification.SaleId, notification.ProductId, notification.ProductName);
        return Task.CompletedTask;
    }
}
