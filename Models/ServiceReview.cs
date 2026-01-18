using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("ServiceReviews")]
    public class ServiceReview
    {
        [Key]
        public int ServiceReviewId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; } // 1..5

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
