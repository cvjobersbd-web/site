using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace al_ibtisam.Controllers
{
    [IgnoreAntiforgeryToken]
    public class CheckoutController : Controller
    {
        private const string CartCookieKey = "MyCart";
        private const string BuyNowCookieKey = "BuyNowItem";

        // ============================================================
        // Checkout পেজ (GET) — Cart থেকে
        // URL: /Checkout/Checkout
        // ============================================================
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            var delivery = 110m;
            var total = subtotal + delivery;

            ViewBag.CartItems = cart;
            ViewBag.Subtotal = subtotal;
            ViewBag.DeliveryCharge = delivery;
            ViewBag.Total = total;
            ViewBag.IsBuyNow = false;

            return View(new CheckoutViewModel());
        }

        // ============================================================
        // Buy Now → Checkout (GET) — শুধু একটি প্রোডাক্ট নিয়ে
        // URL: /Checkout/BuyNowCheckout
        // ============================================================
        public IActionResult BuyNowCheckout()
        {
            var buyNowItem = GetBuyNowItem();
            if (buyNowItem == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var cart = new List<CartItemModel> { buyNowItem };
            var subtotal = buyNowItem.Price * buyNowItem.Quantity;
            var delivery = 110m;
            var total = subtotal + delivery;

            ViewBag.CartItems = cart;
            ViewBag.Subtotal = subtotal;
            ViewBag.DeliveryCharge = delivery;
            ViewBag.Total = total;
            ViewBag.IsBuyNow = true;

            return View("Checkout", new CheckoutViewModel());
        }

        // ============================================================
        // Buy Now → Cookie তে সাময়িক সেভ (AJAX থেকে কল হবে)
        // URL: /Checkout/SetBuyNow
        // ============================================================
        [HttpPost]
        public IActionResult SetBuyNow([FromBody] BuyNowRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return Json(new { success = false, message = "Invalid product." });

            var product = GetAllProducts().FirstOrDefault(p => p.Id == req.ProductId);
            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            var buyNowItem = new CartItemModel
            {
                ProductId = product.Id,
                Name = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Size = req.Size ?? "",
                Quantity = req.Quantity < 1 ? 1 : req.Quantity,
                Brand = product.Brand
            };

            var json = JsonSerializer.Serialize(buyNowItem);
            Response.Cookies.Append(BuyNowCookieKey, json, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            return Json(new { success = true });
        }

        // ============================================================
        // Order Confirm (POST) — Cart & BuyNow উভয়ের জন্য
        // ============================================================
        [HttpPost]
        public IActionResult Confirm(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cart = GetCart();
                if (cart == null || cart.Count == 0)
                {
                    var buyNowItem = GetBuyNowItem();
                    if (buyNowItem != null)
                    {
                        cart = new List<CartItemModel> { buyNowItem };
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                var subtotal = cart.Sum(c => c.Price * c.Quantity);
                var delivery = 110m;

                ViewBag.CartItems = cart;
                ViewBag.Subtotal = subtotal;
                ViewBag.DeliveryCharge = delivery;
                ViewBag.Total = subtotal + delivery;

                return View("Checkout", model);
            }

            // ============================================================
            // এখানে Order Saving Logic বসান
            // ============================================================

            TempData["OrderSuccess"] = "আপনার অর্ডার সফলভাবে সম্পন্ন হয়েছে! ইনশাআল্লাহ ৩-৫ কর্মদিবসের মধ্যে ডেলিভারি করা হবে।";

            // Cart + BuyNow দুটোই ক্লিয়ার
            Response.Cookies.Delete(CartCookieKey);
            Response.Cookies.Delete(BuyNowCookieKey);

            return RedirectToAction("Success", "Checkout");
        }

        // ============================================================
        // Success পেজ
        // ============================================================
        public IActionResult Success()
        {
            if (TempData["OrderSuccess"] == null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ============================================================
        // Cookie Helpers
        // ============================================================
        private List<CartItemModel> GetCart()
        {
            if (Request.Cookies.TryGetValue(CartCookieKey, out var value)
                && !string.IsNullOrEmpty(value))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<CartItemModel>>(value)
                           ?? new List<CartItemModel>();
                }
                catch { return new List<CartItemModel>(); }
            }
            return new List<CartItemModel>();
        }

        private CartItemModel? GetBuyNowItem()
        {
            if (Request.Cookies.TryGetValue(BuyNowCookieKey, out var value)
                && !string.IsNullOrEmpty(value))
            {
                try
                {
                    return JsonSerializer.Deserialize<CartItemModel>(value);
                }
                catch { return null; }
            }
            return null;
        }

        // ============================================================
        // ডামি প্রোডাক্ট — CartController এর সাথে মিল রাখুন
        // ============================================================
        private List<Product> GetAllProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "As-Shabab Panjabi - 23", ImageUrl = "https://i.ibb.co.com/mVHcbR0p/As-Shabab-Panjabi-13.jpg", Price = 500, CategoryName = "Panjabi", Brand = "As-Shabab" },
                new Product { Id = 2, Name = "Semi Luxury Panjabi - 09", ImageUrl = "https://i.ibb.co.com/RTpJ1KLY/As-Shabab-Panjabi-09.jpg", Price = 2950, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 3, Name = "Premium Panjabi - 10", ImageUrl = "https://i.ibb.co.com/ymjnSMHt/Superior-Panjabi-08.jpg", Price = 2690, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 4, Name = "Premium Panjabi - 09", ImageUrl = "https://i.ibb.co.com/JRWPJpdn/As-Shabab-Panjabi-08.jpg", Price = 2690, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 5, Name = "Premium Panjabi - 08", ImageUrl = "https://i.ibb.co.com/5WKkqDgr/Original-Arab-s-Thobe-22.jpg", Price = 2690, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 6, Name = "Semi Luxury Panjabi Combo - 0708", ImageUrl = "https://i.ibb.co.com/cXb0F6qS/Soft-Arabian-Thobe-26.jpg", Price = 2950, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 7, Name = "Semi Luxury Panjabi Combo - 0609", ImageUrl = "https://i.ibb.co.com/hFPqhKn7/Soft-Arabian-Thobe-25.jpg", Price = 2950, CategoryName = "Panjabi", Brand = "Believers" },
                new Product { Id = 8, Name = "Superior Panjabi Combo - 0607", ImageUrl = "https://i.ibb.co.com/S4P1QVnd/Premium-Arabian-Thobe-27.jpg", Price = 2250, CategoryName = "Panjabi", Brand = "Believers" },

                new Product { Id = 9,  Name = "Emirati Thobe - 01", ImageUrl = "https://i.ibb.co.com/5X6DjR4y/Emirati-Thobe-01.jpg", Price = 2250, CategoryName = "Thobe", Brand = "Believers" },
                new Product { Id = 10, Name = "Emirati Thobe - 02", ImageUrl = "https://i.ibb.co.com/h1KQvZX4/Emirati-Thobe-02.jpg", Price = 2250, CategoryName = "Thobe", Brand = "Believers" },
                new Product { Id = 11, Name = "Emirati Thobe - 19", ImageUrl = "https://i.ibb.co.com/1f8bJ2Ny/Emirati-Thobe-19.jpg", Price = 2250, CategoryName = "Thobe", Brand = "Believers" },
                new Product { Id = 12, Name = "Emirati Thobe - 23", ImageUrl = "https://i.ibb.co.com/7JHxJDVk/Emirati-Thobe-23.jpg", Price = 2250, CategoryName = "Thobe", Brand = "Believers" },

                new Product { Id = 17, Name = "Waffle Drop Shoulder - Tawaqkul", ImageUrl = "https://i.ibb.co.com/jPFyGtNg/Waffle-Drop-Shoulder-T-shirt-Muslim.jpg", Price = 890, CategoryName = "T-Shirt", Brand = "Believers" },
                new Product { Id = 18, Name = "Waffle Drop Shoulder - Khilafah", ImageUrl = "https://i.ibb.co.com/BKy5fc43/Waffle-Drop-Shoulder-T-shirt-Khilafah.jpg", Price = 890, CategoryName = "T-Shirt", Brand = "Believers" },
                new Product { Id = 19, Name = "Waffle Drop Shoulder - Desciplined", ImageUrl = "https://i.ibb.co.com/gFVXwBjM/Waffle-Drop-Shoulder-T-shirt-Desciplined.jpg", Price = 890, CategoryName = "T-Shirt", Brand = "Believers" },
                new Product { Id = 20, Name = "Waffle Drop Shoulder - Inspired", ImageUrl = "https://i.ibb.co.com/7NXCzbFk/Waffle-Drop-Shoulder-T-shirt-Inspired.jpg", Price = 890, CategoryName = "T-Shirt", Brand = "Believers" },

                new Product { Id = 33, Name = "NB 9060 Dark Gray Replica", ImageUrl = "https://i.ibb.co.com/35qxDPsK/NB-9060-Dark-Gray-Replica.jpg", Price = 2250, CategoryName = "Sneakers", Brand = "New Balance" },
                new Product { Id = 34, Name = "NB 9060 Gray Black Replica", ImageUrl = "https://i.ibb.co.com/xqS4VNJV/NB-9060-Gray-Black-Replica.jpg", Price = 2250, CategoryName = "Sneakers", Brand = "New Balance" },
                new Product { Id = 35, Name = "China Branded Sneakers Y26-1084", ImageUrl = "https://i.ibb.co.com/pvs72Mmv/China-Branded-Sneakers-Y26-1084.jpg", Price = 1890, CategoryName = "Sneakers", Brand = "China Branded" },
                new Product { Id = 36, Name = "China Branded Sneakers Y26-1076", ImageUrl = "https://i.ibb.co.com/NdP6w6q4/China-Branded-Sneakers-Y26-1076.jpg", Price = 1890, CategoryName = "Sneakers", Brand = "China Branded" },
            };
        }

        // ============================================================
        // Models
        // ============================================================
        public class CartItemModel
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Size { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Brand { get; set; } = string.Empty;
        }
    }

    public class BuyNowRequest
    {
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; } = 1;
    }
}