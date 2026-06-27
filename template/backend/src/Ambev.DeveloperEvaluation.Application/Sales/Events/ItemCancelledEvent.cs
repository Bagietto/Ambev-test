using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

/// <summary>
/// Event published when an item inside a sale is cancelled.
/// </summary>
public class ItemCancelledEvent : INotification
{
    public Guid SaleId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }

    public ItemCancelledEvent(Guid saleId, Guid productId, string productName)
    {
        SaleId = saleId;
        ProductId = productId;
        ProductName = productName;
    }
}
