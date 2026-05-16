using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class TripService
{
    private readonly TravelBuddyDbContext _db;
    private readonly VisaService _visa;
    private readonly CandidateGeneratorService _candidates;

    public TripService(TravelBuddyDbContext db, VisaService visa, CandidateGeneratorService candidates)
    {
        _db = db;
        _visa = visa;
        _candidates = candidates;
    }

    public async Task<TripResponse> CreateAsync(Guid userId, CreateTripRequest request, CancellationToken ct)
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DestinationCountryCode = request.DestinationCountryCode.ToUpperInvariant(),
            DestinationCountryName = request.DestinationCountryName,
            StartDate = ParseDate(request.StartDate),
            EndDate = ParseDate(request.EndDate),
            PartySize = Math.Max(1, request.PartySize),
            BudgetBand = Enum.Parse<BudgetBand>(request.BudgetBand, true),
            EnergyLevel = Enum.Parse<EnergyLevel>(request.EnergyLevel, true),
            VisaIntent = Enum.Parse<VisaIntent>(request.VisaIntent, true),
            PreferencesJson = JsonSerializer.Serialize(request.Preferences),
            PersonalNotes = request.PersonalNotes,
            Status = "questionnaire_complete"
        };
        _db.Trips.Add(trip);
        await _db.SaveChangesAsync(ct);

        var profile = await _db.UserProfiles.AsNoTracking().FirstAsync(p => p.UserId == userId, ct);
        var passport = profile.PassportCountryCode ?? profile.HomeCountryCode ?? "PK";
        await _visa.AttachGuidanceAsync(trip, passport, ct);
        await _candidates.GenerateForTripAsync(trip.Id, ct);

        return ToResponse(trip);
    }

    public async Task<List<TripResponse>> ListAsync(Guid userId, CancellationToken ct)
    {
        var trips = await _db.Trips.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
        return trips.Select(ToResponse).ToList();
    }

    public async Task<TripResponse?> GetAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId, ct);
        return trip is null ? null : ToResponse(trip);
    }

    public async Task<TripResponse?> UpdateAsync(Guid userId, Guid tripId, UpdateTripRequest request, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId, ct);
        if (trip is null) return null;

        if (request.StartDate is not null) trip.StartDate = ParseDate(request.StartDate);
        if (request.EndDate is not null) trip.EndDate = ParseDate(request.EndDate);
        if (request.PartySize.HasValue) trip.PartySize = Math.Max(1, request.PartySize.Value);
        if (request.BudgetBand is not null) trip.BudgetBand = Enum.Parse<BudgetBand>(request.BudgetBand, true);
        if (request.EnergyLevel is not null) trip.EnergyLevel = Enum.Parse<EnergyLevel>(request.EnergyLevel, true);
        if (request.VisaIntent is not null) trip.VisaIntent = Enum.Parse<VisaIntent>(request.VisaIntent, true);
        if (request.Preferences is not null) trip.PreferencesJson = JsonSerializer.Serialize(request.Preferences);
        if (request.PersonalNotes is not null) trip.PersonalNotes = request.PersonalNotes;
        trip.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToResponse(trip);
    }

    private static DateOnly? ParseDate(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : DateOnly.Parse(value);

    public static TripResponse ToResponse(Trip trip) => new(
        trip.Id,
        trip.DestinationCountryCode,
        trip.DestinationCountryName,
        trip.StartDate?.ToString("yyyy-MM-dd"),
        trip.EndDate?.ToString("yyyy-MM-dd"),
        trip.PartySize,
        trip.BudgetBand.ToString(),
        trip.EnergyLevel.ToString(),
        trip.VisaIntent.ToString(),
        JsonSerializer.Deserialize<List<string>>(trip.PreferencesJson) ?? new List<string>(),
        trip.PersonalNotes,
        trip.Status,
        trip.CreatedAt);
}
