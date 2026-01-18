using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class PaymentController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public PaymentController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        // שלב 1 – פתיחת מסך תשלום
        public IActionResult BuyNow(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == id && p.IsVisible);
            if (pkg == null)
                return NotFound();

            if (pkg.AvailableRooms <= 0)
            {
                TempData["Error"] = "This trip is fully booked.";
                return RedirectToAction("Index", "TravelPackages");
            }

            return View(pkg);
        }

        // שלב 2 – ביצוע תשלום
        [HttpPost]
        public IActionResult BuyNow(int packageId, string cardNumber, string exp, string cvv)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // בדיקה בסיסית של נתוני כרטיס (לא שומרים!)
            cardNumber = (cardNumber ?? "").Replace(" ", "");
            cvv = (cvv ?? "").Trim();

            if (cardNumber.Length < 12 || cvv.Length < 3)
            {
                TempData["Error"] = "Invalid credit card details.";
                return RedirectToAction("BuyNow", new { id = packageId });
            }

            // בדיקת 3 הזמנות פעילות
            int activeBookings = _db.Bookings.Count(b => b.UserId == userId && b.Status == "Active");
            if (activeBookings >= 3)
            {
                TempData["Error"] = "You already have 3 active trips.";
                return RedirectToAction("Index", "TravelPackages");
            }

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (pkg == null || pkg.AvailableRooms <= 0)
            {
                TempData["Error"] = "Trip is no longer available.";
                return RedirectToAction("Index", "TravelPackages");
            }

            // יצירת Booking
            var booking = new Booking
            {
                UserId = userId.Value,
                PackageId = packageId,
                BookedAt = DateTime.Now,
                Status = "Active",
                TotalPrice = pkg.BasePrice
            };

            pkg.AvailableRooms--;

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            // יצירת Payment
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                PaidAt = DateTime.Now,
                Amount = booking.TotalPrice,
                Status = "Success",
                Method = "Card",
                TransactionRef = Guid.NewGuid().ToString()
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();

            TempData["Success"] = "Payment successful! Your trip has been booked.";
            return RedirectToAction("MyTrips", "Account");
        }
    }
}
