using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("TravelPackages")]
    public class TravelPackage
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        [StringLength(100)]
        public string PackageName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        public int AvailableRooms { get; set; }

        public int AgeLimit { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public bool IsVisible { get; set; }

        public DateTime CreatedAt { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
