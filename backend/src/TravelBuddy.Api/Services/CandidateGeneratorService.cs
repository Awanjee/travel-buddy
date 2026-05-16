using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class CandidateGeneratorService
{
    private readonly TravelBuddyDbContext _db;
    private readonly BookingLinkService _booking;

    public CandidateGeneratorService(TravelBuddyDbContext db, BookingLinkService booking)
    {
        _db = db;
        _booking = booking;
    }

    public async Task<int> GenerateForTripAsync(Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct);
        if (trip is null) return 0;

        var existing = await _db.TripCandidates.AnyAsync(c => c.TripId == tripId, ct);
        if (existing) return 0;

        var prefs = JsonSerializer.Deserialize<List<string>>(trip.PreferencesJson) ?? new List<string>();
        var pool = DestinationContent.GetCandidates(trip.DestinationCountryCode, trip.BudgetBand, prefs, trip.EnergyLevel);

        foreach (var item in pool)
        {
            var query = item.Type == CandidateType.Hotel
                ? $"{item.Name} {trip.DestinationCountryName} hotel"
                : $"{item.Name} {trip.DestinationCountryName}";
            _db.TripCandidates.Add(new TripCandidate
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Type = item.Type,
                Tag = item.Tag,
                Name = item.Name,
                Description = item.Description,
                Location = item.Location,
                PriceEstimateUsd = item.Price,
                Score = item.Score,
                BookingSearchQuery = query,
                ExternalUrl = _booking.BuildSearchUrl(item.Type, query, trip.DestinationCountryName)
            });
        }

        trip.Status = "discovery";
        trip.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return pool.Count;
    }
}

public record CandidateSeed(
    CandidateType Type,
    CandidateTag Tag,
    string Name,
    string Description,
    string? Location,
    decimal? Price,
    int Score);

public static class DestinationContent
{
    public static List<CandidateSeed> GetCandidates(
        string countryCode,
        BudgetBand budget,
        List<string> preferences,
        EnergyLevel energy)
    {
        var baseList = countryCode.ToUpperInvariant() switch
        {
            "JP" => JapanContent(),
            "TR" => TurkeyContent(),
            _ => GenericContent(countryCode)
        };

        var luxury = budget == BudgetBand.Luxury;
        var budgetPref = budget == BudgetBand.Budget;
        return baseList
            .Where(c => !budgetPref || c.Price is null or <= 80)
            .Where(c => !luxury || c.Price is null or >= 40)
            .Where(c => energy != EnergyLevel.Low || c.Score <= 90)
            .OrderByDescending(c => ScorePreference(c, preferences))
            .ThenByDescending(c => c.Score)
            .ToList();
    }

    private static int ScorePreference(CandidateSeed c, List<string> prefs)
    {
        var score = 0;
        foreach (var p in prefs.Select(x => x.ToLowerInvariant()))
        {
            if (p.Contains("architecture") && c.Description.Contains("temple", StringComparison.OrdinalIgnoreCase)) score += 5;
            if (p.Contains("history") && c.Tag is CandidateTag.MustSee or CandidateTag.Popular) score += 3;
            if (p.Contains("night") && c.Description.Contains("night", StringComparison.OrdinalIgnoreCase)) score += 5;
            if (p.Contains("country") && c.Description.Contains("countryside", StringComparison.OrdinalIgnoreCase)) score += 5;
            if (p.Contains("food") && c.Type == CandidateType.Activity) score += 2;
        }
        return score;
    }

    private static List<CandidateSeed> JapanContent() => new()
    {
        new(CandidateType.Place, CandidateTag.MustSee, "Senso-ji Temple", "Tokyo's oldest temple in Asakusa — iconic architecture and street food nearby.", "Tokyo", null, 95),
        new(CandidateType.Place, CandidateTag.HiddenGem, "Yanaka Ginza", "Showa-era shopping street with local snacks — quieter than major districts.", "Tokyo", null, 88),
        new(CandidateType.Place, CandidateTag.OftenMissed, "Fushimi Inari early morning", "Thousands of torii gates — arrive before 7am to avoid crowds.", "Kyoto", null, 92),
        new(CandidateType.Hotel, CandidateTag.Popular, "Hotel Gracery Shinjuku", "Central Shinjuku base near transport and nightlife.", "Tokyo", 120m, 85),
        new(CandidateType.Hotel, CandidateTag.HiddenGem, "Piece Hostel Sanjo", "Design-forward hostel/hotel hybrid in Kyoto's core.", "Kyoto", 45m, 82),
        new(CandidateType.Activity, CandidateTag.Popular, "TeamLab Borderless", "Immersive digital art — book tickets weeks ahead.", "Tokyo", 35m, 90),
        new(CandidateType.Activity, CandidateTag.OftenMissed, "Nishiki Market food walk", "Kyoto's kitchen — sample pickles, mochi, and grilled seafood.", "Kyoto", 25m, 87),
        new(CandidateType.Activity, CandidateTag.HiddenGem, "Omoide Yokocho izakaya crawl", "Tiny alley bars in Shinjuku for local atmosphere.", "Tokyo", 40m, 86),
    };

    private static List<CandidateSeed> TurkeyContent() => new()
    {
        new(CandidateType.Place, CandidateTag.MustSee, "Hagia Sophia", "Byzantine masterpiece at the heart of historic Istanbul.", "Istanbul", null, 96),
        new(CandidateType.Place, CandidateTag.HiddenGem, "Balat colorful streets", "Photogenic neighborhood cafes and vintage shops.", "Istanbul", null, 84),
        new(CandidateType.Place, CandidateTag.OftenMissed, "Chora Church mosaics", "Stunning Byzantine art — less crowded than major sites.", "Istanbul", null, 88),
        new(CandidateType.Hotel, CandidateTag.Popular, "Sultanahmet Palace Hotel", "Walkable to Blue Mosque and Grand Bazaar.", "Istanbul", 90m, 83),
        new(CandidateType.Hotel, CandidateTag.HiddenGem, "Karakoy Rooms", "Boutique stay near Galata Bridge and ferries.", "Istanbul", 70m, 81),
        new(CandidateType.Activity, CandidateTag.Popular, "Bosphorus sunset cruise", "See Europe and Asia from the water.", "Istanbul", 30m, 89),
        new(CandidateType.Activity, CandidateTag.OftenMissed, "Turkish breakfast in Kadikoy", "Asian-side food scene locals love.", "Istanbul", 20m, 85),
        new(CandidateType.Activity, CandidateTag.HiddenGem, "Cappadocia balloon (day trip)", "Book reputable operator; often missed if only staying in Istanbul.", "Cappadocia", 180m, 80),
    };

    private static List<CandidateSeed> GenericContent(string code) => new()
    {
        new(CandidateType.Place, CandidateTag.MustSee, $"Capital highlights — {code}", "Explore the main historic district and central landmarks.", "Capital", null, 80),
        new(CandidateType.Hotel, CandidateTag.Popular, "City center hotel", "Convenient base near transit.", "Capital", 75m, 75),
        new(CandidateType.Activity, CandidateTag.Popular, "Guided walking tour", "Orientation tour to learn local context.", "Capital", 30m, 78),
    };
}
