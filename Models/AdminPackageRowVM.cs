namespace TravelAgencyMVC.Models
{
    public class AdminPackageRowVM
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string Destination { get; set; }
        public string Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableRooms { get; set; }
        public bool IsVisible { get; set; }
        public int CategoryId { get; set; }
    }
}
