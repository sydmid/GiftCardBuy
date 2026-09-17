namespace GiftStore.Gifticard.Dtos;

using System.Text.Json.Serialization;

public class GifticardStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("account_name")]
    public string? AccountName { get; set; }

    [JsonPropertyName("wallet_balance")]
    public decimal WalletBalance { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "IRT";
}

public class GifticardVariantDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = "US";

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("face_value")]
    public decimal FaceValue { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stock_status")]
    public string StockStatus { get; set; } = "instock"; // "instock" or "outofstock"
}

public class GifticardBuyRequest
{
    [JsonPropertyName("variant_id")]
    public string VariantId { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }
}

public class GifticardBuyResponse
{
    [JsonPropertyName("tracking_code")]
    public string TrackingCode { get; set; } = string.Empty;

    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "waiting_payment";

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class GifticardConfirmRequest
{
    [JsonPropertyName("tracking_code")]
    public string TrackingCode { get; set; } = string.Empty;
}

public class GifticardConfirmResponse
{
    [JsonPropertyName("tracking_code")]
    public string TrackingCode { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "processing";

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class GifticardCardItemDto
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("pin")]
    public string? Pin { get; set; }

    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("expire_date")]
    public string? ExpireDate { get; set; }
}

public class GifticardRetrieveResponse
{
    [JsonPropertyName("tracking_code")]
    public string TrackingCode { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "completed"; // "completed", "processing", "failed"

    [JsonPropertyName("cards")]
    public List<GifticardCardItemDto> Cards { get; set; } = [];

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
