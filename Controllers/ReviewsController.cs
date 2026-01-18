using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public ReviewsController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Trip(int packageId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // חובה: המשתמש הזמין את הטיול
            bool hasBooking = _db.Bookings.Any(b =>
                b.UserId == userId.Value &&
                b.PackageId == packageId);

            if (!hasBooking)
                return Forbid();

            // אם כבר יש Review - נציג אותו לעריכה/צפייה (נעדיף חסימה - לפי דרישה)
            bool alreadyReviewed = _db.TripReviews.Any(r =>
                r.UserId == userId.Value &&
                r.PackageId == packageId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You already submitted a review for this trip.";
                return RedirectToAction("MyTrips", "Account");
            }

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (pkg == null) return NotFound();

            ViewBag.PackageName = pkg.PackageName;
            ViewBag.PackageId = packageId;

            return View();
        }

        [HttpPost]
        public IActionResult Trip(int packageId, int rating, string? comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            comment = (comment ?? "").Trim();

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Trip", new { packageId });
            }

            bool hasBooking = _db.Bookings.Any(b =>
                b.UserId == userId.Value &&
                b.PackageId == packageId);

            if (!hasBooking)
                return Forbid();

            bool alreadyReviewed = _db.TripReviews.Any(r =>
                r.UserId == userId.Value &&
                r.PackageId == packageId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You already submitted a review for this trip.";
                return RedirectToAction("MyTrips", "Account");
            }

            var review = new TripReview
            {
                UserId = userId.Value,
                PackageId = packageId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
                CreatedAt = DateTime.Now
            };

            _db.TripReviews.Add(review);
            _db.SaveChanges();

            TempData["Success"] = "Thanks! Your review was submitted.";
            return RedirectToAction("MyTrips", "Account");
        }
    }
}
