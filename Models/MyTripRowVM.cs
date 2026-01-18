namespace TravelAgencyMVC.Models
{
    public class MyTripRowVM
    {
        public int BookingId { get; set; }
        public int PackageId { get; set; }

        public string PackageName { get; set; }
        public string Destination { get; set; }
        public string Country { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime BookedAt { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

        // ✅ חדש
        public bool HasReview { get; set; }
        public bool CanReview { get; set; }
    }
}
