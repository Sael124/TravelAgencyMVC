using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public NotificationsController(TravelAgencyDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        public IActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var list = _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult MarkRead(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var n = _db.Notifications.FirstOrDefault(x => x.NotificationId == id && x.UserId == userId);
            if (n == null)
                return NotFound();

            n.IsRead = true;
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkAllRead()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var unread = _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var n in unread)
                n.IsRead = true;

            _db.SaveChanges();

            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction("Index");
        }
    }
}
