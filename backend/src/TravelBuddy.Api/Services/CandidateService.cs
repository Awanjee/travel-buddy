using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class CandidateService
{
    private readonly TravelBuddyDbContext _db;
    private readonly BookingLinkService _booking;

    public CandidateService(TravelBuddyDbContext db, BookingLinkService booking)
    {
        _db = db;
        _booking = booking;
    }

    public async Task<List<CandidateResponse>> GetPendingAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        if (!await OwnsTripAsync(userId, tripId, ct)) return new List<CandidateResponse>();

        var trip = await _db.Trips.AsNoTracking().FirstAsync(t => t.Id == tripId, ct);
        var decidedIds = await _db.TripDecisions.AsNoTracking()
            .Where(d => d.TripId == tripId)
            .Select(d => d.CandidateId)
            .ToListAsync(ct);

        var candidates = await _db.TripCandidates.AsNoTracking()
            .Where(c => c.TripId == tripId && !decidedIds.Contains(c.Id))
            .OrderByDescending(c => c.Score)
            .ToListAsync(ct);

        return candidates.Select(c => ToResponse(c, trip.DestinationCountryName, null)).ToList();
    }

    public async Task<List<CandidateResponse>> GetAllAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        if (!await OwnsTripAsync(userId, tripId, ct)) return new List<CandidateResponse>();

        var trip = await _db.Trips.AsNoTracking().FirstAsync(t => t.Id == tripId, ct);
        var decisions = await _db.TripDecisions.AsNoTracking()
            .Where(d => d.TripId == tripId)
            .ToDictionaryAsync(d => d.CandidateId, d => d.Decision.ToString(), ct);

        var candidates = await _db.TripCandidates.AsNoTracking()
            .Where(c => c.TripId == tripId)
            .OrderByDescending(c => c.Score)
            .ToListAsync(ct);

        return candidates.Select(c =>
            ToResponse(c, trip.DestinationCountryName, decisions.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<CandidateResponse?> DecideAsync(
        Guid userId, Guid tripId, Guid candidateId, DecisionRequest request, CancellationToken ct)
    {
        if (!await OwnsTripAsync(userId, tripId, ct)) return null;

        var decision = Enum.Parse<DecisionType>(request.Decision, true);
        var candidate = await _db.TripCandidates
            .Include(c => c.Trip)
            .FirstOrDefaultAsync(c => c.Id == candidateId && c.TripId == tripId, ct);
        if (candidate is null) return null;

        var existing = await _db.TripDecisions.FirstOrDefaultAsync(d => d.CandidateId == candidateId, ct);
        if (existing is not null)
        {
            existing.Decision = decision;
            existing.DecidedAt = DateTime.UtcNow;
        }
        else
        {
            _db.TripDecisions.Add(new TripDecision
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                CandidateId = candidateId,
                Decision = decision,
                DecidedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
        return ToResponse(candidate, candidate.Trip.DestinationCountryName, decision.ToString());
    }

    private CandidateResponse ToResponse(TripCandidate c, string country, string? decision)
    {
        var link = _booking.ForCandidate(c, country);
        return new CandidateResponse(
            c.Id,
            c.Type.ToString(),
            c.Tag.ToString(),
            c.Name,
            c.Description,
            c.Location,
            c.ImageUrl,
            c.PriceEstimateUsd,
            c.Score,
            link.Url,
            decision);
    }

    private Task<bool> OwnsTripAsync(Guid userId, Guid tripId, CancellationToken ct) =>
        _db.Trips.AnyAsync(t => t.Id == tripId && t.UserId == userId, ct);
}
