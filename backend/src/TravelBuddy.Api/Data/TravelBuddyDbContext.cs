using Microsoft.EntityFrameworkCore;
using TravelBuddy.Api.Entities;

namespace TravelBuddy.Api.Data;

public class TravelBuddyDbContext : DbContext
{
    public TravelBuddyDbContext(DbContextOptions<TravelBuddyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<VisaGuidance> VisaGuidances => Set<VisaGuidance>();
    public DbSet<VisaChecklistItem> VisaChecklistItems => Set<VisaChecklistItem>();
    public DbSet<TripCandidate> TripCandidates => Set<TripCandidate>();
    public DbSet<TripDecision> TripDecisions => Set<TripDecision>();
    public DbSet<ItineraryVersion> ItineraryVersions => Set<ItineraryVersion>();
    public DbSet<TravelerProfile> TravelerProfiles => Set<TravelerProfile>();
    public DbSet<ConnectionRequest> ConnectionRequests => Set<ConnectionRequest>();
    public DbSet<SwipeEvent> SwipeEvents => Set<SwipeEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasOne(x => x.User).WithOne(x => x.Profile).HasForeignKey<UserProfile>(x => x.UserId);
        });

        modelBuilder.Entity<Trip>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(x => x.Trips).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<VisaGuidance>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Trip).WithOne(x => x.VisaGuidance).HasForeignKey<VisaGuidance>(x => x.TripId);
        });

        modelBuilder.Entity<VisaChecklistItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.VisaGuidance).WithMany(x => x.ChecklistItems).HasForeignKey(x => x.VisaGuidanceId);
        });

        modelBuilder.Entity<TripCandidate>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Trip).WithMany(x => x.Candidates).HasForeignKey(x => x.TripId);
        });

        modelBuilder.Entity<TripDecision>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Trip).WithMany(x => x.Decisions).HasForeignKey(x => x.TripId);
            e.HasOne(x => x.Candidate).WithOne(x => x.Decision).HasForeignKey<TripDecision>(x => x.CandidateId);
            e.HasIndex(x => x.CandidateId).IsUnique();
        });

        modelBuilder.Entity<ItineraryVersion>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Trip).WithMany(x => x.ItineraryVersions).HasForeignKey(x => x.TripId);
        });

        modelBuilder.Entity<TravelerProfile>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasOne(x => x.User).WithOne(x => x.TravelerProfile).HasForeignKey<TravelerProfile>(x => x.UserId);
        });

        modelBuilder.Entity<ConnectionRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.FromUser).WithMany().HasForeignKey(x => x.FromUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToUser).WithMany().HasForeignKey(x => x.ToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SwipeEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TargetUser).WithMany().HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
