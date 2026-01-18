using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("WaitingList")]
    public class WaitingListEntry
    {
        [Key]
        public int WaitingId { get; set; }

        public int UserId { get; set; }
        public int PackageId { get; set; }

        public DateTime JoinedAt { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Waiting";

        public DateTime? NotifiedAt { get; set; }

        // Navigation (כמו שעשינו ב-Bookings)
        [ForeignKey(nameof(PackageId))]
        public TravelPackage Package { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
