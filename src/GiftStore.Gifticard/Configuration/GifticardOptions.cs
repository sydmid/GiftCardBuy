namespace GiftStore.Gifticard.Configuration;

public class GifticardOptions
{
    public const string SectionName = "Gifticard";

    public string Environment { get; set; } = "Sandbox"; // "Sandbox" or "Production"
    public bool UseFakeInDevelopment { get; set; } = true;
    public GifticardEnvironmentProfile Sandbox { get; set; } = new()
    {
        BaseUrl = "https://api.gifticard.ir/",
        ApiToken = "sandbox_demo_token_123"
    };
    public GifticardEnvironmentProfile Production { get; set; } = new()
    {
        BaseUrl = "https://api.gifticard.ir/"
    };

    public GifticardEnvironmentProfile ActiveProfile =>
        string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase) ? Production : Sandbox;
}

public class GifticardEnvironmentProfile
{
    public string BaseUrl { get; set; } = "https://api.gifticard.ir/";
    public string? ApiToken { get; set; }
    public string? ConsumerKey { get; set; }
    public string? ConsumerSecret { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
