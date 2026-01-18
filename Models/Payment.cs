using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public DateTime PaidAt { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Success";

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = "Card";

        [Required]
        [StringLength(100)]
        public string TransactionRef { get; set; } = Guid.NewGuid().ToString();
    }
}
