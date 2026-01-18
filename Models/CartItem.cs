using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int UserId { get; set; }
        public int PackageId { get; set; }

        public DateTime AddedAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";
    }
}
