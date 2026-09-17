namespace GiftStore.Payment.Models;

public record PaymentInitiationRequest(
    Guid OrderId,
    string OrderNumber,
    decimal AmountToman,
    string Description,
    string CustomerEmail,
    string? CustomerMobile,
    string CallbackUrl
);

public record PaymentInitiationResult(
    bool IsSuccess,
    string TransactionReference,
    string GatewayRedirectUrl,
    string? ErrorMessage = null
);

public record PaymentVerificationRequest(
    Guid OrderId,
    string TransactionReference,
    decimal ExpectedAmountToman,
    string? Authority,
    string? StatusParam
);

public record PaymentVerificationResult(
    bool IsSuccess,
    string GatewayTransactionId,
    string? ReferenceCode,
    string RawResponse,
    string? ErrorMessage = null
);

public record PaymentRefundResult(
    bool IsSuccess,
    string? RefundTransactionId,
    string? ErrorMessage = null
);
