using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // পুরনো সব Hardcoded Action (এগুলো ঠিকই কাজ করবে)
        // ============================================================
        public IActionResult Panjabi() => LoadCategory("Panjabi");
        public IActionResult Thobe() => LoadCategory("Thobe");
        public IActionResult Jubbah() => LoadCategory("Jubbah");
        public IActionResult TShirt() => LoadCategory("T-Shirt");
        public IActionResult PoloShirt() => LoadCategory("Polo Shirt");
        public IActionResult PantTrouser() => LoadCategory("Pant & Trouser");
        public IActionResult WomensClothing() => LoadCategory("Women's Clothing");
        public IActionResult Attar() => LoadCategory("Attar");
        public IActionResult Perfumes() => LoadCategory("Perfumes");
        public IActionResult Gadgets() => LoadCategory("Gadgets");
        public IActionResult Watch() => LoadCategory("Watch");
        public IActionResult Wallet() => LoadCategory("Wallet");
        public IActionResult Sneakers() => LoadCategory("Sneakers");
        public IActionResult Waistcoat() => LoadCategory("Waistcoat");
        public IActionResult Tupi() => LoadCategory("Tupi");
        public IActionResult ComboOffers() => LoadCategory("Combo Offers");
        public IActionResult Katua() => LoadCategory("Katua");
        public IActionResult FlashSale() => LoadCategory("Flash Sale");

        // ============================================================
        // 🆕 নতুন জেনেরিক অ্যাকশন — যেকোনো নতুন ক্যাটাগরির জন্য
        // URL: /Category/Load?name=Bra  অথবা /Category/Bra (রাউট কনফিগ)
        // ============================================================
        [HttpGet]
        public IActionResult Load(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction("Index", "Home");

            return LoadCategory(name);
        }

        // ============================================================
        // কমন মেথড — ডাটাবেজ থেকে ক্যাটাগরি অনুযায়ী ডাটা ফিল্টার করে ভিউ রিটার্ন
        // ============================================================
        private IActionResult LoadCategory(string categoryName)
        {
            ViewBag.CategoryName = categoryName;

            // ডাটাবেজ থেকে ক্যাটাগরি অনুযায়ী প্রোডাক্ট ফিল্টার
            var products = _db.Products
                .Where(p => p.CategoryName.ToLower() == categoryName.ToLower())
                .OrderBy(p => p.Id)
                .ToList();

            return View("Category", products);
        }
    }
}