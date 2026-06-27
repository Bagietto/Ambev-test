using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an item in a sale, containing product info, quantity, pricing, and discount.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the sale this item belongs to.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the external product identifier.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the desnormalized name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of units sold for this product.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the total discount applied to this item.
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this item (Quantity * UnitPrice - Discount).
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Calculates the discount and total amount for the item based on the business rules:
    /// - Compras com mais de 4 unidades idênticas recebem 10% de desconto.
    /// - Compras entre 10 e 20 unidades idênticas recebem 20% de desconto.
    /// - Compras com menos de 4 unidades não recebem desconto.
    /// </summary>
    public void CalculateDiscountAndTotal()
    {
        decimal discountPercentage = 0m;

        if (Quantity >= 10)
        {
            discountPercentage = 0.20m;
        }
        else if (Quantity >= 4)
        {
            discountPercentage = 0.10m;
        }

        Discount = Quantity * UnitPrice * discountPercentage;
        TotalAmount = (Quantity * UnitPrice) - Discount;
    }
}
