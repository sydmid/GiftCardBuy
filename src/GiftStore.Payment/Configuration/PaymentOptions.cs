namespace GiftStore.Payment.Configuration;

public class PaymentOptions
{
    public const string SectionName = "Payment";

    public string Provider { get; set; } = "Fake"; // "Fake" or "Zarinpal"
    public string CallbackBaseUrl { get; set; } = "https://localhost:7001/checkout/payment-return";
    public FakeGatewayOptions Fake { get; set; } = new();
    public ZarinpalOptions Zarinpal { get; set; } = new();
}

public class FakeGatewayOptions
{
    public bool AutoApprove { get; set; } = true;
}

public class ZarinpalOptions
{
    public string MerchantId { get; set; } = "00000000-0000-0000-0000-000000000000";
    public bool IsSandbox { get; set; } = true;
    public string BaseUrl => IsSandbox
        ? "https://sandbox.zarinpal.com/pg/rest/WebGate/"
        : "https://api.zarinpal.com/pg/v4/payment/";
}
