using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace al_ibtisam.Controllers
{
    /// <summary>
    /// ভাষা পরিবর্তন (English ⇄ বাংলা) কন্ট্রোলার।
    /// 
    /// কাজ:
    /// ১. ইউজার "EN" বা "বাংলা" বাটনে ক্লিক করলে এই কন্ট্রোলারে আসে।
    /// ২. নির্বাচিত ভাষা ".AspNetCore.Culture" Cookie-তে সেভ হয় (১ বছরের জন্য)।
    /// ৩. তারপর ইউজারকে আগের পেজে ফেরত পাঠানো হয়।
    /// 
    /// ব্যবহার (View-এ):
    ///   /Language/Set?culture=en&amp;returnUrl=/Home/Index
    ///   /Language/Set?culture=bn&amp;returnUrl=/Product/Details/5
    /// </summary>
    public class LanguageController : Controller
    {
        // ============================================================
        // GET: /Language/Set?culture=en&returnUrl=/
        // ============================================================
        [HttpGet]
        public IActionResult Set(string? culture, string? returnUrl = null)
        {
            // ---------- ১. Culture ভ্যালিডেশন ----------
            // শুধু "en" অথবা "bn" গ্রহণ করা হবে।
            // অন্য কিছু আসলে ডিফল্ট "en" ধরে নেওয়া হবে।
            if (string.IsNullOrWhiteSpace(culture)
                || (culture != "en" && culture != "bn"))
            {
                culture = "en";
            }

            // ---------- ২. Cookie-তে ভাষা সেভ ----------
            // ".AspNetCore.Culture" নামের Cookie-তে "c=bn|uic=bn" ফরম্যাটে সেভ হবে।
            // পরবর্তী সব রিকোয়েস্টে ASP.NET Core এই Cookie পড়ে
            // Thread.CurrentThread.CurrentUICulture সেট করবে।
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,   // ".AspNetCore.Culture"
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture, culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),  // ১ বছর মনে থাকবে
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = false                              // JS থেকে পড়ার দরকার হলে
                });

            // ---------- ৩. Return URL ভ্যালিডেশন ----------
            // শুধু local URL-এ redirect করা হবে (open-redirect attack ঠেকাতে)।
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Return URL না থাকলে হোম পেজে পাঠান
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // GET: /Language/Get — বর্তমান ভাষা JSON আকারে (ঐচ্ছিক)
        // ------------------------------------------------------------
        // JS থেকে বর্তমান ভাষা জানতে চাইলে এই endpoint ব্যবহার করা যাবে।
        // উদাহরণ: fetch('/Language/Get').then(r => r.json())
        // ============================================================
        [HttpGet]
        public IActionResult Get()
        {
            var currentCulture = System.Globalization.CultureInfo
                .CurrentUICulture.TwoLetterISOLanguageName;

            return Json(new
            {
                culture = currentCulture,
                isBangla = currentCulture == "bn",
                displayName = currentCulture == "bn" ? "বাংলা" : "English"
            });
        }
    }
}