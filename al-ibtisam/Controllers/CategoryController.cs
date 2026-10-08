using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;

namespace al_ibtisam.Controllers
{
    public class CategoryController : Controller
    {
        // ============================================================
        // ডামি ডাটা — পরে আসল ডাটাবেজ থেকে ডাটা আনবেন
        // ============================================================
        private List<Product> GetDummyProducts()
        {
            return new List<Product>
            {
                // ---------- Panjabi ----------
                new Product { Id = 1,  Name = "As-Shabab Panjabi - 23",              ImageUrl = "https://i.ibb.co.com/mVHcbR0p/As-Shabab-Panjabi-13.jpg",       Price = 500,  OldPrice = null, DiscountPercent = 0,  CategoryName = "Panjabi" },
                new Product { Id = 2,  Name = "Semi Luxury Panjabi - 09",             ImageUrl = "https://i.ibb.co.com/RTpJ1KLY/As-Shabab-Panjabi-09.jpg",       Price = 2950, OldPrice = null, DiscountPercent = 0,  CategoryName = "Panjabi" },
                new Product { Id = 3,  Name = "Premium Panjabi - 10",                 ImageUrl = "https://i.ibb.co.com/ymjnSMHt/Superior-Panjabi-08.jpg",        Price = 2690, OldPrice = null, DiscountPercent = 0,  CategoryName = "Panjabi" },
                new Product { Id = 4,  Name = "Premium Panjabi - 09",                 ImageUrl = "https://i.ibb.co.com/JRWPJpdn/As-Shabab-Panjabi-08.jpg",       Price = 2690, OldPrice = null, DiscountPercent = 0,  CategoryName = "Panjabi" },
                new Product { Id = 5,  Name = "Premium Panjabi - 08",                 ImageUrl = "https://i.ibb.co.com/5WKkqDgr/Original-Arab-s-Thobe-22.jpg",   Price = 2690, OldPrice = null, DiscountPercent = 0,  CategoryName = "Panjabi" },
                new Product { Id = 6,  Name = "Semi Luxury Panjabi Combo - 0708",     ImageUrl = "https://i.ibb.co.com/cXb0F6qS/Soft-Arabian-Thobe-26.jpg",      Price = 2950, OldPrice = 5900, DiscountPercent = 50, CategoryName = "Panjabi" },
                new Product { Id = 7,  Name = "Semi Luxury Panjabi Combo - 0609",     ImageUrl = "https://i.ibb.co.com/hFPqhKn7/Soft-Arabian-Thobe-25.jpg",      Price = 2950, OldPrice = 5900, DiscountPercent = 50, CategoryName = "Panjabi" },
                new Product { Id = 8,  Name = "Superior Panjabi Combo - 0607",        ImageUrl = "https://i.ibb.co.com/S4P1QVnd/Premium-Arabian-Thobe-27.jpg",   Price = 2250, OldPrice = 4500, DiscountPercent = 50, CategoryName = "Panjabi" },

                // ---------- Thobe ----------
                new Product { Id = 9,  Name = "Emirati Thobe - 01",                   ImageUrl = "https://i.ibb.co.com/5X6DjR4y/Emirati-Thobe-01.jpg",           Price = 2250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Thobe" },
                new Product { Id = 10, Name = "Emirati Thobe - 02",                   ImageUrl = "https://i.ibb.co.com/h1KQvZX4/Emirati-Thobe-02.jpg",           Price = 2250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Thobe" },
                new Product { Id = 11, Name = "Emirati Thobe - 19",                   ImageUrl = "https://i.ibb.co.com/1f8bJ2Ny/Emirati-Thobe-19.jpg",           Price = 2250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Thobe" },
                new Product { Id = 12, Name = "Emirati Thobe - 23",                   ImageUrl = "https://i.ibb.co.com/7JHxJDVk/Emirati-Thobe-23.jpg",           Price = 2250, OldPrice = 4500, DiscountPercent = 50, CategoryName = "Thobe" },

                // ---------- Shirt ----------
                new Product { Id = 13, Name = "Premium Formal Shirt - 01",            ImageUrl = "https://i.ibb.co.com/VYskZpVz/2.jpg",                          Price = 1250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Shirt" },
                new Product { Id = 14, Name = "Premium Formal Shirt - 02",            ImageUrl = "https://i.ibb.co.com/whjZxfNK/3.jpg",                          Price = 1250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Shirt" },
                new Product { Id = 15, Name = "Casual Shirt - 03",                    ImageUrl = "https://i.ibb.co.com/JwbWyD8G/4.jpg",                          Price = 1150, OldPrice = null, DiscountPercent = 0,  CategoryName = "Shirt" },
                new Product { Id = 16, Name = "Casual Shirt - 04",                    ImageUrl = "https://i.ibb.co.com/gLKg35Sq/5.jpg",                          Price = 1150, OldPrice = 2300, DiscountPercent = 50, CategoryName = "Shirt" },

                // ---------- T-Shirt ----------
                new Product { Id = 17, Name = "Waffle Drop Shoulder - Tawaqkul",      ImageUrl = "https://i.ibb.co.com/jPFyGtNg/Waffle-Drop-Shoulder-T-shirt-Muslim.jpg",        Price = 890, OldPrice = null, DiscountPercent = 0, CategoryName = "T-Shirt" },
                new Product { Id = 18, Name = "Waffle Drop Shoulder - Khilafah",      ImageUrl = "https://i.ibb.co.com/BKy5fc43/Waffle-Drop-Shoulder-T-shirt-Khilafah.jpg",      Price = 890, OldPrice = null, DiscountPercent = 0, CategoryName = "T-Shirt" },
                new Product { Id = 19, Name = "Waffle Drop Shoulder - Desciplined",   ImageUrl = "https://i.ibb.co.com/gFVXwBjM/Waffle-Drop-Shoulder-T-shirt-Desciplined.jpg",   Price = 890, OldPrice = null, DiscountPercent = 0, CategoryName = "T-Shirt" },
                new Product { Id = 20, Name = "Waffle Drop Shoulder - Inspired",      ImageUrl = "https://i.ibb.co.com/7NXCzbFk/Waffle-Drop-Shoulder-T-shirt-Inspired.jpg",      Price = 890, OldPrice = 1780, DiscountPercent = 50, CategoryName = "T-Shirt" },

                // ---------- Polo Shirt ----------
                new Product { Id = 21, Name = "Premium Polo Shirt - 01",              ImageUrl = "https://i.ibb.co.com/whjZxfNK/3.jpg",                          Price = 990, OldPrice = null, DiscountPercent = 0, CategoryName = "Polo Shirt" },
                new Product { Id = 22, Name = "Premium Polo Shirt - 02",              ImageUrl = "https://i.ibb.co.com/JwbWyD8G/4.jpg",                          Price = 990, OldPrice = null, DiscountPercent = 0, CategoryName = "Polo Shirt" },
                new Product { Id = 23, Name = "Premium Polo Shirt - 03",              ImageUrl = "https://i.ibb.co.com/gLKg35Sq/5.jpg",                          Price = 990, OldPrice = null, DiscountPercent = 0, CategoryName = "Polo Shirt" },
                new Product { Id = 24, Name = "Premium Polo Shirt - 04",              ImageUrl = "https://i.ibb.co.com/RkNZH6Vh/6.jpg",                          Price = 990, OldPrice = 1980, DiscountPercent = 50, CategoryName = "Polo Shirt" },

                // ---------- Pant & Trouser ----------
                new Product { Id = 25, Name = "China Angle Trouser - Dark Chocolate", ImageUrl = "https://i.ibb.co.com/Wv0vpLyJ/China-Angle-Trouser-Dark-Chocolate.jpg", Price = 1890, OldPrice = null, DiscountPercent = 0,  CategoryName = "Pant & Trouser" },
                new Product { Id = 26, Name = "China Angle Trouser - Olive",          ImageUrl = "https://i.ibb.co.com/gFFdwXJc/China-Angle-Trouser-Olive.jpg",          Price = 1890, OldPrice = null, DiscountPercent = 0,  CategoryName = "Pant & Trouser" },
                new Product { Id = 27, Name = "China Angle Trouser - Beige",          ImageUrl = "https://i.ibb.co.com/5XQmBL9X/China-Angle-Trouser-Beige.jpg",          Price = 1890, OldPrice = null, DiscountPercent = 0,  CategoryName = "Pant & Trouser" },
                new Product { Id = 28, Name = "China Micro Pajama - White",           ImageUrl = "https://i.ibb.co.com/hx3GD1yx/China-Micro-Pajama-White.jpg",           Price = 1890, OldPrice = 3780, DiscountPercent = 50, CategoryName = "Pant & Trouser" },

                // ---------- Attar ----------
                new Product { Id = 29, Name = "Jadore Al Oud",                        ImageUrl = "https://i.ibb.co.com/21gWcMk1/Jadore-Al-Oud.jpg",              Price = 1250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Attar" },
                new Product { Id = 30, Name = "Safa",                                 ImageUrl = "https://i.ibb.co.com/zhtxbdHw/Safa.jpg",                       Price = 950,  OldPrice = null, DiscountPercent = 0,  CategoryName = "Attar" },
                new Product { Id = 31, Name = "Best Seller Attar Combo 60ml",         ImageUrl = "https://i.ibb.co.com/gFvWzrDZ/Best-Seller-Attar-Combo-60ml.jpg", Price = 1890, OldPrice = null, DiscountPercent = 0, CategoryName = "Attar" },
                new Product { Id = 32, Name = "Corporate Elegance Combo 2.0",         ImageUrl = "https://i.ibb.co.com/7xwwtRRg/Corporate-Elegance-Combo-2-0.jpg", Price = 2250, OldPrice = 4500, DiscountPercent = 50, CategoryName = "Attar" },

                // ---------- Sneakers ----------
                new Product { Id = 33, Name = "NB 9060 Dark Gray Replica",            ImageUrl = "https://i.ibb.co.com/35qxDPsK/NB-9060-Dark-Gray-Replica.jpg",  Price = 2250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Sneakers" },
                new Product { Id = 34, Name = "NB 9060 Gray Black Replica",           ImageUrl = "https://i.ibb.co.com/xqS4VNJV/NB-9060-Gray-Black-Replica.jpg", Price = 2250, OldPrice = null, DiscountPercent = 0,  CategoryName = "Sneakers" },
                new Product { Id = 35, Name = "China Branded Sneakers Y26-1084",      ImageUrl = "https://i.ibb.co.com/pvs72Mmv/China-Branded-Sneakers-Y26-1084.jpg", Price = 1890, OldPrice = null, DiscountPercent = 0, CategoryName = "Sneakers" },
                new Product { Id = 36, Name = "China Branded Sneakers Y26-1076",      ImageUrl = "https://i.ibb.co.com/NdP6w6q4/China-Branded-Sneakers-Y26-1076.jpg", Price = 1890, OldPrice = 3780, DiscountPercent = 50, CategoryName = "Sneakers" },

                // ---------- Waistcoat ----------
                new Product { Id = 37, Name = "Platinum Jacquard Waistcoat - 111",    ImageUrl = "https://i.ibb.co.com/BHZm9SrF/Platinum-Jacquard-Waistcoat-111.jpg", Price = 2250, OldPrice = null, DiscountPercent = 0, CategoryName = "Waistcoat" },
                new Product { Id = 38, Name = "Platinum Jacquard Waistcoat - 108",    ImageUrl = "https://i.ibb.co.com/F4fPjnwr/Platinum-Jacquard-Waistcoat-108.jpg", Price = 2250, OldPrice = null, DiscountPercent = 0, CategoryName = "Waistcoat" },
                new Product { Id = 39, Name = "Platinum Jacquard Waistcoat - 106",    ImageUrl = "https://i.ibb.co.com/BVj5M5rY/Platinum-Jacquard-Waistcoat-106.jpg", Price = 2250, OldPrice = null, DiscountPercent = 0, CategoryName = "Waistcoat" },
                new Product { Id = 40, Name = "Premium Suit Embroidered - 303",       ImageUrl = "https://i.ibb.co.com/zWMGgjzs/Premium-Suit-Embroidered-Waistcoat-303.jpg", Price = 2250, OldPrice = 4500, DiscountPercent = 50, CategoryName = "Waistcoat" },

                // ---------- Jubbah ----------
                new Product { Id = 41, Name = "As-Shabab Panjabi - 08",               ImageUrl = "https://i.ibb.co.com/5WKkqDgr/Original-Arab-s-Thobe-22.jpg",   Price = 1890, OldPrice = null, DiscountPercent = 0, CategoryName = "Jubbah" },
                new Product { Id = 42, Name = "As-Shabab Panjabi - 09",               ImageUrl = "https://i.ibb.co.com/cXb0F6qS/Soft-Arabian-Thobe-26.jpg",      Price = 1890, OldPrice = null, DiscountPercent = 0, CategoryName = "Jubbah" },
                new Product { Id = 43, Name = "As-Shabab Panjabi - 13",               ImageUrl = "https://i.ibb.co.com/hFPqhKn7/Soft-Arabian-Thobe-25.jpg",      Price = 1890, OldPrice = null, DiscountPercent = 0, CategoryName = "Jubbah" },
                new Product { Id = 44, Name = "Superior Panjabi - 08",                ImageUrl = "https://i.ibb.co.com/S4P1QVnd/Premium-Arabian-Thobe-27.jpg",   Price = 2250, OldPrice = 4500, DiscountPercent = 50, CategoryName = "Jubbah" },
            };
        }

        // ============================================================
        // প্রতিটি ক্যাটাগরির জন্য Action
        // ============================================================
        public IActionResult Panjabi()
        {
            return LoadCategory("Panjabi");
        }

        public IActionResult Thobe()
        {
            return LoadCategory("Thobe");
        }

        public IActionResult Shirt()
        {
            return LoadCategory("Shirt");
        }

        public IActionResult TShirt()
        {
            return LoadCategory("T-Shirt");
        }

        public IActionResult PoloShirt()
        {
            return LoadCategory("Polo Shirt");
        }

        public IActionResult PantTrouser()
        {
            return LoadCategory("Pant & Trouser");
        }

        public IActionResult WomensClothing()
        {
            return LoadCategory("Women's Clothing");
        }

        public IActionResult Attar()
        {
            return LoadCategory("Attar");
        }

        public IActionResult Perfumes()
        {
            return LoadCategory("Perfumes");
        }

        public IActionResult Gadgets()
        {
            return LoadCategory("Gadgets");
        }

        public IActionResult Watch()
        {
            return LoadCategory("Watch");
        }

        public IActionResult Wallet()
        {
            return LoadCategory("Wallet");
        }

        public IActionResult Sneakers()
        {
            return LoadCategory("Sneakers");
        }

        public IActionResult Waistcoat()
        {
            return LoadCategory("Waistcoat");
        }

        public IActionResult Tupi()
        {
            return LoadCategory("Tupi");
        }

        public IActionResult ComboOffers()
        {
            return LoadCategory("Combo Offers");
        }

        public IActionResult Jubbah()
        {
            return LoadCategory("Jubbah");
        }

        public IActionResult FlashSale()
        {
            return LoadCategory("Flash Sale");
        }

        // ============================================================
        // কমন মেথড — ক্যাটাগরি অনুযায়ী প্রোডাক্ট ফিল্টার করে ভিউ রিটার্ন
        // ============================================================
        private IActionResult LoadCategory(string categoryName)
        {
            ViewBag.CategoryName = categoryName;
            var products = GetDummyProducts()
                .Where(p => p.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View("Category", products);
        }
    }
}