using al_ibtisam.Data;
using al_ibtisam.Models;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    // [Authorize(Roles = "Admin")] // প্রয়োজনে আনকমেন্ট করুন
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminController> _logger;
        private readonly IImageService _imageService;

        // ============================================================
        // Constructor — Dependency Injection
        // ============================================================
        public AdminController(
            AppDbContext db,
            ILogger<AdminController> logger,
            IImageService imageService)
        {
            _db = db;
            _logger = logger;
            _imageService = imageService;
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
            ViewBag.TodayLogins = users.Count(u =>
                u.LastLoginAt.HasValue &&
                u.LastLoginAt.Value.Date == DateTime.UtcNow.Date);

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
                TempData["Success"] = "ইউজারের স্ট্যাটাস আপডেট করা হয়েছে।";
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
                TempData["Success"] = "ইউজার ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Users));
        }

        // ============================================================
        // 3. PRODUCTS MANAGEMENT
        // ============================================================

        // GET: /Admin/Products?category=Panjabi
        public async Task<IActionResult> Products(string? category = null)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.CategoryName == category);
            }

            var products = await query
                .OrderBy(p => p.Id)
                .ToListAsync();

            ViewBag.Categories = CategoryStore.GetAll();
            ViewBag.SelectedCategory = category;
            ViewBag.TotalCount = await _db.Products.CountAsync();

            return View(products);
        }

        // ============================================================
        // ADD PRODUCT (GET)
        // URL: /Admin/AddProduct
        // ============================================================
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewBag.Categories = CategoryStore.GetAll();
            return View(new ProductFormViewModel());
        }

        // ============================================================
        // ADD PRODUCT (POST) — File Upload সহ
        // URL: /Admin/AddProduct
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductFormViewModel model)
        {
            _logger.LogInformation("========== ADD PRODUCT ATTEMPT ==========");
            _logger.LogInformation($"Name: {model.Name}, Category: {model.CategoryName}");

            // ---------- মেইন ইমেজ বাধ্যতামূলক চেক ----------
            if (model.MainImageFile == null || model.MainImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.MainImageFile),
                    "মেইন ইমেজ আপলোড করতে হবে");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = CategoryStore.GetAll();
                return View(model);
            }

            try
            {
                // ---------- 1. Main Image Upload ----------
                var mainImagePath = await _imageService.UploadImageAsync(
                    model.MainImageFile!, "products");

                if (string.IsNullOrEmpty(mainImagePath))
                {
                    ModelState.AddModelError(nameof(model.MainImageFile),
                        "ইমেজ আপলোড ব্যর্থ হয়েছে। সঠিক ফাইল দিন (JPG/PNG/WEBP, সর্বোচ্চ 5MB)।");
                    ViewBag.Categories = CategoryStore.GetAll();
                    return View(model);
                }

                // ---------- 2. Gallery Upload ----------
                var galleryPaths = new List<string>();
                if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                {
                    galleryPaths = await _imageService.UploadMultipleImagesAsync(
                        model.GalleryFiles, "gallery");
                }

                // ---------- 3. Size Chart Upload ----------
                string? sizeChartPath = null;
                if (model.SizeChartFile != null && model.SizeChartFile.Length > 0)
                {
                    sizeChartPath = await _imageService.UploadImageAsync(
                        model.SizeChartFile, "sizecart");
                }

                // ---------- 4. Product Entity তৈরি ----------
                var product = new Product
                {
                    Name = model.Name.Trim(),
                    CategoryName = model.CategoryName,
                    Brand = string.IsNullOrWhiteSpace(model.Brand)
                        ? "Believers" : model.Brand.Trim(),
                    Price = model.Price,
                    OldPrice = model.OldPrice,
                    DiscountPercent = model.DiscountPercent,
                    StockQuantity = model.StockQuantity,
                    InStock = model.StockQuantity > 0,
                    ImageUrl = mainImagePath,
                    GalleryImagesString = galleryPaths.Count > 0
                        ? string.Join(",", galleryPaths) : null,
                    SizeChartImageUrl = sizeChartPath,
                    SizesString = string.IsNullOrWhiteSpace(model.SizesString)
                        ? "M,L,XL,XXL" : model.SizesString,
                    ShortDescription = model.ShortDescription?.Trim(),
                    KeyFeaturesString = model.KeyFeaturesString?.Trim(),
                    WhyYouWillLoveItString = model.WhyYouWillLoveItString?.Trim(),
                    ColorDisclaimer = model.ColorDisclaimer?.Trim()
                };

                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ Product added! ID = {product.Id}");

                TempData["Success"] = "নতুন প্রোডাক্ট সফলভাবে যোগ করা হয়েছে।";
                return RedirectToAction(nameof(Products),
                    new { category = product.CategoryName });
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddProduct error: {ex.Message}");
                _logger.LogError($"Inner: {ex.InnerException?.Message}");

                ModelState.AddModelError(string.Empty,
                    "প্রোডাক্ট যোগ করতে সমস্যা হয়েছে। আবার চেষ্টা করুন।");

                ViewBag.Categories = CategoryStore.GetAll();
                return View(model);
            }
        }

        // ============================================================
        // EDIT PRODUCT (GET)
        // URL: /Admin/EditProduct/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Entity → ViewModel ম্যাপিং
            var vm = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryName = product.CategoryName,
                Brand = product.Brand,
                Price = product.Price,
                OldPrice = product.OldPrice,
                DiscountPercent = product.DiscountPercent,
                StockQuantity = product.StockQuantity,
                SizesString = product.SizesString,
                ShortDescription = product.ShortDescription,
                KeyFeaturesString = product.KeyFeaturesString,
                WhyYouWillLoveItString = product.WhyYouWillLoveItString,
                ColorDisclaimer = product.ColorDisclaimer,
                ExistingImageUrl = product.ImageUrl,
                ExistingSizeChartUrl = product.SizeChartImageUrl,
                ExistingGalleryImagesString = product.GalleryImagesString
            };

            ViewBag.Categories = CategoryStore.GetAll();
            return View(vm);
        }

        // ============================================================
        // EDIT PRODUCT (POST) — File Upload সহ
        // URL: /Admin/EditProduct
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductFormViewModel model)
        {
            _logger.LogInformation("========== EDIT PRODUCT ATTEMPT ==========");
            _logger.LogInformation($"ID: {model.Id}, Name: {model.Name}");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = CategoryStore.GetAll();
                return View(model);
            }

            var existing = await _db.Products.FindAsync(model.Id);
            if (existing == null) return NotFound();

            try
            {
                // ---------- 1. Main Image (নতুন দিলে পুরনো ডিলিট) ----------
                if (model.MainImageFile != null && model.MainImageFile.Length > 0)
                {
                    var newPath = await _imageService.UploadImageAsync(
                        model.MainImageFile, "products");

                    if (!string.IsNullOrEmpty(newPath))
                    {
                        _imageService.DeleteImage(existing.ImageUrl);
                        existing.ImageUrl = newPath;
                        _logger.LogInformation($"Main image updated: {newPath}");
                    }
                }

                // ---------- 2. Gallery (নতুনগুলো পুরনোর সাথে যোগ হবে) ----------
                if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                {
                    var newGallery = await _imageService.UploadMultipleImagesAsync(
                        model.GalleryFiles, "gallery");

                    if (newGallery.Count > 0)
                    {
                        var existingList = string.IsNullOrEmpty(existing.GalleryImagesString)
                            ? new List<string>()
                            : existing.GalleryImagesString
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .ToList();

                        existingList.AddRange(newGallery);
                        existing.GalleryImagesString = string.Join(",", existingList);

                        _logger.LogInformation(
                            $"Gallery updated: {newGallery.Count} new images added");
                    }
                }

                // ---------- 3. Size Chart ----------
                if (model.SizeChartFile != null && model.SizeChartFile.Length > 0)
                {
                    var newSizeChart = await _imageService.UploadImageAsync(
                        model.SizeChartFile, "sizecart");

                    if (!string.IsNullOrEmpty(newSizeChart))
                    {
                        _imageService.DeleteImage(existing.SizeChartImageUrl);
                        existing.SizeChartImageUrl = newSizeChart;
                        _logger.LogInformation($"Size chart updated: {newSizeChart}");
                    }
                }

                // ---------- 4. অন্যান্য ফিল্ড আপডেট ----------
                existing.Name = model.Name.Trim();
                existing.CategoryName = model.CategoryName;
                existing.Brand = string.IsNullOrWhiteSpace(model.Brand)
                    ? "Believers" : model.Brand.Trim();
                existing.Price = model.Price;
                existing.OldPrice = model.OldPrice;
                existing.DiscountPercent = model.DiscountPercent;
                existing.StockQuantity = model.StockQuantity;
                existing.InStock = model.StockQuantity > 0;
                existing.SizesString = string.IsNullOrWhiteSpace(model.SizesString)
                    ? "M,L,XL,XXL" : model.SizesString;
                existing.ShortDescription = model.ShortDescription?.Trim();
                existing.KeyFeaturesString = model.KeyFeaturesString?.Trim();
                existing.WhyYouWillLoveItString = model.WhyYouWillLoveItString?.Trim();
                existing.ColorDisclaimer = model.ColorDisclaimer?.Trim();

                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ Product updated! ID = {existing.Id}");

                TempData["Success"] = "প্রোডাক্ট সফলভাবে আপডেট হয়েছে।";
                return RedirectToAction(nameof(Products),
                    new { category = existing.CategoryName });
            }
            catch (Exception ex)
            {
                _logger.LogError($"EditProduct error: {ex.Message}");
                _logger.LogError($"Inner: {ex.InnerException?.Message}");

                ModelState.AddModelError(string.Empty,
                    "প্রোডাক্ট আপডেটে সমস্যা হয়েছে। আবার চেষ্টা করুন।");

                ViewBag.Categories = CategoryStore.GetAll();
                return View(model);
            }
        }

        // ============================================================
        // DELETE PRODUCT (POST) — ইমেজসহ ডিলিট
        // URL: /Admin/DeleteProduct
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);

            if (product != null)
            {
                try
                {
                    // ---------- 1. Main image ডিলিট ----------
                    _imageService.DeleteImage(product.ImageUrl);

                    // ---------- 2. Gallery images ডিলিট ----------
                    if (!string.IsNullOrEmpty(product.GalleryImagesString))
                    {
                        foreach (var img in product.GalleryImagesString
                            .Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            _imageService.DeleteImage(img.Trim());
                        }
                    }

                    // ---------- 3. Size chart ডিলিট ----------
                    _imageService.DeleteImage(product.SizeChartImageUrl);

                    // ---------- 4. ডাটাবেজ থেকে রিমুভ ----------
                    _db.Products.Remove(product);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"✅ Product deleted! ID = {id}");

                    TempData["Success"] = "প্রোডাক্ট এবং এর সব ইমেজ ডিলিট করা হয়েছে।";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteProduct error: {ex.Message}");
                    TempData["Error"] = "প্রোডাক্ট ডিলিট করতে সমস্যা হয়েছে।";
                }
            }

            return RedirectToAction(nameof(Products));
        }

        // ============================================================
        // 3.1 STOCK MANAGEMENT (AJAX)
        // ============================================================

        // POST: /Admin/UpdateStock?id=5&delta=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int delta)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            var newQty = product.StockQuantity + delta;
            if (newQty < 0) newQty = 0;

            product.StockQuantity = newQty;
            product.InStock = newQty > 0;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Stock updated: Product #{id} → {newQty}");

            return Json(new
            {
                success = true,
                stockQuantity = newQty,
                inStock = product.InStock
            });
        }

        // POST: /Admin/SetStock?id=5&quantity=20
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStock(int id, int quantity)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            if (quantity < 0) quantity = 0;

            product.StockQuantity = quantity;
            product.InStock = quantity > 0;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Stock set: Product #{id} → {quantity}");

            return Json(new
            {
                success = true,
                stockQuantity = quantity,
                inStock = product.InStock
            });
        }

        // ============================================================
        // 4. CATEGORIES MANAGEMENT
        // URL: /Admin/Categories
        // ============================================================
        public async Task<IActionResult> Categories()
        {
            var categories = CategoryStore.GetAll();
            var products = await _db.Products.ToListAsync();

            var categoriesData = categories.Select(catName => new
            {
                Name = catName,
                ProductCount = products.Count(p =>
                    !string.IsNullOrEmpty(p.CategoryName) &&
                    p.CategoryName.Equals(catName, StringComparison.OrdinalIgnoreCase))
            }).ToList();

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
                CategoryStore.Add(categoryName);
                TempData["Success"] = "নতুন ক্যাটাগরি যোগ করা হয়েছে।";
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
                CategoryStore.Delete(categoryName);
                TempData["Success"] = "ক্যাটাগরি ডিলিট করা হয়েছে।";
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