using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelAgencyMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public AccountController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            email = (email ?? "").Trim();
            password = (password ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email is required.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Password is required.";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u =>
                u.Email.Trim() == email &&
                u.Password.Trim() == password &&
                u.Status.Trim() == "Active");

            if (user == null)
            {
                ViewBag.Error = "Email or password is incorrect.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            // RoleId: 1 = Admin, 2 = User (לפי הטבלה שלך)
            if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "TravelPackages");

        }
        public IActionResult MyTrips()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var uid = userId.Value;
            var today = DateTime.Now.Date;

            // להביא את כל ה-Reviews של המשתמש פעם אחת (יעיל)
            var reviewedPackageIds = _db.TripReviews
                .Where(r => r.UserId == uid)
                .Select(r => r.PackageId)
                .Distinct()
                .ToList();

            var trips = _db.Bookings
                .Include(b => b.Package)
                .Where(b => b.UserId == uid)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => new MyTripRowVM
                {
                    BookingId = b.BookingId,
                    PackageId = b.PackageId,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice,
                    BookedAt = b.BookedAt,

                    PackageName = b.Package.PackageName,
                    Destination = b.Package.Destination,
                    Country = b.Package.Country,
                    StartDate = b.Package.StartDate,
                    EndDate = b.Package.EndDate,

                    // ✅ חדש
                    HasReview = reviewedPackageIds.Contains(b.PackageId),
                    // המלצה: לאפשר Review רק אחרי שהטיול התחיל/נגמר (אפשר גם אחרי EndDate)
                    CanReview = b.Package.EndDate.Date < today
                })
                .ToList();

            return View(trips);
        }

        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // להביא הזמנה + חבילה כדי לבדוק תאריכים
            var booking = _db.Bookings
                .Include(b => b.Package)
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId.Value);

            if (booking == null)
                return NotFound();

            if (booking.Status.Trim() != "Active")
            {
                TempData["Error"] = "This booking cannot be cancelled.";
                return RedirectToAction("MyTrips");
            }

            // ✅ כלל ביטול בסיסי: מותר עד 3 ימים לפני StartDate
            var now = DateTime.Now;
            var start = booking.Package.StartDate;

            if ((start - now).TotalDays < 3)
            {
                TempData["Error"] = "Cancellation period has ended (less than 3 days before departure).";
                return RedirectToAction("MyTrips");
            }

            // לבטל
            booking.Status = "Cancelled";

            // להחזיר חדר לחבילה
            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == booking.PackageId);
            if (pkg != null)
                pkg.AvailableRooms += 1;

            _db.SaveChanges();

            // לקדם תור (אם יש ממתינים)
            var next = _db.WaitingList
                .Where(w => w.PackageId == booking.PackageId && w.Status.Trim() == "Waiting")
                .OrderBy(w => w.JoinedAt)
                .FirstOrDefault();

            if (next != null)
            {
                next.Status = "Notified";
                next.NotifiedAt = DateTime.Now;
                _db.SaveChanges();

                // ✅ יצירת Notification למשתמש הראשון בתור
                _db.Notifications.Add(new Notification
                {
                    UserId = next.UserId,
                    Type = "WaitingList",
                    Title = "A room is now available!",
                    Message = $"A room became available for package #{booking.PackageId}. " +
                              $"You can now book it from the trip page. This offer is for the first user in the waiting list.",
                    CreatedAt = DateTime.Now,
                    SentAt = DateTime.Now,
                    IsRead = false
                });

                _db.SaveChanges();

                TempData["Success"] = "Booking cancelled. The next user in the waiting list was notified.";
            }
            else
            {
                TempData["Success"] = "Booking cancelled successfully.";
            }

            return RedirectToAction("MyTrips");
        }
        public IActionResult Notifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var list = _db.Notifications
                .Where(n => n.UserId == userId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult MarkNotificationRead(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var n = _db.Notifications.FirstOrDefault(x => x.NotificationId == id && x.UserId == userId.Value);
            if (n != null)
            {
                n.IsRead = true;
                _db.SaveChanges();
            }

            return RedirectToAction("Notifications");
        }






        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
