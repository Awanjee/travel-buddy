namespace TravelBuddy.Api.Entities;

public class VisaGuidance
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string PassportCountryCode { get; set; } = string.Empty;
    public string DestinationCountryCode { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public int TimelineMinDays { get; set; }
    public int TimelineMaxDays { get; set; }
    public string TimelineNotes { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime LastVerifiedAt { get; set; }

    public Trip Trip { get; set; } = null!;
    public ICollection<VisaChecklistItem> ChecklistItems { get; set; } = new List<VisaChecklistItem>();
}

public class VisaChecklistItem
{
    public Guid Id { get; set; }
    public Guid VisaGuidanceId { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;

    public VisaGuidance VisaGuidance { get; set; } = null!;
}
