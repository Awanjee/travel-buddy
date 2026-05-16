using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class ItineraryService
{
    private readonly TravelBuddyDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ItineraryService(TravelBuddyDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ItineraryResponse?> BuildAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips
            .Include(t => t.Candidates)
            .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId, ct);
        if (trip is null) return null;

        var approved = await _db.TripDecisions.AsNoTracking()
            .Where(d => d.TripId == tripId && d.Decision == DecisionType.Approved)
            .Join(_db.TripCandidates.AsNoTracking(),
                d => d.CandidateId,
                c => c.Id,
                (d, c) => c)
            .ToListAsync(ct);

        if (approved.Count == 0)
            return null;

        var days = EstimateDays(trip);
        var markdown = BuildMarkdown(trip, approved, days);
        var versionNumber = await _db.ItineraryVersions.CountAsync(v => v.TripId == tripId, ct) + 1;

        var exportDir = Path.Combine(_env.ContentRootPath, "exports");
        Directory.CreateDirectory(exportDir);
        var pdfFileName = $"trip-{tripId}-v{versionNumber}.pdf";
        var pdfPath = Path.Combine(exportDir, pdfFileName);
        GeneratePdf(pdfPath, trip, approved, days, markdown);

        var version = new ItineraryVersion
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            VersionNumber = versionNumber,
            PlanMarkdown = markdown,
            ExportPdfPath = pdfFileName,
            CreatedAt = DateTime.UtcNow
        };
        _db.ItineraryVersions.Add(version);
        trip.Status = "plan_ready";
        trip.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new ItineraryResponse(
            version.Id,
            version.VersionNumber,
            version.PlanMarkdown,
            $"/api/trips/{tripId}/itinerary/{version.Id}/pdf",
            version.CreatedAt);
    }

    public async Task<ItineraryResponse?> GetLatestAsync(Guid userId, Guid tripId, CancellationToken ct)
    {
        var trip = await _db.Trips.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId, ct);
        if (trip is null) return null;

        var version = await _db.ItineraryVersions.AsNoTracking()
            .Where(v => v.TripId == tripId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);
        if (version is null) return null;

        return new ItineraryResponse(
            version.Id,
            version.VersionNumber,
            version.PlanMarkdown,
            version.ExportPdfPath is null ? null : $"/api/trips/{tripId}/itinerary/{version.Id}/pdf",
            version.CreatedAt);
    }

    public async Task<(byte[] Bytes, string FileName)?> GetPdfAsync(
        Guid userId, Guid tripId, Guid versionId, CancellationToken ct)
    {
        var version = await _db.ItineraryVersions.AsNoTracking()
            .Include(v => v.Trip)
            .FirstOrDefaultAsync(v => v.Id == versionId && v.TripId == tripId && v.Trip.UserId == userId, ct);
        if (version?.ExportPdfPath is null) return null;

        var path = Path.Combine(_env.ContentRootPath, "exports", version.ExportPdfPath);
        if (!File.Exists(path)) return null;
        return (await File.ReadAllBytesAsync(path, ct), version.ExportPdfPath);
    }

    private static int EstimateDays(Trip trip)
    {
        if (trip.StartDate.HasValue && trip.EndDate.HasValue)
            return Math.Max(1, trip.EndDate.Value.DayNumber - trip.StartDate.Value.DayNumber + 1);
        return 5;
    }

    private static string BuildMarkdown(Trip trip, List<TripCandidate> approved, int days)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {trip.DestinationCountryName} trip plan");
        sb.AppendLine();
        sb.AppendLine($"- **Party size:** {trip.PartySize}");
        sb.AppendLine($"- **Budget:** {trip.BudgetBand}");
        sb.AppendLine($"- **Energy:** {trip.EnergyLevel}");
        sb.AppendLine($"- **Duration:** {days} day(s)");
        sb.AppendLine();
        sb.AppendLine("## Approved places & stays");
        foreach (var group in approved.GroupBy(c => c.Type))
        {
            sb.AppendLine($"### {group.Key}");
            foreach (var item in group)
            {
                sb.AppendLine($"- **{item.Name}** ({item.Tag}) — {item.Description}");
                if (item.Location is not null) sb.AppendLine($"  - Location: {item.Location}");
            }
            sb.AppendLine();
        }
        sb.AppendLine("## Suggested day flow");
        var perDay = Math.Max(1, approved.Count / days);
        for (var d = 1; d <= days; d++)
        {
            sb.AppendLine($"### Day {d}");
            var slice = approved.Skip((d - 1) * perDay).Take(perDay);
            foreach (var item in slice)
                sb.AppendLine($"- {item.Name} ({item.Type})");
            if (!slice.Any())
                sb.AppendLine("- Free exploration / rest");
            sb.AppendLine();
        }
        sb.AppendLine("---");
        sb.AppendLine("*Generated by Travel Buddy. Verify visa, bookings, and opening hours before you travel.*");
        return sb.ToString();
    }

    private static void GeneratePdf(string path, Trip trip, List<TripCandidate> approved, int days, string markdown)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text($"{trip.DestinationCountryName} — Travel Buddy Plan").Bold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Item().Text($"Party: {trip.PartySize} | Budget: {trip.BudgetBand} | {days} days").FontSize(11);
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                    foreach (var item in approved)
                    {
                        col.Item().Text($"{item.Type}: {item.Name}").Bold();
                        col.Item().Text(item.Description).FontSize(10);
                        col.Item().PaddingBottom(8);
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf(path);
    }
}
