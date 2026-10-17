using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    // [Authorize(Roles = "Admin")] // প্রয়োজনে এটি আনকমেন্ট করুন
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext db, ILogger<AdminController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================================
        // 1. DASHBOARD
        // URL: /Admin/Dashboard
        // ============================================================
        public async Task<IActionResult> Dashboard()
        {
            var users = await _db.Users.ToListAsync();

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.IsActive);
            ViewBag.TodayLogins = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value.Date == DateTime.UtcNow.Date);

            return View(users);
        }

        // ============================================================
        // 2. USERS MANAGEMENT
        // URL: /Admin/Users
        // ============================================================
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            return View(users);
        }

        // GET: /Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /Admin/ToggleActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] = "ইউজারের স্ট্যাটাস আপডেট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                TempData["Success"] = "ইউজার ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Users));
        }

        // ============================================================
        // 3. PRODUCTS MANAGEMENT
        // URL: /Admin/Products
        // ============================================================
        public async Task<IActionResult> Products()
        {
            var products = await _db.Products.ToListAsync();
            return View(products);
        }

        // GET: /Admin/AddProduct
        public IActionResult AddProduct()
        {
            return View(new Product());
        }

        // POST: /Admin/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product model)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "নতুন প্রোডাক্ট সফলভাবে যোগ করা হয়েছে।";
                return RedirectToAction(nameof(Products));
            }
            return View(model);
        }

        // POST: /Admin/DeleteProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "প্রোডাক্ট ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Products));
        }

        // ============================================================
        // 4. CATEGORIES MANAGEMENT
        // URL: /Admin/Categories
        // ============================================================
        public async Task<IActionResult> Categories()
        {
            // CategoryStore থেকে সব ক্যাটাগরি নিয়ে আসা
            var categories = al_ibtisam.Models.CategoryStore.GetAll();

            // ডাটাবেজ থেকে সব প্রোডাক্ট নিয়ে আসা (ক্যাটাগরি কাউন্ট করার জন্য)
            var products = await _db.Products.ToListAsync();

            // প্রতিটি ক্যাটাগরির জন্য কতগুলো প্রোডাক্ট আছে তা হিসাব করা
            var categoriesData = categories.Select(catName => new
            {
                Name = catName,
                ProductCount = products.Count(p => !string.IsNullOrEmpty(p.CategoryName) && p.CategoryName.Equals(catName, StringComparison.OrdinalIgnoreCase))
            }).ToList();

            // ViewBag-এ ডাটা পাঠানো হচ্ছে (আপনার Categories.cshtml ভিউতে ব্যবহারের জন্য)
            ViewBag.CategoriesData = categoriesData;

            return View();
        }

        // POST: /Admin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                // CategoryStore-এ নতুন ক্যাটাগরি যোগ করা
                al_ibtisam.Models.CategoryStore.Add(categoryName);
                TempData["Success"] = "নতুন ক্যাটাগরি যোগ করা হয়েছে।";
            }
            else
            {
                TempData["Error"] = "ক্যাটাগরির নাম খালি রাখা যাবে না।";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                // CategoryStore থেকে ক্যাটাগরি ডিলিট করা
                al_ibtisam.Models.CategoryStore.Delete(categoryName);
                TempData["Success"] = "ক্যাটাগরি ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Categories));
        }

        // ============================================================
        // 5. SIGN OUT
        // URL: /Admin/SignOut
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }
    }
}