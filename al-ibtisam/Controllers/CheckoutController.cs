using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace al_ibtisam.Controllers
{
    [IgnoreAntiforgeryToken]
    public class CheckoutController : Controller
    {
        private const string CartCookieKey = "MyCart";

        // ============================================================
        // Checkout পেজ (GET)
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

            return View(new CheckoutViewModel());
        }

        // ============================================================
        // Order Confirm (POST)
        // URL: /Checkout/Confirm
        // ============================================================
        [HttpPost]
        public IActionResult Confirm(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cart = GetCart();
                var subtotal = cart.Sum(c => c.Price * c.Quantity);
                var delivery = 110m;

                ViewBag.CartItems = cart;
                ViewBag.Subtotal = subtotal;
                ViewBag.DeliveryCharge = delivery;
                ViewBag.Total = subtotal + delivery;

                return View("Checkout", model);
            }

            // ============================================================
            // এখানে আপনার আসল Order Saving Logic বসান
            // উদাহরণ:
            //   1. একটি Order entity তৈরি করুন (নাম, ফোন, জেলা, ঠিকানা, টোটাল ইত্যাদি)
            //   2. প্রতিটি CartItem এর জন্য OrderItem entity তৈরি করুন
            //   3. Database এ save করুন (EF Core / Dapper)
            //   4. Order ID গ্রাহককে দিন / SMS পাঠান
            // ============================================================

            // ডামি: অর্ডার সফল
            TempData["OrderSuccess"] = "আপনার অর্ডার সফলভাবে সম্পন্ন হয়েছে! ইনশাআল্লাহ ৩-৫ কর্মদিবসের মধ্যে ডেলিভারি করা হবে।";

            // Cart Cookie ক্লিয়ার করুন
            Response.Cookies.Delete(CartCookieKey);

            return RedirectToAction("Success", "Checkout");
        }

        // ============================================================
        // Order Success পেজ (GET)
        // URL: /Checkout/Success
        // ============================================================
        public IActionResult Success()
        {
            // যদি সরাসরি URL দিয়ে আসে (TempData খালি), তাহলে হোমে পাঠান
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ============================================================
        // Cookie থেকে Cart লোড
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
                catch
                {
                    return new List<CartItemModel>();
                }
            }
            return new List<CartItemModel>();
        }

        // ============================================================
        // CartItemModel — CartController এর সাথে মিল থাকতে হবে
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
}