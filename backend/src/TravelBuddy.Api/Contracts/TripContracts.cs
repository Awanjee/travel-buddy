namespace TravelBuddy.Api.Contracts;

public record CreateTripRequest(
    string DestinationCountryCode,
    string DestinationCountryName,
    string? StartDate,
    string? EndDate,
    int PartySize,
    string BudgetBand,
    string EnergyLevel,
    string VisaIntent,
    List<string> Preferences,
    string? PersonalNotes);

public record UpdateTripRequest(
    string? StartDate,
    string? EndDate,
    int? PartySize,
    string? BudgetBand,
    string? EnergyLevel,
    string? VisaIntent,
    List<string>? Preferences,
    string? PersonalNotes);

public record TripResponse(
    Guid Id,
    string DestinationCountryCode,
    string DestinationCountryName,
    string? StartDate,
    string? EndDate,
    int PartySize,
    string BudgetBand,
    string EnergyLevel,
    string VisaIntent,
    List<string> Preferences,
    string? PersonalNotes,
    string Status,
    DateTime CreatedAt);
