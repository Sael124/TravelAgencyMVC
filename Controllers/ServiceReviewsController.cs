using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class ServiceReviewsController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public ServiceReviewsController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult Create(int rating, string? comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            comment = (comment ?? "").Trim();

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5.";
                return View();
            }

            // אפשר להגביל לביקורת אחת למשתמש (הכי נקי)
            bool already = _db.ServiceReviews.Any(r => r.UserId == userId.Value);
            if (already)
            {
                TempData["Error"] = "You already submitted a service review.";
                return RedirectToAction("Index", "Home");
            }

            var review = new ServiceReview
            {
                UserId = userId.Value,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
                CreatedAt = DateTime.Now,
                IsPublished = false // חשוב: לא מתפרסם עד שאדמין יאשר
            };

            _db.ServiceReviews.Add(review);
            _db.SaveChanges();

            TempData["Success"] = "Thanks! Your review was sent for approval.";
            return RedirectToAction("Index", "Home");
        }
    }
}
