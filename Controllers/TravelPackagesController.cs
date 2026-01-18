using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelAgencyMVC.Controllers
{
    public class TravelPackagesController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public TravelPackagesController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;

            var packages = _db.TravelPackages
                .Where(p => p.IsVisible)
                .OrderBy(p => p.StartDate)
                .ToList();

            var primaryImages = _db.PackageImages
                .Where(i => i.IsPrimary)
                .ToList();

            var activeDiscounts = _db.Discounts
                .Where(d => d.IsActive && d.StartAt <= now && now <= d.EndAt)
                .ToList();

            var vm = packages.Select(p =>
            {
                var img = primaryImages.FirstOrDefault(i => i.PackageId == p.PackageId);
                var disc = activeDiscounts.FirstOrDefault(d => d.PackageId == p.PackageId);

                return new TripCardVM
                {
                    Package = p,
                    PrimaryImageUrl = img?.ImageUrl,
                    HasActiveDiscount = disc != null,
                    OldPrice = disc?.OldPrice,
                    NewPrice = disc?.NewPrice
                };
            }).ToList();

            return View(vm);
        }

        public IActionResult Book(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var package = _db.TravelPackages.FirstOrDefault(p => p.PackageId == id && p.IsVisible);
            if (package == null)
                return NotFound();

            var img = _db.PackageImages.FirstOrDefault(i => i.PackageId == id && i.IsPrimary);
            ViewBag.PrimaryImageUrl = img?.ImageUrl;

            int waitingCount = _db.WaitingList.Count(w => w.PackageId == id && w.Status.Trim() == "Waiting");
            ViewBag.WaitingCount = waitingCount;

            bool isNotified = _db.WaitingList.Any(w =>
                w.PackageId == id &&
                w.UserId == userId.Value &&
                w.Status.Trim() == "Notified");
            ViewBag.IsNotified = isNotified;

            return View(package);
        }
        [HttpPost]
        public IActionResult ConfirmBooking(int packageId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // ✅ Transaction כדי למנוע מצב ששני משתמשים יקחו את החדר האחרון יחד
            using var tx = _db.Database.BeginTransaction();

            try
            {
                // 1) בדיקת Waiting List - רק הראשון יכול להזמין אם יש תור
                var waiting = _db.WaitingList
                    .Where(w => w.PackageId == packageId && w.Status.Trim() == "Waiting")
                    .OrderBy(w => w.JoinedAt)
                    .ToList();

                if (waiting.Any())
                {
                    var first = waiting.First();
                    if (first.UserId != userId.Value)
                    {
                        TempData["Error"] = "This trip is currently reserved for another user in the waiting list.";
                        tx.Rollback();
                        return RedirectToAction("Book", new { id = packageId });
                    }
                }

                // 2) בדיקת 3 הזמנות פעילות
                int activeBookings = _db.Bookings.Count(b => b.UserId == userId.Value && b.Status.Trim() == "Active");
                if (activeBookings >= 3)
                {
                    TempData["Error"] = "You already have 3 active trips.";
                    tx.Rollback();
                    return RedirectToAction("Book", new { id = packageId });
                }

                // 3) להביא את החבילה "בתוך הטרנזקציה"
                var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
                if (pkg == null)
                {
                    tx.Rollback();
                    return NotFound();
                }

                // 4) החדר האחרון - החלטה אטומית
                if (pkg.AvailableRooms <= 0)
                {
                    TempData["Error"] = "This trip is fully booked. You may join the waiting list.";
                    tx.Rollback();
                    return RedirectToAction("Book", new { id = packageId });
                }

                // 5) מורידים חדר לפני שמסיימים, הכל בתוך tx
                pkg.AvailableRooms--;

                // 6) יצירת הזמנה
                var booking = new Booking
                {
                    UserId = userId.Value,
                    PackageId = packageId,
                    BookedAt = DateTime.Now,
                    Status = "Active",
                    TotalPrice = pkg.BasePrice
                };

                _db.Bookings.Add(booking);

                // 7) אם המשתמש היה בתור - להסיר אותו
                var myWaiting = _db.WaitingList.FirstOrDefault(w =>
                    w.PackageId == packageId &&
                    w.UserId == userId.Value &&
                    w.Status.Trim() == "Waiting");

                if (myWaiting != null)
                    _db.WaitingList.Remove(myWaiting);

                _db.SaveChanges();

                tx.Commit();

                TempData["Success"] = "Your booking was confirmed!";
                return RedirectToAction("MyTrips", "Account");
            }
            catch
            {
                tx.Rollback();
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Book", new { id = packageId });
            }
        }


        [HttpPost]
        public IActionResult JoinWaitingList(int packageId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId && p.IsVisible);
            if (pkg == null)
                return NotFound();

            // אם יש חדרים - עדיף להזמין ולא להיכנס לתור
            if (pkg.AvailableRooms > 0)
            {
                TempData["Success"] = "A room is available now. You can book immediately.";
                return RedirectToAction("Book", new { id = packageId });
            }

            // למנוע כניסה כפולה לתור (Waiting או Notified)
            bool already = _db.WaitingList.Any(w =>
                w.PackageId == packageId &&
                w.UserId == userId.Value &&
                (w.Status.Trim() == "Waiting" || w.Status.Trim() == "Notified"));

            if (already)
            {
                TempData["Error"] = "You are already in the waiting list for this trip.";
                return RedirectToAction("Book", new { id = packageId });
            }

            var entry = new WaitingListEntry
            {
                PackageId = packageId,
                UserId = userId.Value,
                JoinedAt = DateTime.Now,
                Status = "Waiting",
                NotifiedAt = null
            };

            _db.WaitingList.Add(entry);
            _db.SaveChanges();

            int position = _db.WaitingList.Count(w =>
                w.PackageId == packageId && w.Status.Trim() == "Waiting");

            TempData["Success"] = $"You joined the waiting list. Your position is #{position}.";
            return RedirectToAction("Book", new { id = packageId });
        }
    }
}
