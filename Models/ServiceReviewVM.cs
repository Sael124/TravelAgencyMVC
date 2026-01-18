namespace TravelAgencyMVC.Models
{
    public class ServiceReviewVM
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserFullName { get; set; }
    }
}
