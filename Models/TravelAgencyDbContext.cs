using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TravelAgencyMVC.Models
{
    public class TravelAgencyDbContext : DbContext
    {
        public DbSet<TravelPackage> TravelPackages { get; set; } = null!;
        public DbSet<PackageImage> PackageImages { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WaitingListEntry> WaitingList { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TripReview> TripReviews { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;

        public TravelAgencyDbContext(DbContextOptions<TravelAgencyDbContext> options)
            : base(options)
        {
        }
    }
}
