using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // প্রোডাক্ট ডিটেইলস পেজ
        // GET: /Product/Details/1
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            // ডাটাবেজ থেকে নির্দিষ্ট আইডির প্রোডাক্ট খোঁজা
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                // প্রোডাক্ট না পাওয়া গেলে হোম পেজে পাঠিয়ে দেওয়া
                return RedirectToAction("Index", "Home");
            }

            // একই ক্যাটাগরির রিলেটেড প্রোডাক্ট (সর্বোচ্চ ৪টি)
            var related = await _db.Products
                .Where(p => p.CategoryName.ToLower() == product.CategoryName.ToLower()
                            && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = related;

            return View(product);
        }
    }
}