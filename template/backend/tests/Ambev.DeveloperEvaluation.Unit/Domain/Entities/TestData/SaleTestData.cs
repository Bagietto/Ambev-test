using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sale and SaleItem entities using the Bogus library.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Number(1, 20))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(10m, 100m));

    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Address.City())
        .RuleFor(s => s.Status, f => SaleStatus.Active);

    /// <summary>
    /// Generates a valid SaleItem entity with customizable quantity and price.
    /// </summary>
    public static SaleItem GenerateValidSaleItem(int? quantity = null, decimal? unitPrice = null)
    {
        var item = SaleItemFaker.Generate();
        if (quantity.HasValue) item.Quantity = quantity.Value;
        if (unitPrice.HasValue) item.UnitPrice = unitPrice.Value;
        item.CalculateDiscountAndTotal();
        return item;
    }

    /// <summary>
    /// Generates a valid Sale entity with random details and no items initially.
    /// </summary>
    public static Sale GenerateValidSale()
    {
        return SaleFaker.Generate();
    }

    /// <summary>
    /// Generates a valid Sale entity loaded with random valid items.
    /// </summary>
    public static Sale GenerateValidSaleWithItems(int itemCount = 2)
    {
        var sale = GenerateValidSale();
        for (int i = 0; i < itemCount; i++)
        {
            sale.Items.Add(GenerateValidSaleItem());
        }
        sale.CalculateTotal();
        return sale;
    }
}
