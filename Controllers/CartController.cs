using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public CartController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var items = _db.CartItems
                .Where(c => c.UserId == userId.Value && c.Status.Trim() == "Open")
                .ToList();

            var packageIds = items.Select(i => i.PackageId).ToList();

            var packages = _db.TravelPackages
                .Where(p => packageIds.Contains(p.PackageId))
                .ToList();

            var vm = items.Select(i =>
            {
                var p = packages.First(x => x.PackageId == i.PackageId);
                return new CartRowVM
                {
                    CartItemId = i.CartItemId,
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Price = p.BasePrice
                };
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public IActionResult Add(int packageId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // לא להכניס אותו דבר פעמיים
            bool exists = _db.CartItems.Any(c =>
                c.UserId == userId.Value &&
                c.PackageId == packageId &&
                c.Status.Trim() == "Open");

            if (!exists)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = userId.Value,
                    PackageId = packageId,
                    AddedAt = DateTime.Now,
                    Status = "Open"
                });
                _db.SaveChanges();
            }

            TempData["Success"] = "Added to cart.";
            return RedirectToAction("Index", "TravelPackages");
        }

        [HttpPost]
        public IActionResult Remove(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var item = _db.CartItems.FirstOrDefault(c => c.CartItemId == cartItemId && c.UserId == userId.Value);
            if (item == null)
                return NotFound();

            item.Status = "Removed";
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var items = _db.CartItems.Where(c => c.UserId == userId.Value && c.Status.Trim() == "Open").ToList();
            if (!items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Checkout(string cardNumber, string exp, string cvv)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            cardNumber = (cardNumber ?? "").Replace(" ", "");
            cvv = (cvv ?? "").Trim();

            if (cardNumber.Length < 12 || cvv.Length < 3)
            {
                TempData["Error"] = "Invalid payment data.";
                return View();
            }

            var items = _db.CartItems.Where(c => c.UserId == userId.Value && c.Status.Trim() == "Open").ToList();
            if (!items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return View();
            }

            foreach (var ci in items)
            {
                // בדיקת 3 הזמנות פעילות
                int activeBookings = _db.Bookings.Count(b => b.UserId == userId.Value && b.Status.Trim() == "Active");
                if (activeBookings >= 3)
                {
                    TempData["Error"] = "You already have 3 active trips. Remove items and try again.";
                    return View();
                }

                var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == ci.PackageId);
                if (pkg == null || pkg.AvailableRooms <= 0)
                {
                    TempData["Error"] = $"Trip '{ci.PackageId}' is not available.";
                    return View();
                }

                pkg.AvailableRooms--;

                var booking = new Booking
                {
                    UserId = userId.Value,
                    PackageId = ci.PackageId,
                    BookedAt = DateTime.Now,
                    Status = "Active",
                    TotalPrice = pkg.BasePrice
                };

                _db.Bookings.Add(booking);
                _db.SaveChanges();

                _db.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    PaidAt = DateTime.Now,
                    Amount = booking.TotalPrice,
                    Status = "Success",
                    Method = "Card",
                    TransactionRef = Guid.NewGuid().ToString()
                });

                // לסמן את פריט העגלה כ-Paid
                ci.Status = "Paid";

                // אם היה בתור - להסיר אותו
                var myEntry = _db.WaitingList.FirstOrDefault(w =>
                    w.PackageId == ci.PackageId &&
                    w.UserId == userId.Value &&
                    (w.Status.Trim() == "Waiting" || w.Status.Trim() == "Notified"));

                if (myEntry != null)
                    _db.WaitingList.Remove(myEntry);

                _db.SaveChanges();
            }

            TempData["Success"] = "Payment successful!";
            return RedirectToAction("MyTrips", "Account");
        }
    }
}
