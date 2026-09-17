namespace GiftStore.Contracts.Enums;

public enum OrderStatus
{
    Draft = 0,
    AwaitingPayment = 10,
    PaymentPendingVerification = 20,
    PaymentConfirmed = 30,
    FulfillmentPending = 40,
    SupplierOrderCreated = 50,
    SupplierOrderConfirmationPending = 60,
    SupplierOrderRetrievalPending = 70,
    Fulfilled = 80,
    FulfillmentFailed = 90,
    PaymentFailed = 100,
    PaymentExpired = 110,
    RefundPending = 120,
    Refunded = 130,
    Cancelled = 140,
    ManualReviewRequired = 150
}
