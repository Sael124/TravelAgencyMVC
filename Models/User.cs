using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyMVC.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required, StringLength(20)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
