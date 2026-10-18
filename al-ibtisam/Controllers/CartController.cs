using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using System.Text.Json;

namespace al_ibtisam.Controllers
{
    [IgnoreAntiforgeryToken]
    public class CartController : Controller
    {
        private const string CartCookieKey = "MyCart";

        private readonly AppDbContext _db;
        private readonly ILogger<CartController> _logger;

        public CartController(AppDbContext db, ILogger<CartController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================================
        // CartItem মডেল
        // ============================================================
        public class CartItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Size { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Brand { get; set; } = string.Empty;
        }

        // ============================================================
        // Add to Cart (AJAX)
        // URL: POST /Cart/Add
        // ============================================================
        [HttpPost]
        public IActionResult Add([FromBody] AddToCartRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return Json(new { success = false, message = "Invalid product." });

            // ডাটাবেজ থেকে প্রোডাক্ট খোঁজা
            var product = _db.Products.FirstOrDefault(p => p.Id == req.ProductId);
            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            // স্টক চেক
            if (product.StockQuantity <= 0)
                return Json(new { success = false, message = "Out of stock." });

            var cart = GetCart();

            var existing = cart.FirstOrDefault(c => c.ProductId == req.ProductId && c.Size == req.Size);
            if (existing != null)
            {
                // স্টক লিমিট ছাড়িয়ে যাচ্ছে কিনা চেক
                var newQty = existing.Quantity + req.Quantity;
                if (newQty > product.StockQuantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Only {product.StockQuantity} items available in stock."
                    });
                }
                existing.Quantity = newQty;
            }
            else
            {
                if (req.Quantity > product.StockQuantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Only {product.StockQuantity} items available in stock."
                    });
                }

                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Size = req.Size ?? "",
                    Quantity = req.Quantity,
                    Brand = product.Brand
                });
            }

            SaveCart(cart);

            return Json(new
            {
                success = true,
                count = cart.Sum(c => c.Quantity),
                message = "Added to cart"
            });
        }

        // ============================================================
        // Cart Items (AJAX)
        // URL: GET /Cart/Items
        // ============================================================
        [HttpGet]
        public IActionResult Items()
        {
            var cart = GetCart();
            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            return Json(new
            {
                success = true,
                count = cart.Sum(c => c.Quantity),
                subtotal = subtotal,
                items = cart
            });
        }

        // ============================================================
        // Remove item
        // URL: POST /Cart/Remove
        // ============================================================
        [HttpPost]
        public IActionResult Remove([FromBody] AddToCartRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return Json(new { success = false });

            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == req.ProductId && c.Size == req.Size);
            SaveCart(cart);

            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            return Json(new
            {
                success = true,
                count = cart.Sum(c => c.Quantity),
                subtotal = subtotal
            });
        }

        // ============================================================
        // Update quantity
        // URL: POST /Cart/UpdateQty
        // ============================================================
        [HttpPost]
        public IActionResult UpdateQty([FromBody] AddToCartRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return Json(new { success = false });

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == req.ProductId && c.Size == req.Size);
            if (item != null)
            {
                var newQty = req.Quantity < 1 ? 1 : req.Quantity;

                // স্টক লিমিট চেক
                var product = _db.Products.FirstOrDefault(p => p.Id == req.ProductId);
                if (product != null && newQty > product.StockQuantity)
                {
                    newQty = product.StockQuantity;
                    if (newQty < 1) newQty = 1;
                }

                item.Quantity = newQty;
            }
            SaveCart(cart);

            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            return Json(new
            {
                success = true,
                count = cart.Sum(c => c.Quantity),
                subtotal = subtotal
            });
        }

        // ============================================================
        // Cookie Helpers
        // ============================================================
        private List<CartItem> GetCart()
        {
            if (Request.Cookies.TryGetValue(CartCookieKey, out var value)
                && !string.IsNullOrEmpty(value))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<CartItem>>(value) ?? new List<CartItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Cart deserialize error: {ex.Message}");
                    return new List<CartItem>();
                }
            }
            return new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            Response.Cookies.Append(CartCookieKey, json, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
        }
    }

    // ============================================================
    // Request Model
    // ============================================================
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; } = 1;
    }
}