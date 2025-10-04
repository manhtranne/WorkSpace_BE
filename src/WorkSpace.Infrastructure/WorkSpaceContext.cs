using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Domain.Entities;
using WorkSpace.Domain.SeedWorks.Constants;
namespace WorkSpace.Infrastructure;

public class WorkSpaceContext : IdentityDbContext<AppUser, AppRole, int>
{
    public WorkSpaceContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<WorkSpaces> Workspaces { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<HostProfile> HostProfiles { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<WorkspaceType> WorkspaceTypes { get; set; }
    public DbSet<WorkspaceImage> WorkspaceImages { get; set; }
    public DbSet<WorkspaceAmenity> WorkspaceAmenities { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<BookingStatus> BookingStatuses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<BookingParticipant> BookingParticipants { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<PromotionUsage> PromotionUsages { get; set; }
    public DbSet<AvailabilitySchedule> AvailabilitySchedules { get; set; }
    public DbSet<BlockedTimeSlot> BlockedTimeSlots { get; set; }
    public DbSet<WorkSpaceFavorite> WorkSpaceFavorites { get; set; }
    public DbSet<Post> Posts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

            // WorkspaceAmenity (N-N)
            modelBuilder.Entity<WorkspaceAmenity>()
                .HasKey(wa => new { wa.WorkspaceId, wa.AmenityId });
            modelBuilder.Entity<WorkspaceAmenity>()
                .HasOne(wa => wa.Workspace)
                .WithMany(w => w.WorkspaceAmenities)
                .HasForeignKey(wa => wa.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkspaceAmenity>()
                .HasOne(wa => wa.Amenity)
                .WithMany(a => a.WorkspaceAmenities)
                .HasForeignKey(wa => wa.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkspaceImage (1-N)
            modelBuilder.Entity<WorkspaceImage>()
                .HasOne(wi => wi.Workspace)
                .WithMany(w => w.WorkspaceImages)
                .HasForeignKey(wi => wi.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkSpaceFavorite (N-N)
            modelBuilder.Entity<WorkSpaceFavorite>()
                .HasKey(wf => new { wf.WorkspaceId, wf.UserId });
            modelBuilder.Entity<WorkSpaceFavorite>()
                .HasOne(wf => wf.Workspace)
                .WithMany(w => w.WorkSpaceFavorites)
                .HasForeignKey(wf => wf.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkSpaceFavorite>()
                .HasOne(wf => wf.User)
                .WithMany(u => u.WorkSpaceFavorites)
                .HasForeignKey(wf => wf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Address (1-N)
            modelBuilder.Entity<Address>()
                .HasMany(a => a.Workspaces)
                .WithOne(w => w.Address)
                .HasForeignKey(w => w.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkspaceType (1-N)
            modelBuilder.Entity<WorkspaceType>()
                .HasMany(wt => wt.Workspaces)
                .WithOne(w => w.WorkspaceType)
                .HasForeignKey(w => w.WorkspaceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // HostProfile (1-N) & (1-1)
            modelBuilder.Entity<HostProfile>()
                .HasMany(h => h.Workspaces)
                .WithOne(w => w.Host)
                .HasForeignKey(w => w.HostId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HostProfile>()
                .HasOne(h => h.User)
                .WithOne(u => u.HostProfile)
                .HasForeignKey<HostProfile>(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Workspace)
                .WithMany(w => w.Bookings)
                .HasForeignKey(b => b.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.BookingStatus)
                .WithMany(bs => bs.Bookings)
                .HasForeignKey(b => b.BookingStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();
            modelBuilder.Entity<Booking>()
                .HasMany(b => b.BookingParticipants)
                .WithOne(bp => bp.Booking)
                .HasForeignKey(bp => bp.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Workspace)
                .WithMany(w => w.Reviews)
                .HasForeignKey(r => r.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasIndex(r => r.BookingId)
                .IsUnique();

            // Payment (1-1 với Booking)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // PromotionUsage
            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Promotion)
                .WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Booking)
                .WithMany(b => b.PromotionUsages)
                .HasForeignKey(pu => pu.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.PromotionUsages)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AvailabilitySchedule
            modelBuilder.Entity<AvailabilitySchedule>()
                .HasOne(a => a.Workspace)
                .WithMany(w => w.AvailabilitySchedules)
                .HasForeignKey(a => a.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlockedTimeSlot
            modelBuilder.Entity<BlockedTimeSlot>()
                .HasOne(b => b.Workspace)
                .WithMany(w => w.BlockedTimeSlots)
                .HasForeignKey(b => b.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AppUserClaims").HasKey(x => x.Id);

            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AppRoleClaims")
                .HasKey(x => x.Id);

            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AppUserLogins").HasKey(x => x.UserId);

            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AppUserRoles")
                .HasKey(x => new { x.RoleId, x.UserId });

            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AppUserTokens")
                .HasKey(x => new { x.UserId });

    }
}