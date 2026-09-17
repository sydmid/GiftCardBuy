namespace GiftStore.Gifticard.Client;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GiftStore.Contracts.Enums;
using GiftStore.Gifticard.Configuration;
using GiftStore.Gifticard.Dtos;
using GiftStore.Gifticard.Interfaces;
using GiftStore.Gifticard.Models;

public class GifticardClient : IGiftCardSupplier
{
    private readonly HttpClient _httpClient;
    private readonly GifticardOptions _options;
    private readonly ILogger<GifticardClient> _logger;

    public GifticardClient(
        HttpClient httpClient,
        IOptions<GifticardOptions> options,
        ILogger<GifticardClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        var profile = _options.ActiveProfile;
        _httpClient.BaseAddress = new Uri(profile.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(profile.TimeoutSeconds);

        if (!string.IsNullOrWhiteSpace(profile.ApiToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", profile.ApiToken);
        }
    }

    public async Task<SupplierAccountStatus> GetAccountStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gift-i-Card: Checking account status at /auth/status");
            var response = await _httpClient.GetFromJsonAsync<GifticardStatusResponse>("/auth/status", cancellationToken);
            if (response == null)
            {
                return new SupplierAccountStatus(false, "Unknown", 0m, "IRT", DateTime.UtcNow);
            }

            return new SupplierAccountStatus(
                IsConnected: string.Equals(response.Status, "active", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(response.Status, "ok", StringComparison.OrdinalIgnoreCase),
                AccountName: response.AccountName ?? "Gifticard Business",
                WalletBalanceToman: response.WalletBalance,
                Currency: response.Currency,
                CheckedAtUtc: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gift-i-Card: Failed to fetch account status");
            return new SupplierAccountStatus(false, "Disconnected", 0m, "IRT", DateTime.UtcNow);
        }
    }

    public async Task<IReadOnlyList<SupplierProductVariant>> GetVariantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gift-i-Card: Fetching variant list from /giftcard/variant-list");
            var variants = await _httpClient.GetFromJsonAsync<List<GifticardVariantDto>>("/giftcard/variant-list", cancellationToken);
            if (variants == null) return [];

            return variants.Select(v => new SupplierProductVariant(
                VariantId: v.Id,
                ProductId: v.ProductId,
                Brand: MapBrandFromTitle(v.Title),
                Title: v.Title,
                Sku: v.Sku,
                Country: v.Country,
                Currency: v.Currency,
                FaceValue: v.FaceValue,
                PriceToman: v.Price,
                InStock: string.Equals(v.StockStatus, "instock", StringComparison.OrdinalIgnoreCase)
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gift-i-Card: Error retrieving variant list");
            throw;
        }
    }

    public async Task<PurchaseInitiationResult> InitiatePurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gift-i-Card: Initiating purchase for variant {VariantId}, qty {Qty}", request.SupplierVariantId, request.Quantity);

            var payload = new GifticardBuyRequest
            {
                VariantId = request.SupplierVariantId,
                Count = request.Quantity,
                CallbackUrl = request.CallbackUrl
            };

            var httpResponse = await _httpClient.PostAsJsonAsync("/giftcard/buy", payload, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gift-i-Card /giftcard/buy error {StatusCode}: {Error}", httpResponse.StatusCode, error);
                return new PurchaseInitiationResult(false, null, null, 0m, $"API returned {httpResponse.StatusCode}: {error}");
            }

            var result = await httpResponse.Content.ReadFromJsonAsync<GifticardBuyResponse>(cancellationToken: cancellationToken);
            if (result == null || string.IsNullOrWhiteSpace(result.TrackingCode))
            {
                return new PurchaseInitiationResult(false, null, null, 0m, "Invalid response from Gift-i-Card buy endpoint");
            }

            return new PurchaseInitiationResult(true, result.TrackingCode, result.Status, result.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gift-i-Card: Exception during InitiatePurchaseAsync");
            return new PurchaseInitiationResult(false, null, null, 0m, ex.Message);
        }
    }

    public async Task<PurchaseConfirmationResult> ConfirmPurchaseAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gift-i-Card: Confirming purchase for tracking code {TrackingCode}", reference.TrackingCode);

            var payload = new GifticardConfirmRequest
            {
                TrackingCode = reference.TrackingCode
            };

            var httpResponse = await _httpClient.PostAsJsonAsync("/giftcard/confirm", payload, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Gift-i-Card /giftcard/confirm error {StatusCode}: {Error}", httpResponse.StatusCode, error);
                return new PurchaseConfirmationResult(false, reference.TrackingCode, "failed", null, error);
            }

            var result = await httpResponse.Content.ReadFromJsonAsync<GifticardConfirmResponse>(cancellationToken: cancellationToken);
            return new PurchaseConfirmationResult(
                IsSuccess: true,
                TrackingCode: reference.TrackingCode,
                Status: result?.Status ?? "processing",
                Message: result?.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gift-i-Card: Exception during ConfirmPurchaseAsync");
            return new PurchaseConfirmationResult(false, reference.TrackingCode, "failed", null, ex.Message);
        }
    }

    public async Task<SupplierOrderResult> RetrieveOrderAsync(SupplierPurchaseReference reference, CancellationToken cancellationToken = default)
    {
        try
        {
            // Official docs: /giftcard/retrieve?tracking_code=ORD-12345
            _logger.LogInformation("Gift-i-Card: Retrieving order and codes for tracking code {TrackingCode}", reference.TrackingCode);

            var endpoint = $"/giftcard/retrieve?tracking_code={Uri.EscapeDataString(reference.TrackingCode)}";
            var httpResponse = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                return new SupplierOrderResult(false, reference.TrackingCode, "failed", [], err);
            }

            var result = await httpResponse.Content.ReadFromJsonAsync<GifticardRetrieveResponse>(cancellationToken: cancellationToken);
            if (result == null)
            {
                return new SupplierOrderResult(false, reference.TrackingCode, "failed", [], "Null response from retrieve endpoint");
            }

            var cards = result.Cards.Select(c => new PurchasedCardItem(
                Code: c.Code,
                Pin: c.Pin,
                SerialNumber: c.SerialNumber,
                ExpirationDate: c.ExpireDate
            )).ToList();

            return new SupplierOrderResult(
                IsSuccess: string.Equals(result.Status, "completed", StringComparison.OrdinalIgnoreCase) && cards.Count > 0,
                TrackingCode: reference.TrackingCode,
                Status: result.Status,
                Cards: cards,
                ErrorMessage: result.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gift-i-Card: Exception during RetrieveOrderAsync");
            return new SupplierOrderResult(false, reference.TrackingCode, "failed", [], ex.Message);
        }
    }

    private static CardBrand MapBrandFromTitle(string title)
    {
        var lower = title.ToLowerInvariant();
        if (lower.Contains("apple") || lower.Contains("itunes")) return CardBrand.Apple;
        if (lower.Contains("steam")) return CardBrand.Steam;
        if (lower.Contains("google")) return CardBrand.GooglePlay;
        if (lower.Contains("playstation") || lower.Contains("psn")) return CardBrand.PlayStation;
        if (lower.Contains("xbox")) return CardBrand.Xbox;
        if (lower.Contains("amazon")) return CardBrand.Amazon;
        if (lower.Contains("spotify")) return CardBrand.Spotify;
        if (lower.Contains("netflix")) return CardBrand.Netflix;
        if (lower.Contains("razer")) return CardBrand.RazerGold;
        return CardBrand.Other;
    }
}
