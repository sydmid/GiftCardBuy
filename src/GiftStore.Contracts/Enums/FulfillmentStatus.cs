namespace GiftStore.Contracts.Enums;

public enum FulfillmentStatus
{
    Pending = 0,
    SupplierRequested = 1,
    SupplierConfirmed = 2,
    CodesRetrieved = 3,
    Failed = 4,
    ManualReview = 5
}
