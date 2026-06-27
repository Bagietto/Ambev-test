using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale and SaleItem entities.
/// Tests cover discounts calculations and domain constraints validations.
/// </summary>
public class SaleTests
{
    [Fact(DisplayName = "Sale status should change to Cancelled when Cancel is called")]
    public void Given_ActiveSale_When_Cancelled_Then_StatusShouldBeCancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Status = SaleStatus.Active;

        // Act
        sale.Cancel();

        // Assert
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
    }

    [Theory(DisplayName = "Discount should be 0% for purchases with less than 4 items")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_SaleItem_When_QuantityLessThan4_Then_DiscountShouldBeZero(int quantity)
    {
        // Arrange & Act
        var item = SaleTestData.GenerateValidSaleItem(quantity, 10.00m);

        // Assert
        Assert.Equal(0.00m, item.Discount);
        Assert.Equal(quantity * 10.00m, item.TotalAmount);
    }

    [Theory(DisplayName = "Discount should be 10% for purchases with 4 to 9 items")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Given_SaleItem_When_QuantityBetween4And9_Then_DiscountShouldBe10Percent(int quantity)
    {
        // Arrange & Act
        var unitPrice = 10.00m;
        var item = SaleTestData.GenerateValidSaleItem(quantity, unitPrice);

        // Assert
        var expectedOriginalTotal = quantity * unitPrice;
        var expectedDiscount = expectedOriginalTotal * 0.10m;
        var expectedTotal = expectedOriginalTotal - expectedDiscount;

        Assert.Equal(expectedDiscount, item.Discount);
        Assert.Equal(expectedTotal, item.TotalAmount);
    }

    [Theory(DisplayName = "Discount should be 20% for purchases with 10 to 20 items")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_SaleItem_When_QuantityBetween10And20_Then_DiscountShouldBe20Percent(int quantity)
    {
        // Arrange & Act
        var unitPrice = 10.00m;
        var item = SaleTestData.GenerateValidSaleItem(quantity, unitPrice);

        // Assert
        var expectedOriginalTotal = quantity * unitPrice;
        var expectedDiscount = expectedOriginalTotal * 0.20m;
        var expectedTotal = expectedOriginalTotal - expectedDiscount;

        Assert.Equal(expectedDiscount, item.Discount);
        Assert.Equal(expectedTotal, item.TotalAmount);
    }

    [Fact(DisplayName = "Validation should fail when sale has no items")]
    public void Given_SaleWithNoItems_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Items.Clear();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Error == "NotEmptyValidator");
    }

    [Fact(DisplayName = "Validation should fail when product quantity exceeds 20 items")]
    public void Given_SaleItemWithExcessiveQuantity_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleTestData.GenerateValidSaleItem(21, 10m);
        sale.Items.Add(item);

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
    }
}
