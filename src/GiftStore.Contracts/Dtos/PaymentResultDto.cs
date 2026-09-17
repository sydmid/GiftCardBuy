namespace GiftStore.Contracts.Dtos;

using GiftStore.Contracts.Enums;

public record PaymentInitiationDto(
    string PaymentAttemptId,
    string GatewayUrl,
    string TransactionReference,
    PaymentStatus Status
);

public record PaymentVerificationDto(
    bool IsSuccessful,
    string TransactionId,
    string? ReferenceCode,
    string? GatewayMessage,
    PaymentStatus Status
);
