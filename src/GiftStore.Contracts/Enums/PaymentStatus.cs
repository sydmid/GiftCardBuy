namespace GiftStore.Contracts.Enums;

public enum PaymentStatus
{
    Initiated = 0,
    Pending = 1,
    Success = 2,
    Failed = 3,
    Expired = 4,
    Refunded = 5,
    DuplicateCallbackIgnored = 6
}
