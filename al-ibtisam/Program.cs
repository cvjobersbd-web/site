using al_ibtisam.Data;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;          // 🆕 Localization
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;              // 🆕 Localization options
using System.Globalization;                      // 🆕 CultureInfo

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. PostgreSQL DbContext Register
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });

    // Development-এ detailed error দেখানোর জন্য
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// ============================================================
// 2. Cookie Authentication
// ============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // ---------- Paths ----------
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/SignOut";
        options.AccessDeniedPath = "/Account/SignIn";
        options.ReturnUrlParameter = "returnUrl";

        // ---------- Expiration ----------
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        // ---------- Cookie Settings ----------
        options.Cookie.Name = "AlIbtisam.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// ============================================================
// 3. MVC / Controllers with Views
// ============================================================
builder.Services.AddControllersWithViews();

// ============================================================
// 3.1 🆕 Localization — দুই ভাষা সাপোর্ট (English + বাংলা)
// ------------------------------------------------------------
// - ResourcesPath = "Resources" → .resx ফাইলগুলো Resources/ ফোল্ডারে
// - Supported: "en" (English), "bn" (বাংলা)
// - Default: "en"
// - ভাষা মনে রাখা হয় ".AspNetCore.Culture" Cookie-তে (১ বছর)
// ============================================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("bn")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Cookie থেকে ভাষা পড়বে (সবার আগে চেক করবে)
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = ".AspNetCore.Culture"
    });
});

// ============================================================
// 4. Image Upload Service
// ============================================================
builder.Services.AddScoped<IImageService, ImageService>();

// ============================================================
// 5. Site Setting Service (স্লাইডার interval ইত্যাদির জন্য)
// ============================================================
builder.Services.AddScoped<ISiteSettingService, SiteSettingService>();

// ============================================================
// 6. Session
// ============================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "AlIbtisam.Session";
});

// ============================================================
// 7. HttpContextAccessor
// ============================================================
builder.Services.AddHttpContextAccessor();

// ============================================================
// 8. Antiforgery (CSRF Protection)
// ============================================================
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "AlIbtisam.Antiforgery";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// ============================================================
// 9. অ্যাপ স্টার্টআপে ফোল্ডার তৈরি + DB চেক + Categories/SiteSettings চেক
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // ---------- 9.1 Uploads ফোল্ডার অটো তৈরি ----------
    try
    {
        var imageService = services.GetRequiredService<IImageService>();
        imageService.EnsureUploadFolders();
        Console.WriteLine("✅ Upload folders are ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Upload folder creation error: " + ex.Message);
    }

    // ---------- 9.2 Database কানেকশন চেক ----------
    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        // যেহেতু pgAdmin দিয়ে ম্যানুয়ালি টেবিল তৈরি করা হয়েছে, তাই migration বন্ধ
        // db.Database.Migrate();

        if (db.Database.CanConnect())
        {
            Console.WriteLine("✅ Database connected successfully.");
        }
        else
        {
            Console.WriteLine("❌ Database connection failed. Check your connection string.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Database initialization error: " + ex.Message);
        Console.WriteLine("👉 appsettings.json-এ Connection String চেক করুন।");

        if (ex.InnerException != null)
        {
            Console.WriteLine("👉 Inner Error: " + ex.InnerException.Message);
        }
    }

    // ---------- 9.3 Categories টেবিল চেক ----------
    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        var categoryCount = db.Categories.Count();

        if (categoryCount == 0)
        {
            Console.WriteLine("⚠️ Categories টেবিল খালি — SQL script দিয়ে ডাটা ইনসার্ট করুন।");
            Console.WriteLine("👉 pgAdmin-এ Categories SQL script চালান।");
        }
        else
        {
            Console.WriteLine($"✅ Categories loaded: {categoryCount} টি");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ Categories টেবিল পাওয়া যায়নি: " + ex.Message);
        Console.WriteLine("👉 pgAdmin-এ Categories টেবিল তৈরি করার SQL script চালান।");
    }

    // ---------- 9.4 HeroSlides টেবিল চেক ----------
    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        var slideCount = db.HeroSlides.Count();
        Console.WriteLine($"✅ HeroSlides loaded: {slideCount} টি স্লাইড");
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ HeroSlides টেবিল পাওয়া যায়নি: " + ex.Message);
        Console.WriteLine("👉 pgAdmin-এ HeroSlides টেবিল তৈরি করার SQL script চালান।");
    }

    // ---------- 9.5 SiteSettings টেবিল চেক + ডিফল্ট তৈরি ----------
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var siteSettings = services.GetRequiredService<ISiteSettingService>();

        // টেবিল আছে কিনা চেক
        var settingCount = db.SiteSettings.Count();

        // ডিফল্ট HeroSliderInterval না থাকলে তৈরি করুন
        var existing = db.SiteSettings
            .FirstOrDefault(s => s.SettingKey == SiteSettingService.HeroSliderIntervalKey);

        if (existing == null)
        {
            db.SiteSettings.Add(new al_ibtisam.Models.SiteSetting
            {
                SettingKey = SiteSettingService.HeroSliderIntervalKey,
                SettingValue = "3",
                UpdatedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            Console.WriteLine("✅ Default HeroSliderInterval = 3 সেকেন্ড তৈরি করা হয়েছে।");
        }
        else
        {
            Console.WriteLine($"✅ SiteSettings loaded: {settingCount} টি | Slider Interval = {existing.SettingValue}s");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ SiteSettings টেবিল পাওয়া যায়নি: " + ex.Message);
        Console.WriteLine("👉 pgAdmin-এ SiteSettings টেবিল তৈরি করার SQL script চালান।");
    }

    // ---------- 9.6 🆕 Fonts ফোল্ডার চেক ----------
    try
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var fontsPath = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "fonts");
        var regularFont = Path.Combine(fontsPath, "regular.ttf");
        var boldFont = Path.Combine(fontsPath, "bold.ttf");

        if (!Directory.Exists(fontsPath))
        {
            Directory.CreateDirectory(fontsPath);
            Console.WriteLine("⚠️ wwwroot/fonts ফোল্ডার তৈরি করা হয়েছে — এখানে regular.ttf ও bold.ttf রাখুন।");
        }

        if (!File.Exists(regularFont) || !File.Exists(boldFont))
        {
            Console.WriteLine("⚠️ বাংলা ফন্ট পাওয়া যায়নি — wwwroot/fonts/regular.ttf ও bold.ttf রাখুন।");
        }
        else
        {
            Console.WriteLine("✅ AdorNoirrit বাংলা ফন্ট লোড করার জন্য প্রস্তুত।");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("⚠️ Fonts ফোল্ডার চেক ব্যর্থ: " + ex.Message);
    }
}

// ============================================================
// 10. Configure HTTP Request Pipeline
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// 🆕 Localization — অবশ্যই UseRouting-এর আগে
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

// HTTPS redirect
app.UseHttpsRedirection();

// Static files serve (CSS, JS, images, /uploads, /fonts সহ সব)
app.UseStaticFiles();

// Routing enable
app.UseRouting();

// Session (UseAuthentication-এর আগে)
app.UseSession();

// Authentication → Authorization (ক্রম গুরুত্বপূর্ণ)
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// 11. Route Configuration
// ============================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();