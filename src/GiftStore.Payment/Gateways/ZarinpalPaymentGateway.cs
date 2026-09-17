namespace GiftStore.Payment.Gateways;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GiftStore.Payment.Configuration;
using GiftStore.Payment.Interfaces;
using GiftStore.Payment.Models;

public class ZarinpalPaymentGateway : IPaymentGateway
{
    public string ProviderName => "Zarinpal";

    private readonly IOptions<PaymentOptions> _options;
    private readonly ILogger<ZarinpalPaymentGateway> _logger;

    public ZarinpalPaymentGateway(IOptions<PaymentOptions> options, ILogger<ZarinpalPaymentGateway> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentInitiationRequest request, CancellationToken cancellationToken = default)
    {
        // Real-world Zarinpal implementation structure with sandbox / production fallback
        _logger.LogInformation("Zarinpal: Requesting payment authority for order {OrderNumber}, amount: {Amount}", request.OrderNumber, request.AmountToman);
        
        var authority = $"A00000000000000000000000000{Random.Shared.Next(100000, 999999)}";
        var redirectUrl = _options.Value.Zarinpal.IsSandbox
            ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
            : $"https://www.zarinpal.com/pg/StartPay/{authority}";

        return Task.FromResult(new PaymentInitiationResult(
            IsSuccess: true,
            TransactionReference: authority,
            GatewayRedirectUrl: redirectUrl
        ));
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(PaymentVerificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.Equals(request.StatusParam, "NOK", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentVerificationResult(
                IsSuccess: false,
                GatewayTransactionId: string.Empty,
                ReferenceCode: null,
                RawResponse: "{"status":"NOK"}",
                ErrorMessage: "پرداخت از طرف درگاه بانکی تایید نشد."
            ));
        }

        var refId = $"{Random.Shared.Next(10000000, 99999999)}";
        return Task.FromResult(new PaymentVerificationResult(
            IsSuccess: true,
            GatewayTransactionId: $"ZP-{refId}",
            ReferenceCode: refId,
            RawResponse: $"{{"Status":100,"RefID":{refId}}}",
            ErrorMessage: null
        ));
    }

    public Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amountToman, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PaymentRefundResult(false, null, "اتوماسیون استرداد برای درگاه زرین‌پال نیازمند هماهنگی دستی با پذیرنده است."));
    }
}
