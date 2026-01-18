using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(30)]
        public string Type { get; set; } = "System";   // למשל: WaitingList, Payment, Booking, System

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Message { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }   // אצלך זה מאפשר NULL

        public bool IsRead { get; set; } = false;
    }
}
