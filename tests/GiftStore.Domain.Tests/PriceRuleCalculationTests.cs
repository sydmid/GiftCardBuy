namespace GiftStore.Domain.Tests;

using FluentAssertions;
using Xunit;
using GiftStore.Domain.Aggregates.Catalog;

public class PriceRuleCalculationTests
{
    [Fact]
    public void PercentageMargin_WithRounding_ShouldRoundToNearestThousand()
    {
        // 620,000 * 1.07 = 663,400 -> Rounded up to 664,000 Tomans
        var rule = new PriceRule("7% Margin", PriceRuleType.PercentageMargin, 7.0m, round: true);
        var price = rule.CalculateSellingPrice(620_000m);
        price.Should().Be(664_000m);
    }

    [Fact]
    public void FixedMargin_ShouldAddExactValue()
    {
        var rule = new PriceRule("50k Fixed", PriceRuleType.FixedMargin, 50_000m, round: false);
        var price = rule.CalculateSellingPrice(620_000m);
        price.Should().Be(670_000m);
    }
}
