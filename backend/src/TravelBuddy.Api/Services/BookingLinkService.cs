using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class BookingLinkService
{
    public string BuildSearchUrl(CandidateType type, string query, string countryName)
    {
        var q = Uri.EscapeDataString($"{query} {countryName}");
        return type switch
        {
            CandidateType.Hotel => $"https://www.google.com/travel/hotels?q={q}",
            CandidateType.Activity => $"https://www.google.com/search?q={q}+tours",
            _ => $"https://www.google.com/maps/search/{q}"
        };
    }

    public BookingLinkResponse ForCandidate(TripCandidate c, string countryName)
    {
        var query = c.BookingSearchQuery ?? c.Name;
        var url = c.ExternalUrl ?? BuildSearchUrl(c.Type, query, countryName);
        var label = c.Type switch
        {
            CandidateType.Hotel => "Search hotels",
            CandidateType.Activity => "Find activities",
            _ => "View on map"
        };
        return new BookingLinkResponse(label, url);
    }
}
