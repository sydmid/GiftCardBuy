namespace GiftStore.Domain.Aggregates.Catalog;

using GiftStore.Contracts.Enums;
using GiftStore.Domain.Common;

public class StoreProduct : BaseEntity, IAggregateRoot
{
    public string TitleFa { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public CardBrand Brand { get; private set; }
    public string DescriptionFa { get; private set; } = string.Empty;
    public string InstructionsFa { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = "US";
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public int DisplayOrder { get; private set; }

    private readonly List<ProductVariantMapping> _variantMappings = [];
    public IReadOnlyCollection<ProductVariantMapping> VariantMappings => _variantMappings.AsReadOnly();

    private StoreProduct() { }

    public StoreProduct(
        string titleFa,
        string slug,
        CardBrand brand,
        string descriptionFa,
        string instructionsFa,
        string countryCode,
        string imageUrl,
        int displayOrder = 0)
    {
        TitleFa = titleFa;
        Slug = slug;
        Brand = brand;
        DescriptionFa = descriptionFa;
        InstructionsFa = instructionsFa;
        CountryCode = countryCode;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsPublished = true;
    }

    public void AddVariantMapping(ProductVariantMapping mapping)
    {
        _variantMappings.Add(mapping);
    }

    public void SetPublished(bool published)
    {
        IsPublished = published;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDetails(string titleFa, string descriptionFa, string instructionsFa, string imageUrl, int displayOrder)
    {
        TitleFa = titleFa;
        DescriptionFa = descriptionFa;
        InstructionsFa = instructionsFa;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
