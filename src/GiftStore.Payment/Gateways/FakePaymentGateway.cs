namespace GiftStore.Payment.Gateways;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GiftStore.Payment.Configuration;
using GiftStore.Payment.Interfaces;
using GiftStore.Payment.Models;

public class FakePaymentGateway : IPaymentGateway
{
    public string ProviderName => "Fake";

    private readonly IOptions<PaymentOptions> _options;
    private readonly ILogger<FakePaymentGateway> _logger;

    public FakePaymentGateway(IOptions<PaymentOptions> options, ILogger<FakePaymentGateway> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken cancellationToken = default)
    {
        var token = $"FAKE-AUTH-{Guid.NewGuid():N}";
        var redirectUrl = $"{request.CallbackUrl}?Authority={token}&Status=OK&OrderId={request.OrderId}";

        _logger.LogInformation("FakePaymentGateway: Initiated payment for order {OrderNumber}, amount {Amount} Tomans. Redirecting to {Url}",
            request.OrderNumber, request.AmountToman, redirectUrl);

        return Task.FromResult(new PaymentInitiationResult(
            IsSuccess: true,
            TransactionReference: token,
            GatewayRedirectUrl: redirectUrl
        ));
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.Equals(request.StatusParam, "NOK", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("FakePaymentGateway: Payment cancelled or failed for order {OrderId}", request.OrderId);
            return Task.FromResult(new PaymentVerificationResult(
                IsSuccess: false,
                GatewayTransactionId: string.Empty,
                ReferenceCode: null,
                RawResponse: "{"status":"NOK","message":"User cancelled transaction"}",
                ErrorMessage: "تراکنش توسط کاربر لغو شد یا ناموفق بود."
            ));
        }

        var refNumber = $"{Random.Shared.Next(10000000, 99999999)}";
        _logger.LogInformation("FakePaymentGateway: Payment verified successfully for order {OrderId}. Ref: {Ref}", request.OrderId, refNumber);

        return Task.FromResult(new PaymentVerificationResult(
            IsSuccess: true,
            GatewayTransactionId: $"TX-{refNumber}",
            ReferenceCode: refNumber,
            RawResponse: $"{{"status":100,"ref_id":"{refNumber}","amount":{request.ExpectedAmountToman}}}",
            ErrorMessage: null
        ));
    }

    public Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amountToman, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FakePaymentGateway: Refund executed for tx {Tx}, amount {Amount} Tomans", transactionId, amountToman);
        return Task.FromResult(new PaymentRefundResult(true, $"REFUND-{Guid.NewGuid():N}"));
    }
}
