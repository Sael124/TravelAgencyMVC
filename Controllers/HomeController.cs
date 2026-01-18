using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TravelAgencyDbContext _db;


        public HomeController(ILogger<HomeController> logger, TravelAgencyDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        public IActionResult Index()
        {
            int tripsCount = _db.TravelPackages.Count(p => p.IsVisible);

            var reviews = _db.ServiceReviews
                .Where(r => r.IsPublished)
                .Join(
                    _db.Users,
                    r => r.UserId,
                    u => u.UserId,
                    (r, u) => new ServiceReviewVM
                    {
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UserFullName = u.FullName
                    }
                )
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.TotalTrips = tripsCount;
            ViewBag.ServiceReviews = reviews;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
