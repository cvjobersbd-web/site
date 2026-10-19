using al_ibtisam.Data;
using al_ibtisam.Models;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    // [Authorize(Roles = "Admin")] // প্রয়োজনে আনকমেন্ট করুন
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
        // ============================================================
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

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

            // ক্যাটাগরি ড্রপডাউনের জন্য DB থেকে লিস্ট
            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

            ViewBag.SelectedCategory = category;
            ViewBag.TotalCount = await _db.Products.CountAsync();

            return View(products);
        }

        // ============================================================
        // ADD PRODUCT (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

            return View(new ProductFormViewModel());
        }

        // ============================================================
        // ADD PRODUCT (POST) — File Upload সহ
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductFormViewModel model)
        {
            _logger.LogInformation("========== ADD PRODUCT ATTEMPT ==========");

            if (model.MainImageFile == null || model.MainImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.MainImageFile),
                    "মেইন ইমেজ আপলোড করতে হবে");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                // Main Image Upload
                var mainImagePath = await _imageService.UploadImageAsync(
                    model.MainImageFile!, "products");

                if (string.IsNullOrEmpty(mainImagePath))
                {
                    ModelState.AddModelError(nameof(model.MainImageFile),
                        "ইমেজ আপলোড ব্যর্থ হয়েছে (JPG/PNG/WEBP, max 5MB)।");
                    ViewBag.Categories = await _db.Categories
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => c.Name).ToListAsync();
                    return View(model);
                }

                // Gallery Upload
                var galleryPaths = new List<string>();
                if (model.GalleryFiles != null && model.GalleryFiles.Count > 0)
                {
                    galleryPaths = await _imageService.UploadMultipleImagesAsync(
                        model.GalleryFiles, "gallery");
                }

                // Size Chart Upload
                string? sizeChartPath = null;
                if (model.SizeChartFile != null && model.SizeChartFile.Length > 0)
                {
                    sizeChartPath = await _imageService.UploadImageAsync(
                        model.SizeChartFile, "sizecart");
                }

                // Product Entity
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
                ModelState.AddModelError(string.Empty,
                    "প্রোডাক্ট যোগ করতে সমস্যা হয়েছে। আবার চেষ্টা করুন।");

                ViewBag.Categories = await _db.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => c.Name).ToListAsync();
                return View(model);
            }
        }

        // ============================================================
        // EDIT PRODUCT (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

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

            ViewBag.Categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

            return View(vm);
        }

        // ============================================================
        // EDIT PRODUCT (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => c.Name).ToListAsync();
                return View(model);
            }

            var existing = await _db.Products.FindAsync(model.Id);
            if (existing == null) return NotFound();

            try
            {
                // Main Image
                if (model.MainImageFile != null && model.MainImageFile.Length > 0)
                {
                    var newPath = await _imageService.UploadImageAsync(
                        model.MainImageFile, "products");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        _imageService.DeleteImage(existing.ImageUrl);
                        existing.ImageUrl = newPath;
                    }
                }

                // Gallery (যোগ হবে পুরনোর সাথে)
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
                    }
                }

                // Size Chart
                if (model.SizeChartFile != null && model.SizeChartFile.Length > 0)
                {
                    var newSizeChart = await _imageService.UploadImageAsync(
                        model.SizeChartFile, "sizecart");
                    if (!string.IsNullOrEmpty(newSizeChart))
                    {
                        _imageService.DeleteImage(existing.SizeChartImageUrl);
                        existing.SizeChartImageUrl = newSizeChart;
                    }
                }

                // অন্যান্য ফিল্ড
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

                TempData["Success"] = "প্রোডাক্ট সফলভাবে আপডেট হয়েছে।";
                return RedirectToAction(nameof(Products),
                    new { category = existing.CategoryName });
            }
            catch (Exception ex)
            {
                _logger.LogError($"EditProduct error: {ex.Message}");
                ModelState.AddModelError(string.Empty,
                    "প্রোডাক্ট আপডেটে সমস্যা হয়েছে।");

                ViewBag.Categories = await _db.Categories
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => c.Name).ToListAsync();
                return View(model);
            }
        }

        // ============================================================
        // DELETE PRODUCT — ইমেজসহ ডিলিট
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
                    _imageService.DeleteImage(product.ImageUrl);

                    if (!string.IsNullOrEmpty(product.GalleryImagesString))
                    {
                        foreach (var img in product.GalleryImagesString
                            .Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            _imageService.DeleteImage(img.Trim());
                        }
                    }

                    _imageService.DeleteImage(product.SizeChartImageUrl);

                    _db.Products.Remove(product);
                    await _db.SaveChangesAsync();

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

            return Json(new
            {
                success = true,
                stockQuantity = newQty,
                inStock = product.InStock
            });
        }

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

            return Json(new
            {
                success = true,
                stockQuantity = quantity,
                inStock = product.InStock
            });
        }

        // ============================================================
        // 4. SHOP BY CATEGORY MANAGEMENT
        // ============================================================

        // GET: /Admin/ShopCategories
        [HttpGet]
        public async Task<IActionResult> ShopCategories()
        {
            var categories = await _db.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .ToListAsync();
            return View(categories);
        }

        // POST: /Admin/AddShopCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddShopCategory(string categoryName, IFormFile? iconFile)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "ক্যাটাগরির নাম খালি রাখা যাবে না।";
                return RedirectToAction(nameof(ShopCategories));
            }

            var name = categoryName.Trim();

            if (await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
            {
                TempData["Error"] = "এই নামে ক্যাটাগরি আগেই আছে।";
                return RedirectToAction(nameof(ShopCategories));
            }

            string? iconPath = null;
            if (iconFile != null && iconFile.Length > 0)
            {
                // ইমেজ সার্ভিস দিয়ে আপলোড
                iconPath = await _imageService.UploadImageAsync(iconFile, "category-icons");
                if (string.IsNullOrEmpty(iconPath))
                {
                    TempData["Error"] = "আইকন আপলোড ব্যর্থ হয়েছে। (সর্বোচ্চ 5MB, JPG/PNG)";
                    return RedirectToAction(nameof(ShopCategories));
                }
            }

            var maxOrder = await _db.Categories.MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

            _db.Categories.Add(new Category
            {
                Name = name,
                ShowInNavbar = true,
                DisplayOrder = maxOrder + 1,
                CreatedAt = DateTime.UtcNow,
                IconUrl = iconPath // আইকন সেভ
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"নতুন ক্যাটাগরি '{name}' সফলভাবে যোগ করা হয়েছে।";
            return RedirectToAction(nameof(ShopCategories));
        }

        // GET: /Admin/EditShopCategory/5
        [HttpGet]
        public async Task<IActionResult> EditShopCategory(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null)
            {
                TempData["Error"] = "ক্যাটাগরি পাওয়া যায়নি।";
                return RedirectToAction(nameof(ShopCategories));
            }
            return View(cat);
        }

        // POST: /Admin/EditShopCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShopCategory(int id, string categoryName, IFormFile? iconFile)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "ক্যাটাগরির নাম খালি রাখা যাবে না।";
                return RedirectToAction(nameof(EditShopCategory), new { id = id });
            }

            var name = categoryName.Trim();

            // নাম ডুপ্লিকেট চেক (নিজের নাম বাদে)
            if (await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != id))
            {
                TempData["Error"] = "এই নামে অন্য একটি ক্যাটাগরি আগেই আছে।";
                return RedirectToAction(nameof(EditShopCategory), new { id = id });
            }

            // নতুন আইকন আপলোড হলে
            if (iconFile != null && iconFile.Length > 0)
            {
                var newIconPath = await _imageService.UploadImageAsync(iconFile, "category-icons");
                if (!string.IsNullOrEmpty(newIconPath))
                {
                    // পুরনো আইকন ডিলিট করে নতুনটি সেট করা
                    if (!string.IsNullOrEmpty(cat.IconUrl))
                    {
                        _imageService.DeleteImage(cat.IconUrl);
                    }
                    cat.IconUrl = newIconPath;
                }
                else
                {
                    TempData["Error"] = "নতুন আইকন আপলোড ব্যর্থ হয়েছে।";
                    return RedirectToAction(nameof(EditShopCategory), new { id = id });
                }
            }

            cat.Name = name;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"ক্যাটাগরি '{name}' সফলভাবে আপডেট করা হয়েছে।";
            return RedirectToAction(nameof(ShopCategories));
        }

        // POST: /Admin/DeleteShopCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShopCategory(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat != null)
            {
                // চাইলে এখানে ইমেজ ডিলিট করতে পারেন
                if (!string.IsNullOrEmpty(cat.IconUrl))
                {
                    _imageService.DeleteImage(cat.IconUrl);
                }

                _db.Categories.Remove(cat);
                await _db.SaveChangesAsync();
                TempData["Success"] = "ক্যাটাগরি ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(ShopCategories));
        }

        // ============================================================
        // 5. NAVIGATION MENU MANAGEMENT
        // ============================================================

        // GET: /Admin/Navigation
        [HttpGet]
        public async Task<IActionResult> Navigation()
        {
            var menus = await _db.NavigationMenus
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();
            return View(menus);
        }

        // POST: /Admin/AddNavMenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNavMenu(string title, string url, string alignment)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "মেনুর নাম খালি রাখা যাবে না।";
                return RedirectToAction(nameof(Navigation));
            }

            var maxOrder = await _db.NavigationMenus.MaxAsync(m => (int?)m.DisplayOrder) ?? 0;

            _db.NavigationMenus.Add(new NavigationMenu
            {
                Title = title.Trim(),
                Url = string.IsNullOrWhiteSpace(url) ? "#" : url.Trim(),
                Alignment = alignment,
                DisplayOrder = maxOrder + 1,
                IsActive = true
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"নতুন মেনু '{title}' যোগ করা হয়েছে।";
            return RedirectToAction(nameof(Navigation));
        }

        // POST: /Admin/EditNavMenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNavMenu(int id, string title, string url, string alignment)
        {
            var menu = await _db.NavigationMenus.FindAsync(id);
            if (menu == null) return NotFound();

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "মেনুর নাম খালি রাখা যাবে না।";
                return RedirectToAction(nameof(Navigation));
            }

            menu.Title = title.Trim();
            menu.Url = string.IsNullOrWhiteSpace(url) ? "#" : url.Trim();
            menu.Alignment = alignment;

            await _db.SaveChangesAsync();
            TempData["Success"] = "মেনু আপডেট করা হয়েছে।";
            return RedirectToAction(nameof(Navigation));
        }

        // POST: /Admin/ToggleNavMenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNavMenu(int id)
        {
            var menu = await _db.NavigationMenus.FindAsync(id);
            if (menu != null)
            {
                menu.IsActive = !menu.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"মেনু '{menu.Title}' {(menu.IsActive ? "সক্রিয়" : "নিষ্ক্রিয়")} করা হয়েছে।";
            }
            return RedirectToAction(nameof(Navigation));
        }

        // POST: /Admin/DeleteNavMenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNavMenu(int id)
        {
            var menu = await _db.NavigationMenus.FindAsync(id);
            if (menu != null)
            {
                _db.NavigationMenus.Remove(menu);
                await _db.SaveChangesAsync();
                TempData["Success"] = "মেনু ডিলিট করা হয়েছে।";
            }
            return RedirectToAction(nameof(Navigation));
        }

        // ============================================================
        // 6. SIGN OUT
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