using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;

namespace al_ibtisam.Controllers
{
    public class ProductController : Controller
    {
        // ============================================================
        // প্রোডাক্ট ডিটেইলস পেজ
        // GET: /Product/Details/1
        // ============================================================
        public IActionResult Details(int id)
        {
            // ProductStore থেকে নির্দিষ্ট আইডির প্রোডাক্ট খোঁজা
            var product = ProductStore.GetById(id);

            if (product == null)
            {
                // প্রোডাক্ট না পাওয়া গেলে হোম পেজে পাঠিয়ে দেওয়া
                return RedirectToAction("Index", "Home");
            }

            // একই ক্যাটাগরির রিলেটেড প্রোডাক্ট (সর্বোচ্চ ৪টি)
            var related = ProductStore.GetAll()
                .Where(p => p.CategoryName.ToLower() == product.CategoryName.ToLower() && p.Id != product.Id)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = related;

            return View(product);
        }
    }
}