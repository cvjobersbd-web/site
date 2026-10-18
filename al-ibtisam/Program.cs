using al_ibtisam.Data;
using al_ibtisam.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

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
// 4. Image Upload Service
// ============================================================
builder.Services.AddScoped<IImageService, ImageService>();

// ============================================================
// 5. Session
// ============================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "AlIbtisam.Session";
});

// ============================================================
// 6. HttpContextAccessor
// ============================================================
builder.Services.AddHttpContextAccessor();

// ============================================================
// 7. Antiforgery (CSRF Protection)
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
// 8. অ্যাপ স্টার্টআপে ফোল্ডার তৈরি + DB চেক + Categories চেক
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // ---------- 8.1 Uploads ফোল্ডার অটো তৈরি ----------
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

    // ---------- 8.2 Database কানেকশন চেক ----------
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

    // ---------- 8.3 Categories টেবিল চেক (নতুন) ----------
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
}

// ============================================================
// 9. Configure HTTP Request Pipeline
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

// HTTPS redirect
app.UseHttpsRedirection();

// Static files serve (CSS, JS, images, /uploads সহ সব)
app.UseStaticFiles();

// Routing enable
app.UseRouting();

// Session (UseAuthentication-এর আগে)
app.UseSession();

// Authentication → Authorization (ক্রম গুরুত্বপূর্ণ)
app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// 10. Route Configuration
// ============================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();