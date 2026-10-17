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

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // এখানে আপনার ড্যাশবোর্ডের লজিক থাকবে
            var users = await _db.Users.ToListAsync();

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.IsActive);
            ViewBag.TodayLogins = users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value.Date == DateTime.UtcNow.Date);

            return View(users);
        }

        // GET: /Admin/Users
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

        // GET: /Admin/Products
        public async Task<IActionResult> Products()
        {
            var products = await _db.Products.ToListAsync();
            return View(products);
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

        // GET: /Admin/Categories
        public IActionResult Categories()
        {
            // CategoryStore থেকে ডাটা নিয়ে আসা
            var categories = CategoryStore.GetAll();
            // এখানে আপনার ViewBag.CategoriesData ফিল্টার লজিক বসাতে হবে (যদি প্রয়োজন হয়)
            return View();
        }

        // POST: /Admin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                CategoryStore.Add(categoryName);
                TempData["Success"] = "নতুন ক্যাটাগরি যোগ করা হয়েছে।";
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
                CategoryStore.Delete(categoryName);
                TempData["Success"] = "ক্যাটাগরি ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/SignOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }
    }
}