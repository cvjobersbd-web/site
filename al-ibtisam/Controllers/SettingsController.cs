using al_ibtisam.Data;
using al_ibtisam.Models;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Controllers
{
    /// <summary>
    /// অ্যাডমিন প্যানেলের Settings পেজ।
    /// এখানে হোম পেইজের হিরো স্লাইডার (ইমেজ, active/inactive, interval) এবং 
    /// সম্পূর্ণ সাইটের থিম কালার (Customize Site) ম্যানেজ করা হয়।
    /// </summary>
    public class SettingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SettingsController> _logger;
        private readonly IImageService _imageService;
        private readonly ISiteSettingService _siteSettings;

        private const int MinInterval = 1;
        private const int MaxInterval = 60;

        // View নাম constant — বার বার ব্যবহারের জন্য
        private const string SettingsView = "Settings";

        // ============================================================
        // Constructor — Dependency Injection
        // ============================================================
        public SettingsController(
            AppDbContext db,
            ILogger<SettingsController> logger,
            IImageService imageService,
            ISiteSettingService siteSettings)
        {
            _db = db;
            _logger = logger;
            _imageService = imageService;
            _siteSettings = siteSettings;
        }

        // ============================================================
        // Helper: অ্যাডমিন কিনা যাচাই
        // ============================================================
        private bool IsAdmin()
        {
            return User.Identity != null
                   && User.Identity.IsAuthenticated
                   && User.IsInRole("Admin");
        }

        // ============================================================
        // 1. SLIDER SETTINGS (GET)
        // URL: /Settings/Index
        // View: Views/Settings/Settings.cshtml
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account",
                    new { returnUrl = "/Settings/Index" });

            try
            {
                var interval = await _siteSettings.GetSliderIntervalAsync();

                var slides = await _db.HeroSlides
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Id)
                    .ToListAsync();

                ViewBag.SliderInterval = interval;

                // ✅ View name explicitly "Settings" — কারণ ফাইল নাম Settings.cshtml
                return View(SettingsView, slides);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Settings/Index error: {ex.Message}");
                TempData["Error"] = "সেটিংস লোড করতে সমস্যা হয়েছে।";

                return View(SettingsView, new List<HeroSlide>());
            }
        }

        // ============================================================
        // 2. UPDATE SLIDER INTERVAL (POST)
        // URL: /Settings/UpdateSliderInterval
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSliderInterval(int interval)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            // ---------- Range clamp ----------
            if (interval < MinInterval) interval = MinInterval;
            if (interval > MaxInterval) interval = MaxInterval;

            try
            {
                await _siteSettings.SetValueAsync(
                    SiteSettingService.HeroSliderIntervalKey,
                    interval.ToString());

                TempData["Success"] =
                    $"স্লাইডার ইন্টারভ্যাল সফলভাবে {interval} সেকেন্ডে সেট হয়েছে।";
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateSliderInterval error: {ex.Message}");
                TempData["Error"] = "ইন্টারভ্যাল সেভ করতে সমস্যা হয়েছে।";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 3. CUSTOMIZE SITE - THEME COLORS (GET)
        // URL: /Settings/Customize
        // View: Views/Settings/Customize.cshtml
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Customize()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account", new { returnUrl = "/Settings/Customize" });

            var colors = await _siteSettings.GetThemeColorsAsync();
            return View(colors);
        }

        // ============================================================
        // 4. CUSTOMIZE SITE - THEME COLORS (POST)
        // URL: /Settings/Customize
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customize(
            string primaryColor,
            string secondaryColor,
            string buttonColor,
            string priceColor,
            string bgColor)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            try
            {
                if (!string.IsNullOrEmpty(primaryColor))
                    await _siteSettings.SetValueAsync(SiteSettingService.ThemePrimaryColor, primaryColor);

                if (!string.IsNullOrEmpty(secondaryColor))
                    await _siteSettings.SetValueAsync(SiteSettingService.ThemeSecondaryColor, secondaryColor);

                if (!string.IsNullOrEmpty(buttonColor))
                    await _siteSettings.SetValueAsync(SiteSettingService.ThemeButtonColor, buttonColor);

                if (!string.IsNullOrEmpty(priceColor))
                    await _siteSettings.SetValueAsync(SiteSettingService.ThemePriceColor, priceColor);

                if (!string.IsNullOrEmpty(bgColor))
                    await _siteSettings.SetValueAsync(SiteSettingService.ThemeBgColor, bgColor);

                TempData["Success"] = "সাইটের কালার সফলভাবে আপডেট করা হয়েছে!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Customize error: {ex.Message}");
                TempData["Error"] = "কালার সেভ করতে সমস্যা হয়েছে।";
            }

            return RedirectToAction(nameof(Customize));
        }

        // ============================================================
        // 5. ADD SLIDE (GET)
        // URL: /Settings/AddSlide
        // ============================================================
        [HttpGet]
        public IActionResult AddSlide()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            return View(new HeroSlide());
        }

        // ============================================================
        // 6. ADD SLIDE (POST) — File Upload
        // URL: /Settings/AddSlide
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSlide(
            [Bind("IsActive")] HeroSlide model,
            IFormFile? imageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            // ✅ IMPORTANT: ImageUrl রিকোয়ার্ড ভ্যালিডেশন ইগনোর করুন
            ModelState.Remove("ImageUrl");

            // ---------- Image required ----------
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "স্লাইডার ইমেজ আপলোড করতে হবে");
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // ---------- Upload Image ----------
                var imagePath = await _imageService.UploadImageAsync(imageFile!, "slides");

                if (string.IsNullOrEmpty(imagePath))
                {
                    ModelState.AddModelError("imageFile",
                        "ইমেজ আপলোড ব্যর্থ (JPG/PNG/WEBP, সর্বোচ্চ 5MB)।");
                    return View(model);
                }

                // ---------- Auto Display Order ----------
                var maxOrder = await _db.HeroSlides
                    .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

                model.ImageUrl = imagePath;
                model.DisplayOrder = maxOrder + 1;
                model.CreatedAt = DateTime.UtcNow;

                _db.HeroSlides.Add(model);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ HeroSlide added! ID = {model.Id}");

                TempData["Success"] = "নতুন স্লাইড সফলভাবে যোগ হয়েছে।";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddSlide error: {ex.Message}");
                ModelState.AddModelError("", "স্লাইড যোগ করতে সমস্যা হয়েছে। আবার চেষ্টা করুন।");
                return View(model);
            }
        }

        // ============================================================
        // 7. EDIT SLIDE (GET)
        // URL: /Settings/EditSlide/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditSlide(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            var slide = await _db.HeroSlides.FindAsync(id);
            if (slide == null)
                return NotFound();

            return View(slide);
        }

        // ============================================================
        // 8. EDIT SLIDE (POST)
        // URL: /Settings/EditSlide
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSlide(
            [Bind("Id,IsActive")] HeroSlide model,
            IFormFile? imageFile)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            // ✅ IMPORTANT: ImageUrl ভ্যালিডেশন ইগনোর করুন
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
                return View(model);

            var existing = await _db.HeroSlides.FindAsync(model.Id);
            if (existing == null)
                return NotFound();

            try
            {
                // ---------- New Image (ঐচ্ছিক) ----------
                if (imageFile != null && imageFile.Length > 0)
                {
                    var newPath = await _imageService.UploadImageAsync(imageFile, "slides");

                    if (!string.IsNullOrEmpty(newPath))
                    {
                        // পুরনো ইমেজ ডিলিট, নতুন সেট
                        _imageService.DeleteImage(existing.ImageUrl);
                        existing.ImageUrl = newPath;
                    }
                    else
                    {
                        ModelState.AddModelError("imageFile",
                            "ইমেজ আপলোড ব্যর্থ (JPG/PNG/WEBP, সর্বোচ্চ 5MB)।");
                        model.ImageUrl = existing.ImageUrl;
                        return View(model);
                    }
                }

                // ---------- Update Fields ----------
                existing.IsActive = model.IsActive;

                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ HeroSlide updated! ID = {existing.Id}");

                TempData["Success"] = "স্লাইড সফলভাবে আপডেট হয়েছে।";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"EditSlide error: {ex.Message}");
                ModelState.AddModelError("", "স্লাইড আপডেটে সমস্যা হয়েছে। আবার চেষ্টা করুন।");

                model.ImageUrl = existing.ImageUrl;
                return View(model);
            }
        }

        // ============================================================
        // 9. TOGGLE ACTIVE (POST) — AJAX
        // URL: /Settings/ToggleActive?id=5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var slide = await _db.HeroSlides.FindAsync(id);
                if (slide == null)
                    return Json(new { success = false, message = "Slide not found" });

                slide.IsActive = !slide.IsActive;
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    isActive = slide.IsActive,
                    message = slide.IsActive ? "স্লাইড সক্রিয় করা হয়েছে" : "স্লাইড নিষ্ক্রিয় করা হয়েছে"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ToggleActive error: {ex.Message}");
                return Json(new { success = false, message = "সমস্যা হয়েছে" });
            }
        }

        // ============================================================
        // 10. DELETE SLIDE (POST)
        // URL: /Settings/DeleteSlide
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlide(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Account");

            try
            {
                var slide = await _db.HeroSlides.FindAsync(id);
                if (slide == null)
                {
                    TempData["Error"] = "স্লাইড পাওয়া যায়নি।";
                    return RedirectToAction(nameof(Index));
                }

                // ---------- ডাটাবেজ থেকে মুছে ফেলুন ----------
                _db.HeroSlides.Remove(slide);
                await _db.SaveChangesAsync();

                // ---------- তারপর ফাইল মুছুন ----------
                _imageService.DeleteImage(slide.ImageUrl);

                _logger.LogInformation($"🗑️ HeroSlide deleted! ID = {id}");

                TempData["Success"] = "স্লাইড ডিলিট করা হয়েছে।";
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteSlide error: {ex.Message}");
                TempData["Error"] = "স্লাইড ডিলিট করতে সমস্যা হয়েছে।";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}