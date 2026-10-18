using al_ibtisam.Data;
using al_ibtisam.Models;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace al_ibtisam.Controllers
{
    /// <summary>
    /// হোম পেইজের কন্ট্রোলার।
    /// হিরো স্লাইডার ডাটাবেজ (HeroSlides) থেকে লোড করে।
    /// Slider interval (সেকেন্ড) SiteSettings থেকে আসে।
    /// </summary>
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;
        private readonly ISiteSettingService _siteSettings;

        // ============================================================
        // Constructor — Dependency Injection
        // ============================================================
        public HomeController(
            AppDbContext db,
            ILogger<HomeController> logger,
            ISiteSettingService siteSettings)
        {
            _db = db;
            _logger = logger;
            _siteSettings = siteSettings;
        }

        // ============================================================
        // HOME PAGE — হিরো স্লাইডার + interval লোড
        // URL: /
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // ---------- 1. Active slides (ordered) ----------
                var slides = await _db.HeroSlides
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Id)
                    .ToListAsync();

                // ---------- 2. Slider interval (সেকেন্ড) ----------
                var interval = await _siteSettings.GetSliderIntervalAsync();

                // ---------- 3. ViewBag-এ পাঠান ----------
                ViewBag.HeroSlides = slides;
                ViewBag.SliderInterval = interval;            // সেকেন্ড
                ViewBag.SliderIntervalSeconds = interval * 1000; // ms (Bootstrap data-bs-interval)

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Home/Index error: {ex.Message}");

                // Error হলেও হোম পেইজ খুলবে — empty slider
                ViewBag.HeroSlides = new List<HeroSlide>();
                ViewBag.SliderInterval = 3;
                ViewBag.SliderIntervalSeconds = 3000;

                return View();
            }
        }

        // ============================================================
        // PRIVACY
        // URL: /Home/Privacy
        // ============================================================
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // ============================================================
        // ERROR PAGE
        // URL: /Home/Error
        // ============================================================
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}