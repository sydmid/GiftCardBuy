namespace GiftStore.Domain.Aggregates.Cart;

using GiftStore.Domain.Common;

public class ShoppingCart : BaseEntity, IAggregateRoot
{
    public string? CustomerUserId { get; private set; }
    public string AnonymousCartSessionId { get; private set; } = string.Empty;

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private ShoppingCart() { }

    public ShoppingCart(string? customerUserId, string sessionId)
    {
        CustomerUserId = customerUserId;
        AnonymousCartSessionId = sessionId;
    }

    public void AddOrUpdateItem(Guid productVariantMappingId, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.ProductVariantMappingId == productVariantMappingId);
        if (existing != null)
        {
            existing.UpdateQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(Id, productVariantMappingId, quantity));
        }
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productVariantMappingId)
    {
        var item = _items.FirstOrDefault(i => i.ProductVariantMappingId == productVariantMappingId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public class CartItem : BaseEntity
{
    public Guid ShoppingCartId { get; private set; }
    public Guid ProductVariantMappingId { get; private set; }
    public int Quantity { get; private set; }

    private CartItem() { }

    public CartItem(Guid shoppingCartId, Guid productVariantMappingId, int quantity)
    {
        ShoppingCartId = shoppingCartId;
        ProductVariantMappingId = productVariantMappingId;
        Quantity = Math.Max(1, quantity);
    }

    public void UpdateQuantity(int quantity)
    {
        Quantity = Math.Max(1, quantity);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
