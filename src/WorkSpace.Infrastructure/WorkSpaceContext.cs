﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure
{
    public class WorkSpaceContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public WorkSpaceContext(DbContextOptions<WorkSpaceContext> options) : base(options)
        {
        }

        public DbSet<WorkSpace.Domain.Entities.WorkSpace> Workspaces { get; set; } // << ĐÃ SỬA
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<HostProfile> HostProfiles { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Address> Addresses { get; set; }
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


        public DbSet<WorkSpaceRoom> WorkSpaceRooms { get; set; }
        public DbSet<WorkSpaceRoomType> WorkSpaceRoomTypes { get; set; }
        public DbSet<WorkSpaceRoomImage> WorkSpaceRoomImages { get; set; }
        public DbSet<WorkSpaceRoomAmenity> WorkSpaceRoomAmenities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region New Schema Configuration

            modelBuilder.Entity<WorkSpace.Domain.Entities.WorkSpace>() 
                .HasMany(w => w.WorkSpaceRooms)
                .WithOne(wr => wr.WorkSpace)
                .HasForeignKey(wr => wr.WorkSpaceId)
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<WorkSpaceRoomType>()
                .HasMany(wrt => wrt.WorkSpaceRooms)
                .WithOne(wr => wr.WorkSpaceRoomType)
                .HasForeignKey(wr => wr.WorkSpaceRoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkSpaceRoomAmenity>()
                    .HasKey(wra => new { wra.WorkspaceId, wra.AmenityId });

            modelBuilder.Entity<WorkSpaceRoomAmenity>()
                .HasOne(wra => wra.WorkSpaceRoom)
                .WithMany(wr => wr.WorkSpaceRoomAmenities)
                .HasForeignKey(wra => wra.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkSpaceRoomAmenity>()
                .HasOne(wra => wra.Amenity)
                .WithMany() 
                .HasForeignKey(wra => wra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<WorkSpaceRoomImage>()
                .HasOne(wri => wri.WorkSpaceRoom)
                .WithMany(wr => wr.WorkSpaceRoomImages)
                .HasForeignKey(wri => wri.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Booking>()
                .HasOne(b => b.WorkSpaceRoom)
                .WithMany(wr => wr.Bookings)
                .HasForeignKey(b => b.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.WorkSpaceRoom)
                .WithMany(wr => wr.Reviews)
                .HasForeignKey(r => r.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AvailabilitySchedule>()
                .HasOne(a => a.WorkSpaceRoom)
                .WithMany(wr => wr.AvailabilitySchedules)
                .HasForeignKey(a => a.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlockedTimeSlot>()
                .HasOne(b => b.WorkSpaceRoom)
                .WithMany(wr => wr.BlockedTimeSlots)
                .HasForeignKey(b => b.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Original Schema Configuration (with adjustments)

  
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

            modelBuilder.Entity<Address>()
                .HasMany(a => a.Workspaces)
                .WithOne(w => w.Address)
                .HasForeignKey(w => w.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

   
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
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

     
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

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Identity Tables Configuration

            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AppUserClaims").HasKey(x => x.Id);
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AppRoleClaims").HasKey(x => x.Id);
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AppUserLogins").HasKey(x => x.UserId);
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AppUserRoles").HasKey(x => new { x.RoleId, x.UserId });
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AppUserTokens").HasKey(x => x.UserId);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName() ?? string.Empty;
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            #endregion
        }
    }
}