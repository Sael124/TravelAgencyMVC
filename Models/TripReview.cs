using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("TripReviews")]
    public class TripReview
    {
        [Key]
        public int ReviewId { get; set; }

        public int UserId { get; set; }
        public int PackageId { get; set; }

        public int Rating { get; set; } // 1..5

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
