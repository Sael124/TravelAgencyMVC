namespace TravelAgencyMVC.Models
{
    public class AdminTripReviewRowVM
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserFullName { get; set; }
        public string PackageName { get; set; }
        public string Destination { get; set; }
        public string Country { get; set; }
    }
}
