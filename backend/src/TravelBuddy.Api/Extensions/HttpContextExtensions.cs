using System.Security.Claims;

namespace TravelBuddy.Api.Extensions;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var userId) ? userId : null;
    }
}
