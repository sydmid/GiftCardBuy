namespace GiftStore.Domain.Aggregates.Catalog;

using GiftStore.Domain.Common;

public enum PriceRuleType
{
    PercentageMargin = 1,
    FixedMargin = 2,
    FlatPromotion = 3
}

public class PriceRule : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public PriceRuleType RuleType { get; private set; }
    public decimal Value { get; private set; } // e.g. 5.0 for 5%, or 20000 for 20,000 Tomans
    public bool RoundToNearestThousand { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public DateTime? EffectiveStartDateUtc { get; private set; }
    public DateTime? EffectiveEndDateUtc { get; private set; }

    private PriceRule() { }

    public PriceRule(string name, PriceRuleType ruleType, decimal value, bool round = true)
    {
        Name = name;
        RuleType = ruleType;
        Value = value;
        RoundToNearestThousand = round;
        IsActive = true;
    }

    public decimal CalculateSellingPrice(decimal supplierCost)
    {
        if (!IsActive) return supplierCost;
        if (EffectiveStartDateUtc.HasValue && DateTime.UtcNow < EffectiveStartDateUtc.Value) return supplierCost;
        if (EffectiveEndDateUtc.HasValue && DateTime.UtcNow > EffectiveEndDateUtc.Value) return supplierCost;

        decimal calculated = RuleType switch
        {
            PriceRuleType.PercentageMargin => supplierCost * (1m + (Value / 100m)),
            PriceRuleType.FixedMargin => supplierCost + Value,
            PriceRuleType.FlatPromotion => Math.Max(supplierCost * 0.9m, supplierCost - Value),
            _ => supplierCost
        };

        if (RoundToNearestThousand)
        {
            // Round up to nearest 1,000 Tomans for clean Persian retail pricing
            calculated = Math.Ceiling(calculated / 1000m) * 1000m;
        }

        return calculated;
    }

    public void Update(string name, PriceRuleType ruleType, decimal value, bool round, bool isActive)
    {
        Name = name;
        RuleType = ruleType;
        Value = value;
        RoundToNearestThousand = round;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
