using al_ibtisam.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace al_ibtisam.Controllers
{
    /// <summary>
    /// অ্যাডমিন প্যানেল সংক্রান্ত সব কাজ।
    /// শুধু "Admin" রোল এর ইউজার ঢুকতে পারবে।
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _config;

        // ============================================================
        // Constructor — Dependency Injection
        // ============================================================
        public AdminController(
            AppDbContext db,
            ILogger<AdminController> logger,
            IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        // ============================================================
        // Admin Dashboard — সব ইউজারের লিস্ট
        // URL: /Admin/Dashboard
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var users = await _db.Users
                    .OrderByDescending(u => u.LastLoginAt ?? u.CreatedAt)
                    .ToListAsync();

                // ---------- Statistics ----------
                ViewBag.TotalUsers = users.Count;
                ViewBag.ActiveUsers = users.Count(u => u.IsActive);
                ViewBag.TodayLogins = users.Count(u =>
                    u.LastLoginAt.HasValue &&
                    u.LastLoginAt.Value.Date == DateTime.UtcNow.Date);

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Admin Dashboard error: {ex.Message}");
                ViewBag.ErrorMessage = "ডাটা লোড করতে সমস্যা হয়েছে।";
                ViewBag.TotalUsers = 0;
                ViewBag.ActiveUsers = 0;
                ViewBag.TodayLogins = 0;
                return View(new List<al_ibtisam.Models.ApplicationUser>());
            }
        }

        // ============================================================
        // ইউজার ডিটেইলস
        // URL: /Admin/UserDetails/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "ইউজার পাওয়া যায়নি।";
                return RedirectToAction("Dashboard");
            }
            return View(user);
        }

        // ============================================================
        // ইউজার Active / Inactive টগল
        // URL: /Admin/ToggleActive
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "ইউজার পাওয়া যায়নি।";
                return RedirectToAction("Dashboard");
            }

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{user.FullName} এর অ্যাকাউন্ট {(user.IsActive ? "সক্রিয়" : "নিষ্ক্রিয়")} করা হয়েছে।";
            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // ইউজার ডিলিট
        // URL: /Admin/DeleteUser
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "ইউজার পাওয়া যায়নি।";
                return RedirectToAction("Dashboard");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{user.FullName} কে ডিলিট করা হয়েছে।";
            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // Admin Sign Out
        // URL: /Admin/SignOut
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Admin signed out");
            TempData["SuccessMessage"] = "অ্যাডমিন প্যানেল থেকে সাইন আউট হয়েছেন।";
            return RedirectToAction("SignIn", "Account");
        }
    }
}