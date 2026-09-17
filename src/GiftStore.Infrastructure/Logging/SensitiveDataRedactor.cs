namespace GiftStore.Infrastructure.Logging;

using System.Text.RegularExpressions;

public static partial class SensitiveDataRedactor
{
    // Regex patterns for sensitive codes, authorization headers, and tokens
    private static readonly Regex BearerTokenRegex = new(@"Bearer\s+([A-Za-z0-9_\-\.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GiftCardCodeRegex = new(@"(?:code|pin|serial)[""']?\s*[:=]\s*[""']?([A-Za-z0-9\-]{10,30})[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PasswordRegex = new(@"(?:password|secret)[""']?\s*[:=]\s*[""']?([^""'\s]+)[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Redact(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var redacted = BearerTokenRegex.Replace(input, "Bearer [REDACTED_TOKEN]");
        redacted = GiftCardCodeRegex.Replace(redacted, "code:"[REDACTED_GIFT_CODE]"");
        redacted = PasswordRegex.Replace(redacted, "secret:"[REDACTED_SECRET]"");
        return redacted;
    }
}
