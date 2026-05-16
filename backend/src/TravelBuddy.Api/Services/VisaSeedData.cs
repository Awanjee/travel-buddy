namespace TravelBuddy.Api.Services;

public record VisaTemplate(
    string Summary,
    int TimelineMinDays,
    int TimelineMaxDays,
    string TimelineNotes,
    string SourceUrl,
    DateTime LastVerifiedAt,
    List<(string Title, string Description, bool IsRequired)> Checklist);

public static class VisaSeedData
{
    public static VisaTemplate? GetTemplate(string passportCode, string destinationCode) =>
        (passportCode.ToUpperInvariant(), destinationCode.ToUpperInvariant()) switch
        {
            ("PK", "JP") => PakistanToJapan(),
            ("PK", "TR") => PakistanToTurkey(),
            _ => null
        };

    public static VisaTemplate GetGenericTemplate(string passportCode, string destinationCode) => new(
        $"Visa requirements for {passportCode} passport holders traveling to {destinationCode} vary. " +
        "Use official government sources to confirm whether you need a visa, e-visa, or visa on arrival.",
        14,
        45,
        "Processing times are estimates and depend on embassy workload, completeness of your application, and peak travel seasons.",
        $"https://www.google.com/search?q={passportCode}+passport+visa+{destinationCode}",
        DateTime.UtcNow.Date,
        new List<(string, string, bool)>
        {
            ("Check official requirements", "Visit the destination country's embassy or immigration website.", true),
            ("Valid passport", "Typically 6+ months validity beyond your return date with blank pages.", true),
            ("Proof of funds", "Bank statements or sponsorship letter as required.", true),
            ("Travel itinerary", "Flight and accommodation details (can be tentative for some visas).", false),
        });

    private static VisaTemplate PakistanToJapan() => new(
        "Pakistan passport holders generally need a visa to visit Japan for tourism. " +
        "Applications are typically submitted through the Japanese embassy or an accredited visa service center.",
        10,
        21,
        "Allow extra time during peak seasons (spring cherry blossom, year-end holidays). " +
        "Incomplete documents are the most common cause of delays.",
        "https://www.mofa.go.jp/j_info/visit/visa/",
        new DateTime(2026, 1, 15),
        new List<(string, string, bool)>
        {
            ("Passport", "Valid for the duration of stay; at least two blank visa pages.", true),
            ("Visa application form", "Completed and signed; use the form specified by the embassy.", true),
            ("Photo", "Passport-size photo meeting Japan visa specifications.", true),
            ("Flight itinerary", "Round-trip or onward ticket reservation.", true),
            ("Hotel bookings", "Confirmed accommodation for your stay.", true),
            ("Bank statements", "Proof of sufficient funds for the trip (recent 3–6 months).", true),
            ("Employment letter", "Letter from employer stating position, salary, and leave approval.", true),
            ("Travel plan", "Day-by-day itinerary describing activities and cities.", false),
        });

    private static VisaTemplate PakistanToTurkey() => new(
        "Pakistan passport holders can often apply for a Turkish e-Visa for tourism, subject to eligibility rules on the official portal. " +
        "If e-Visa is not available, apply via the Turkish embassy.",
        1,
        7,
        "E-visa approvals are often same-day but can take up to 72 hours. Embassy visas may take 1–2 weeks.",
        "https://www.evisa.gov.tr/",
        new DateTime(2026, 2, 1),
        new List<(string, string, bool)>
        {
            ("Check e-Visa eligibility", "Confirm on the official Republic of Türkiye e-Visa website.", true),
            ("Passport", "Valid at least 150 days beyond entry date (check current rules).", true),
            ("Payment card", "For e-Visa application fee online.", true),
            ("Supporting documents", "Return ticket and hotel booking if requested during application.", true),
            ("Travel insurance", "Recommended; may be required for longer stays.", false),
        });
}
