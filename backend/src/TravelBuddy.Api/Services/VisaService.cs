using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class VisaService
{
    private const string Disclaimer =
        "This information is for planning purposes only and is not legal advice. " +
        "Requirements change frequently. Always verify with the official embassy or consulate before applying.";

    private readonly TravelBuddyDbContext _db;

    public VisaService(TravelBuddyDbContext db) => _db = db;

    public async Task<VisaGuidanceResponse?> GetForTripAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId, ct);
        if (trip is null) return null;

        var guidance = await _db.VisaGuidances.AsNoTracking()
            .Include(v => v.ChecklistItems.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(v => v.TripId == tripId, ct);
        if (guidance is null) return null;

        return ToResponse(guidance);
    }

    public async Task AttachGuidanceAsync(Trip trip, string passportCountryCode, CancellationToken ct)
    {
        var template = VisaSeedData.GetTemplate(passportCountryCode, trip.DestinationCountryCode);
        if (template is null)
        {
            template = VisaSeedData.GetGenericTemplate(passportCountryCode, trip.DestinationCountryCode);
        }

        var guidance = new VisaGuidance
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            PassportCountryCode = passportCountryCode,
            DestinationCountryCode = trip.DestinationCountryCode,
            Summary = template.Summary,
            Disclaimer = Disclaimer,
            TimelineMinDays = template.TimelineMinDays,
            TimelineMaxDays = template.TimelineMaxDays,
            TimelineNotes = template.TimelineNotes,
            SourceUrl = template.SourceUrl,
            LastVerifiedAt = template.LastVerifiedAt
        };

        var order = 0;
        foreach (var item in template.Checklist)
        {
            guidance.ChecklistItems.Add(new VisaChecklistItem
            {
                Id = Guid.NewGuid(),
                VisaGuidanceId = guidance.Id,
                SortOrder = order++,
                Title = item.Title,
                Description = item.Description,
                IsRequired = item.IsRequired
            });
        }

        _db.VisaGuidances.Add(guidance);
        await _db.SaveChangesAsync(ct);
    }

    private static VisaGuidanceResponse ToResponse(VisaGuidance g) => new(
        g.Id,
        g.Summary,
        g.Disclaimer,
        g.TimelineMinDays,
        g.TimelineMaxDays,
        g.TimelineNotes,
        g.SourceUrl,
        g.LastVerifiedAt,
        g.ChecklistItems.Select(c => new VisaChecklistItemDto(c.SortOrder, c.Title, c.Description, c.IsRequired)).ToList());
}
