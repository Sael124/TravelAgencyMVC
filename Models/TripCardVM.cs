namespace TravelAgencyMVC.Models
{
    public class TripCardVM
    {
        public TravelPackage Package { get; set; } = null!;
        public string? PrimaryImageUrl { get; set; }

        public bool HasActiveDiscount { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
    }
}
