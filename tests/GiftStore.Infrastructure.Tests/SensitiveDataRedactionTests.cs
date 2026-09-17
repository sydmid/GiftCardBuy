namespace GiftStore.Infrastructure.Tests;

using FluentAssertions;
using Xunit;
using GiftStore.Infrastructure.Logging;

public class SensitiveDataRedactionTests
{
    [Fact]
    public void Redact_ShouldMaskBearerTokens()
    {
        var rawLog = "Sending request with header Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.secret_token";
        var redacted = SensitiveDataRedactor.Redact(rawLog);

        redacted.Should().NotContain("eyJhbGciOiJIUzI1NiJ9");
        redacted.Should().Contain("Bearer [REDACTED_TOKEN]");
    }

    [Fact]
    public void Redact_ShouldMaskGiftCardCodesAndPins()
    {
        var rawLog = "Retrieved response: code:"XABCD1234567890" from supplier";
        var redacted = SensitiveDataRedactor.Redact(rawLog);

        redacted.Should().NotContain("XABCD1234567890");
        redacted.Should().Contain("[REDACTED_GIFT_CODE]");
    }
}
