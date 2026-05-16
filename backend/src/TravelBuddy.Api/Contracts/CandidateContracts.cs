namespace TravelBuddy.Api.Contracts;

public record CandidateResponse(
    Guid Id,
    string Type,
    string Tag,
    string Name,
    string Description,
    string? Location,
    string? ImageUrl,
    decimal? PriceEstimateUsd,
    int Score,
    string? BookingUrl,
    string? Decision);

public record DecisionRequest(string Decision);
public record GenerateCandidatesResponse(int GeneratedCount);
public record ItineraryResponse(Guid Id, int VersionNumber, string PlanMarkdown, string? ExportPdfUrl, DateTime CreatedAt);
public record BookingLinkResponse(string Label, string Url);
