using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("Discounts")]
    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        public int PackageId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal OldPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal NewPrice { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public bool IsActive { get; set; }
    }
}
