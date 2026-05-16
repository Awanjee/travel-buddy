namespace TravelBuddy.Api.Contracts;

public record RegisterRequest(string Email, string Password, string DisplayName, string? HomeCountryCode);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, Guid UserId, string Email, string DisplayName);
public record UpdateProfileRequest(string DisplayName, string? HomeCountryCode, string? PassportCountryCode, string? Bio);
public record ProfileResponse(Guid UserId, string Email, string DisplayName, string? HomeCountryCode, string? PassportCountryCode, string? Bio);
