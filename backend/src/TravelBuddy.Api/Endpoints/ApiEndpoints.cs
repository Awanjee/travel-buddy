using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Extensions;
using TravelBuddy.Api.Services;

namespace TravelBuddy.Api.Endpoints;

public static class ApiEndpoints
{
    public static void MapTravelBuddyApi(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        var auth = app.MapGroup("/api/auth");
        auth.MapPost("/register", async (RegisterRequest req, AuthService svc, CancellationToken ct) =>
        {
            var result = await svc.RegisterAsync(req, ct);
            return result is null ? Results.Conflict(new { error = "Email already registered." }) : Results.Ok(result);
        });
        auth.MapPost("/login", async (LoginRequest req, AuthService svc, CancellationToken ct) =>
        {
            var result = await svc.LoginAsync(req, ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        });

        var api = app.MapGroup("/api").RequireAuthorization();

        api.MapGet("/profile", async (HttpContext ctx, AuthService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var profile = await svc.GetProfileAsync(userId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        });
        api.MapPut("/profile", async (UpdateProfileRequest req, HttpContext ctx, AuthService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var profile = await svc.UpdateProfileAsync(userId, req, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        });

        api.MapGet("/trips", async (HttpContext ctx, TripService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            return Results.Ok(await svc.ListAsync(userId, ct));
        });
        api.MapPost("/trips", async (CreateTripRequest req, HttpContext ctx, TripService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var trip = await svc.CreateAsync(userId, req, ct);
            return Results.Created($"/api/trips/{trip.Id}", trip);
        });
        api.MapGet("/trips/{tripId:guid}", async (Guid tripId, HttpContext ctx, TripService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var trip = await svc.GetAsync(userId, tripId, ct);
            return trip is null ? Results.NotFound() : Results.Ok(trip);
        });
        api.MapPut("/trips/{tripId:guid}", async (Guid tripId, UpdateTripRequest req, HttpContext ctx, TripService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var trip = await svc.UpdateAsync(userId, tripId, req, ct);
            return trip is null ? Results.NotFound() : Results.Ok(trip);
        });

        api.MapGet("/trips/{tripId:guid}/visa", async (Guid tripId, HttpContext ctx, VisaService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var visa = await svc.GetForTripAsync(userId, tripId, ct);
            return visa is null ? Results.NotFound() : Results.Ok(visa);
        });

        api.MapGet("/trips/{tripId:guid}/candidates/pending", async (Guid tripId, HttpContext ctx, CandidateService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            return Results.Ok(await svc.GetPendingAsync(userId, tripId, ct));
        });
        api.MapGet("/trips/{tripId:guid}/candidates", async (Guid tripId, HttpContext ctx, CandidateService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            return Results.Ok(await svc.GetAllAsync(userId, tripId, ct));
        });
        api.MapPost("/trips/{tripId:guid}/candidates/{candidateId:guid}/decision", async (
            Guid tripId, Guid candidateId, DecisionRequest req, HttpContext ctx, CandidateService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var result = await svc.DecideAsync(userId, tripId, candidateId, req, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        api.MapPost("/trips/{tripId:guid}/itinerary/build", async (Guid tripId, HttpContext ctx, ItineraryService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var plan = await svc.BuildAsync(userId, tripId, ct);
            return plan is null
                ? Results.BadRequest(new { error = "Approve at least one place, hotel, or activity first." })
                : Results.Ok(plan);
        });
        api.MapGet("/trips/{tripId:guid}/itinerary", async (Guid tripId, HttpContext ctx, ItineraryService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var plan = await svc.GetLatestAsync(userId, tripId, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });
        api.MapGet("/trips/{tripId:guid}/itinerary/{versionId:guid}/pdf", async (
            Guid tripId, Guid versionId, HttpContext ctx, ItineraryService svc, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId()!.Value;
            var pdf = await svc.GetPdfAsync(userId, tripId, versionId, ct);
            return pdf is null ? Results.NotFound() : Results.File(pdf.Value.Bytes, "application/pdf", pdf.Value.FileName);
        });
    }
}
