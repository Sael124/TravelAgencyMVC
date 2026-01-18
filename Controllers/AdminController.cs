using Microsoft.AspNetCore.Mvc;
using TravelAgencyMVC.Models;

namespace TravelAgencyMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly TravelAgencyDbContext _db;

        public AdminController(TravelAgencyDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetInt32("RoleId") == 1; // אצלך: 1 = Admin
        }

        public IActionResult ServiceReviews()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var list = _db.ServiceReviews
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult PublishServiceReview(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var r = _db.ServiceReviews.FirstOrDefault(x => x.ServiceReviewId == id);
            if (r == null) return NotFound();

            r.IsPublished = true;
            _db.SaveChanges();

            TempData["Success"] = "Review published.";
            return RedirectToAction("ServiceReviews");
        }

        [HttpPost]
        public IActionResult UnpublishServiceReview(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var r = _db.ServiceReviews.FirstOrDefault(x => x.ServiceReviewId == id);
            if (r == null) return NotFound();

            r.IsPublished = false;
            _db.SaveChanges();

            TempData["Success"] = "Review unpublished.";
            return RedirectToAction("ServiceReviews");
        }

        public IActionResult TripReviews()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var list = _db.TripReviews
                .Join(_db.Users,
                      r => r.UserId,
                      u => u.UserId,
                      (r, u) => new { r, u })
                .Join(_db.TravelPackages,
                      x => x.r.PackageId,
                      p => p.PackageId,
                      (x, p) => new AdminTripReviewRowVM
                      {
                          ReviewId = x.r.ReviewId,
                          Rating = x.r.Rating,
                          Comment = x.r.Comment,
                          CreatedAt = x.r.CreatedAt,

                          UserFullName = x.u.FullName,
                          PackageName = p.PackageName,
                          Destination = p.Destination,
                          Country = p.Country
                      })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult DeleteTripReview(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var r = _db.TripReviews.FirstOrDefault(x => x.ReviewId == id);
            if (r == null) return NotFound();

            _db.TripReviews.Remove(r);
            _db.SaveChanges();

            TempData["Success"] = "Trip review deleted.";
            return RedirectToAction("TripReviews");
        }

        public IActionResult Packages()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var list = _db.TravelPackages
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new AdminPackageRowVM
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Destination = p.Destination,
                    Country = p.Country,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    BasePrice = p.BasePrice,
                    AvailableRooms = p.AvailableRooms,
                    IsVisible = p.IsVisible,
                    CategoryId = p.CategoryId
                })
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult TogglePackageVisibility(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == id);
            if (pkg == null) return NotFound();

            pkg.IsVisible = !pkg.IsVisible;
            _db.SaveChanges();

            TempData["Success"] = "Package visibility updated.";
            return RedirectToAction("Packages");
        }

        [HttpGet]
        public IActionResult EditPackage(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == id);
            if (pkg == null) return NotFound();

            return View(pkg);
        }

        [HttpPost]
        public IActionResult EditPackage(int packageId, decimal basePrice, int availableRooms)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (pkg == null) return NotFound();

            if (basePrice < 0)
            {
                TempData["Error"] = "Price must be non-negative.";
                return RedirectToAction("EditPackage", new { id = packageId });
            }

            if (availableRooms < 0)
            {
                TempData["Error"] = "Rooms must be non-negative.";
                return RedirectToAction("EditPackage", new { id = packageId });
            }

            pkg.BasePrice = basePrice;
            pkg.AvailableRooms = availableRooms;

            _db.SaveChanges();

            TempData["Success"] = "Package updated.";
            return RedirectToAction("Packages");
        }

        public IActionResult Discounts()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var now = DateTime.Now;

            var list = _db.Discounts
                .OrderByDescending(d => d.StartAt)
                .ToList();

            ViewBag.Now = now;
            return View(list);
        }

        [HttpGet]
        public IActionResult CreateDiscount()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // רשימת חבילות לבחירה
            ViewBag.Packages = _db.TravelPackages
                .OrderBy(p => p.PackageName)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateDiscount(int packageId, decimal newPrice, DateTime startAt, DateTime endAt)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (pkg == null) return NotFound();

            if (newPrice <= 0 || newPrice >= pkg.BasePrice)
            {
                TempData["Error"] = "New price must be positive and lower than the base price.";
                return RedirectToAction("CreateDiscount");
            }

            if (endAt <= startAt)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("CreateDiscount");
            }

            // חובה: עד שבוע (7 ימים) מקסימום
            if ((endAt - startAt).TotalDays > 7)
            {
                TempData["Error"] = "Discount duration must be 7 days at most.";
                return RedirectToAction("CreateDiscount");
            }

            // למנוע שתי הנחות חופפות לאותו package
            bool overlap = _db.Discounts.Any(d =>
                d.PackageId == packageId &&
                d.IsActive &&
                !(endAt < d.StartAt || startAt > d.EndAt));

            if (overlap)
            {
                TempData["Error"] = "This package already has an active/overlapping discount.";
                return RedirectToAction("CreateDiscount");
            }

            var disc = new Discount
            {
                PackageId = packageId,
                OldPrice = pkg.BasePrice,
                NewPrice = newPrice,
                StartAt = startAt,
                EndAt = endAt,
                IsActive = true
            };

            _db.Discounts.Add(disc);
            _db.SaveChanges();

            TempData["Success"] = "Discount created.";
            return RedirectToAction("Discounts");
        }

        [HttpPost]
        public IActionResult ToggleDiscount(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var d = _db.Discounts.FirstOrDefault(x => x.DiscountId == id);
            if (d == null) return NotFound();

            d.IsActive = !d.IsActive;
            _db.SaveChanges();

            TempData["Success"] = "Discount status updated.";
            return RedirectToAction("Discounts");
        }

        [HttpGet]
        public IActionResult EditDiscount(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var d = _db.Discounts.FirstOrDefault(x => x.DiscountId == id);
            if (d == null) return NotFound();

            // כדי להציג שם החבילה במסך
            ViewBag.Packages = _db.TravelPackages
                .OrderBy(p => p.PackageName)
                .ToList();

            return View(d);
        }

        [HttpPost]
        public IActionResult EditDiscount(int discountId, int packageId, decimal newPrice, DateTime startAt, DateTime endAt, bool isActive)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var d = _db.Discounts.FirstOrDefault(x => x.DiscountId == discountId);
            if (d == null) return NotFound();

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (pkg == null) return NotFound();

            if (newPrice <= 0 || newPrice >= pkg.BasePrice)
            {
                TempData["Error"] = "New price must be positive and lower than the base price.";
                return RedirectToAction("EditDiscount", new { id = discountId });
            }

            if (endAt <= startAt)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("EditDiscount", new { id = discountId });
            }

            if ((endAt - startAt).TotalDays > 7)
            {
                TempData["Error"] = "Discount duration must be 7 days at most.";
                return RedirectToAction("EditDiscount", new { id = discountId });
            }

            // מניעת חפיפה עם הנחות אחרות לאותה חבילה (חוץ מההנחה הזו)
            bool overlap = _db.Discounts.Any(x =>
                x.DiscountId != discountId &&
                x.PackageId == packageId &&
                x.IsActive &&
                !(endAt < x.StartAt || startAt > x.EndAt));

            if (overlap)
            {
                TempData["Error"] = "This package already has an active/overlapping discount.";
                return RedirectToAction("EditDiscount", new { id = discountId });
            }

            d.PackageId = packageId;
            d.OldPrice = pkg.BasePrice;
            d.NewPrice = newPrice;
            d.StartAt = startAt;
            d.EndAt = endAt;
            d.IsActive = isActive;

            _db.SaveChanges();

            TempData["Success"] = "Discount updated.";
            return RedirectToAction("Discounts");
        }

        [HttpPost]
        public IActionResult DeleteDiscount(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var d = _db.Discounts.FirstOrDefault(x => x.DiscountId == id);
            if (d == null) return NotFound();

            _db.Discounts.Remove(d);
            _db.SaveChanges();

            TempData["Success"] = "Discount deleted.";
            return RedirectToAction("Discounts");
        }
        [HttpGet]
        public IActionResult CreatePackage()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.Categories = _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToList();


            return View();
        }

        [HttpPost]
        public IActionResult CreatePackage(TravelPackage p)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // בדיקות בסיס
            if (string.IsNullOrWhiteSpace(p.PackageName) ||
                string.IsNullOrWhiteSpace(p.Destination) ||
                string.IsNullOrWhiteSpace(p.Country))
            {
                TempData["Error"] = "PackageName / Destination / Country are required.";
                return RedirectToAction("CreatePackage");
            }

            if (p.EndDate <= p.StartDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction("CreatePackage");
            }

            if (p.BasePrice <= 0)
            {
                TempData["Error"] = "Price must be positive.";
                return RedirectToAction("CreatePackage");
            }

            if (p.AvailableRooms < 0)
            {
                TempData["Error"] = "Rooms must be non-negative.";
                return RedirectToAction("CreatePackage");
            }

            p.IsVisible = true;
            p.CreatedAt = DateTime.Now;  // אם יש לך עמודה כזו בטבלה
            _db.TravelPackages.Add(p);
            _db.SaveChanges();

            TempData["Success"] = "Package created. Now add images.";
            return RedirectToAction("AddImages", new { id = p.PackageId });
        }

        private IActionResult AdminGuard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetInt32("RoleId") != 1) // 1 = Admin
            {
                TempData["Error"] = "Access denied. Admins only.";
                return RedirectToAction("Index", "Home");
            }

            return null!;
        }

        // -------------------------
        // Add Images to Package
        // -------------------------
        [HttpGet]
        public IActionResult AddImages(int id)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var pkg = _db.TravelPackages.FirstOrDefault(p => p.PackageId == id);
            if (pkg == null) return NotFound();

            var images = _db.PackageImages
                .Where(i => i.PackageId == id)
                .OrderByDescending(i => i.IsPrimary)
                .ThenByDescending(i => i.ImageId)
                .ToList();

            ViewBag.PackageId = id;
            ViewBag.PackageName = pkg.PackageName;
            ViewBag.Images = images;

            return View();
        }

        [HttpPost]
        public IActionResult AddImage(int packageId, string imageUrl, bool isPrimary)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            imageUrl = (imageUrl ?? "").Trim();
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                TempData["Error"] = "Image URL is required.";
                return RedirectToAction("AddImages", new { id = packageId });
            }

            // אם סימנו Primary, נכבה Primary של כל השאר
            if (isPrimary)
            {
                var others = _db.PackageImages.Where(i => i.PackageId == packageId && i.IsPrimary).ToList();
                foreach (var x in others)
                    x.IsPrimary = false;
            }

            // אם זו התמונה הראשונה בחבילה - נהפוך אותה ל-Primary אוטומטית
            bool hasAny = _db.PackageImages.Any(i => i.PackageId == packageId);
            if (!hasAny)
                isPrimary = true;

            var img = new PackageImage
            {
                PackageId = packageId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary
            };

            _db.PackageImages.Add(img);
            _db.SaveChanges();

            TempData["Success"] = "Image added.";
            return RedirectToAction("AddImages", new { id = packageId });
        }

        [HttpPost]
        public IActionResult SetPrimaryImage(int id, int packageId)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var img = _db.PackageImages.FirstOrDefault(i => i.ImageId == id && i.PackageId == packageId);
            if (img == null) return NotFound();

            var others = _db.PackageImages.Where(i => i.PackageId == packageId && i.IsPrimary).ToList();
            foreach (var x in others)
                x.IsPrimary = false;

            img.IsPrimary = true;
            _db.SaveChanges();

            TempData["Success"] = "Primary image updated.";
            return RedirectToAction("AddImages", new { id = packageId });
        }

        [HttpPost]
        public IActionResult DeleteImage(int id, int packageId)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var img = _db.PackageImages.FirstOrDefault(i => i.ImageId == id && i.PackageId == packageId);
            if (img == null) return NotFound();

            bool wasPrimary = img.IsPrimary;

            _db.PackageImages.Remove(img);
            _db.SaveChanges();

            // אם מחקנו את ה-Primary ויש עוד תמונות, נבחר אחת חדשה כ-Primary
            if (wasPrimary)
            {
                var first = _db.PackageImages
                    .Where(i => i.PackageId == packageId)
                    .OrderByDescending(i => i.ImageId)
                    .FirstOrDefault();

                if (first != null)
                {
                    first.IsPrimary = true;
                    _db.SaveChanges();
                }
            }

            TempData["Success"] = "Image deleted.";
            return RedirectToAction("AddImages", new { id = packageId });
        }
        // -------------------------
        // Users (Admin)
        // -------------------------
        public IActionResult Users()
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            // לא מציגים סיסמאות בעמוד!
            var list = _db.Users
                .OrderBy(u => u.UserId)
                .Select(u => new AdminUserRowVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult ToggleUserStatus(int id)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var u = _db.Users.FirstOrDefault(x => x.UserId == id);
            if (u == null) return NotFound();

            // לא נותנים לנעול אדמין ראשי (RoleId=1)
            if (u.RoleId == 1)
            {
                TempData["Error"] = "You cannot disable an Admin account.";
                return RedirectToAction("Users");
            }

            var current = (u.Status ?? "").Trim();

            if (string.Equals(current, "Active", StringComparison.OrdinalIgnoreCase))
                u.Status = "Disabled";
            else
                u.Status = "Active";

            _db.SaveChanges();

            TempData["Success"] = $"User status updated to: {u.Status}";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var u = _db.Users.FirstOrDefault(x => x.UserId == id);
            if (u == null) return NotFound();

            // לא למחוק אדמין
            if (u.RoleId == 1)
            {
                TempData["Error"] = "You cannot delete an Admin account.";
                return RedirectToAction("Users");
            }

            // לא למחוק את המשתמש שמחובר כרגע
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId != null && currentUserId.Value == u.UserId)
            {
                TempData["Error"] = "You cannot delete your own account while logged in.";
                return RedirectToAction("Users");
            }

            // אם יש למשתמש הזמנות/תורים/סל וכו' – עדיף לא למחוק אלא להשבית
            bool hasBookings = _db.Bookings.Any(b => b.UserId == u.UserId);
            if (hasBookings)
            {
                TempData["Error"] = "This user has bookings. Disable the account instead of deleting.";
                return RedirectToAction("Users");
            }

            _db.Users.Remove(u);
            _db.SaveChanges();

            TempData["Success"] = "User deleted.";
            return RedirectToAction("Users");
        }

        public IActionResult UserBookings(int id)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            var u = _db.Users.FirstOrDefault(x => x.UserId == id);
            if (u == null) return NotFound();

            ViewBag.UserTitle = $"{u.FullName} ({u.Email})";

            var rows = _db.Bookings
                .Where(b => b.UserId == id)
                .Join(_db.TravelPackages,
                      b => b.PackageId,
                      p => p.PackageId,
                      (b, p) => new AdminUserBookingRowVM
                      {
                          BookingId = b.BookingId,
                          PackageName = p.PackageName,
                          Destination = p.Destination,
                          Country = p.Country,
                          StartDate = p.StartDate,
                          EndDate = p.EndDate,
                          Status = b.Status,
                          TotalPrice = b.TotalPrice,
                          BookedAt = b.BookedAt
                      })
                .OrderByDescending(x => x.BookedAt)
                .ToList();

            return View(rows);
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            // Roles לבחירה (Admin/User) מתוך ה-DB
            ViewBag.Roles = _db.Roles
                .OrderBy(r => r.RoleName)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string fullName, string email, string password, int roleId)
        {
            var guard = AdminGuard();
            if (guard != null) return guard;

            fullName = (fullName ?? "").Trim();
            email = (email ?? "").Trim();
            password = (password ?? "").Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Full name is required.";
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Email is required.";
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Password is required.";
                return RedirectToAction("CreateUser");
            }

            // אימות RoleId (רק Admin/User מהטבלה)
            bool roleOk = _db.Roles.Any(r => r.RoleId == roleId && (r.RoleName == "Admin" || r.RoleName == "User"));
            if (!roleOk)
            {
                TempData["Error"] = "Invalid role selected.";
                return RedirectToAction("CreateUser");
            }

            // מניעת כפילות אימייל
            bool exists = _db.Users.Any(u => u.Email.Trim() == email);
            if (exists)
            {
                TempData["Error"] = "This email already exists.";
                return RedirectToAction("CreateUser");
            }

            var u = new User
            {
                FullName = fullName,
                Email = email,
                Password = password,
                RoleId = roleId,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(u);
            _db.SaveChanges();

            TempData["Success"] = "User created successfully.";
            return RedirectToAction("Users");
        }





    }
}
