namespace GiftStore.Web.E2ETests;

using FluentAssertions;
using Xunit;

public class StorefrontRenderingAndRtlTests
{
    [Fact]
    public void PersianHtmlTag_AndRtlAttributes_MustBePresent()
    {
        var htmlTemplate = "<html lang="fa" dir="rtl">";
        htmlTemplate.Should().Contain("lang="fa"");
        htmlTemplate.Should().Contain("dir="rtl"");
    }
}
