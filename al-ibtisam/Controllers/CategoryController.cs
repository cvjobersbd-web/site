using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;

namespace al_ibtisam.Controllers
{
    public class CategoryController : Controller
    {
        // ============================================================
        // প্রতিটি ক্যাটাগরির জন্য Action
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
        // কমন মেথড — ProductStore থেকে ক্যাটাগরি অনুযায়ী ডাটা ফিল্টার করে ভিউ রিটার্ন
        // ============================================================
        private IActionResult LoadCategory(string categoryName)
        {
            ViewBag.CategoryName = categoryName;

            // ইন-মেমরি ProductStore থেকে ডাটা ফিল্টার করে আনা হচ্ছে
            var products = ProductStore.GetAll()
                .Where(p => p.CategoryName.ToLower() == categoryName.ToLower())
                .ToList();

            return View("Category", products);
        }
    }
}