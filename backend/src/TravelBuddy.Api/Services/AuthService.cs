using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Contracts;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Services;

public class AuthService
{
    private readonly TravelBuddyDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthService(TravelBuddyDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        user.Profile = new UserProfile
        {
            UserId = user.Id,
            DisplayName = request.DisplayName.Trim(),
            HomeCountryCode = request.HomeCountryCode?.ToUpperInvariant()
        };
        user.TravelerProfile = new TravelerProfile { UserId = user.Id, IsDiscoverable = false };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(_jwt.CreateToken(user.Id, user.Email), user.Id, user.Email, user.Profile.DisplayName);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return new AuthResponse(
            _jwt.CreateToken(user.Id, user.Email),
            user.Id,
            user.Email,
            user.Profile?.DisplayName ?? user.Email);
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user?.Profile is null) return null;
        var p = user.Profile;
        return new ProfileResponse(user.Id, user.Email, p.DisplayName, p.HomeCountryCode, p.PassportCountryCode, p.Bio);
    }

    public async Task<ProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null) return null;

        profile.DisplayName = request.DisplayName.Trim();
        profile.HomeCountryCode = request.HomeCountryCode?.ToUpperInvariant();
        profile.PassportCountryCode = request.PassportCountryCode?.ToUpperInvariant();
        profile.Bio = request.Bio;
        await _db.SaveChangesAsync(ct);

        var user = await _db.Users.FindAsync([userId], ct);
        return new ProfileResponse(userId, user!.Email, profile.DisplayName, profile.HomeCountryCode, profile.PassportCountryCode, profile.Bio);
    }
}
