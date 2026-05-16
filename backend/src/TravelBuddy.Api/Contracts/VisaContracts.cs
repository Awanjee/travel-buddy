namespace TravelBuddy.Api.Contracts;

public record VisaChecklistItemDto(int SortOrder, string Title, string Description, bool IsRequired);
public record VisaGuidanceResponse(
    Guid Id,
    string Summary,
    string Disclaimer,
    int TimelineMinDays,
    int TimelineMaxDays,
    string TimelineNotes,
    string SourceUrl,
    DateTime LastVerifiedAt,
    List<VisaChecklistItemDto> Checklist);
