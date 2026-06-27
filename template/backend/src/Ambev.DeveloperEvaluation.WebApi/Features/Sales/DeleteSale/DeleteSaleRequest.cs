using System;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;

/// <summary>
/// Request model for deleting a sale.
/// </summary>
public class DeleteSaleRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the sale to delete.
    /// </summary>
    public Guid Id { get; set; }
}
