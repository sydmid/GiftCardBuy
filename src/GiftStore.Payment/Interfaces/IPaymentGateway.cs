namespace GiftStore.Payment.Interfaces;

using GiftStore.Payment.Models;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken cancellationToken = default);
    Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerificationRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amountToman, CancellationToken cancellationToken = default);
}
