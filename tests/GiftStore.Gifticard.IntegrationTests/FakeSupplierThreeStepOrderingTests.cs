namespace GiftStore.Gifticard.IntegrationTests;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using GiftStore.Gifticard.Fakes;
using GiftStore.Gifticard.Models;

public class FakeSupplierThreeStepOrderingTests
{
    [Fact]
    public async Task FullThreeStepFlow_Initiate_Confirm_Retrieve_ShouldSucceed()
    {
        var supplier = new FakeGiftCardSupplier(NullLogger<FakeGiftCardSupplier>.Instance);

        // Step 1: Initiate
        var req = new PurchaseRequest("gic-app-us-10", 2, "LOCAL-ORD-101");
        var initResult = await supplier.InitiatePurchaseAsync(req);

        initResult.IsSuccess.Should().BeTrue();
        initResult.TrackingCode.Should().StartWith("ORD-");
        initResult.Status.Should().Be("waiting_payment");

        // Step 2: Confirm
        var pref = new SupplierPurchaseReference(initResult.TrackingCode!);
        var confirmResult = await supplier.ConfirmPurchaseAsync(pref);

        confirmResult.IsSuccess.Should().BeTrue();
        confirmResult.Status.Should().Be("processing");

        // Step 3: Retrieve
        var retrieveResult = await supplier.RetrieveOrderAsync(pref);

        retrieveResult.IsSuccess.Should().BeTrue();
        retrieveResult.Status.Should().Be("completed");
        retrieveResult.Cards.Should().HaveCount(2);
        retrieveResult.Cards[0].Code.Should().NotBeNullOrWhiteSpace();
    }
}
