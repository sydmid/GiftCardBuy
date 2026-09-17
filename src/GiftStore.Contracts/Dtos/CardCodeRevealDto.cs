namespace GiftStore.Contracts.Dtos;

public record CardCodeRevealDto(
    Guid ItemId,
    string DecryptedCode,
    string? DecryptedPin,
    string? SerialNumber,
    string? ExpirationDate
);
