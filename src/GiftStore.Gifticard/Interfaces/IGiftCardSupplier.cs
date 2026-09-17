namespace GiftStore.Gifticard.Interfaces;

using GiftStore.Gifticard.Models;

public interface IGiftCardSupplier
{
    Task<SupplierAccountStatus> GetAccountStatusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierProductVariant>> GetVariantsAsync(CancellationToken cancellationToken = default);
    Task<PurchaseInitiationResult> InitiatePurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseConfirmationResult> ConfirmPurchaseAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default);
    Task<SupplierOrderResult> RetrieveOrderAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default);
}
