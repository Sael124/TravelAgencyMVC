namespace TravelAgencyMVC.Models
{
    public class CartRowVM
    {
        public int CartItemId { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
    }
}
