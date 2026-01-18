using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int UserId { get; set; }
        public int PackageId { get; set; }

        public DateTime BookedAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        public string? ItineraryFile { get; set; }

        // Navigation:
        [ForeignKey(nameof(PackageId))]
        public TravelPackage Package { get; set; }
    }
}
