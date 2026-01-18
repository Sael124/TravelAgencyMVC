namespace TravelAgencyMVC.Models
{
    public class AdminUserBookingRowVM
    {
        public int BookingId { get; set; }

        public string PackageName { get; set; } = "";
        public string Destination { get; set; } = "";
        public string Country { get; set; } = "";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public DateTime BookedAt { get; set; }
    }
}
