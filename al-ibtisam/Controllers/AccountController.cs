using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace al_ibtisam.Controllers
{
    /// <summary>
    /// অ্যাকাউন্ট সংক্রান্ত সব কাজ — Sign In, Sign Up, Sign Out,
    /// Profile, Orders, Change Password
    /// PostgreSQL ডাটাবেজ + Cookie Authentication ব্যবহার করে।
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _config;

        // ============================================================
        // Constructor — Dependency Injection
        // ============================================================
        public AccountController(
            AppDbContext db,
            ILogger<AccountController> logger,
            IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _config = config;
        }

        // ============================================================
        // SIGN IN (GET)
        // URL: /Account/SignIn
        // ============================================================
        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // অ্যাডমিন হলে সরাসরি Admin Dashboard-এ পাঠান
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ============================================================
        // SIGN IN (POST)
        // URL: /Account/SignIn
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            _logger.LogInformation("========== SIGN IN ATTEMPT ==========");
            _logger.LogInformation($"EmailOrPhone: {model.EmailOrPhone}");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var input = model.EmailOrPhone.Trim();

                // ============================================================
                // 🔐 ধাপ ১: অ্যাডমিন লগইন চেক (appsettings.json থেকে)
                // ============================================================
                var adminEmail = _config["AdminSettings:Email"] ?? "admin";
                var adminPassword = _config["AdminSettings:Password"] ?? "123456";
                var adminFullName = _config["AdminSettings:FullName"] ?? "Administrator";

                if (input.Equals(adminEmail, StringComparison.OrdinalIgnoreCase)
                    && model.Password == adminPassword)
                {
                    _logger.LogInformation("✅ Admin sign in successful");

                    // ---------- Admin Cookie তৈরি ----------
                    var adminClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, "0"),
                        new Claim(ClaimTypes.Name, adminFullName),
                        new Claim(ClaimTypes.Email, adminEmail),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var adminIdentity = new ClaimsIdentity(
                        adminClaims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var adminPrincipal = new ClaimsPrincipal(adminIdentity);

                    var adminAuthProps = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2),
                        IssuedUtc = DateTimeOffset.UtcNow,
                        AllowRefresh = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        adminPrincipal,
                        adminAuthProps);

                    TempData["SuccessMessage"] = $"স্বাগতম, {adminFullName}!";

                    // ---------- সরাসরি Admin Dashboard-এ পাঠান ----------
                    return RedirectToAction("Dashboard", "Admin");
                }

                // ============================================================
                // 👤 ধাপ ২: সাধারণ ইউজার লগইন (ডাটাবেজ থেকে)
                // ============================================================
                var emailLower = input.ToLower();

                var user = await _db.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == emailLower ||
                    u.Phone == input);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {input}");
                    ModelState.AddModelError(string.Empty, "ভুল ইমেইল/ফোন অথবা পাসওয়ার্ড।");
                    return View(model);
                }

                // ---------- Active কিনা চেক ----------
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "আপনার অ্যাকাউন্ট নিষ্ক্রিয় করা হয়েছে। সাপোর্টে যোগাযোগ করুন।");
                    return View(model);
                }

                // ---------- পাসওয়ার্ড verify (BCrypt) ----------
                bool isValidPassword;
                try
                {
                    isValidPassword = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"BCrypt verify error: {ex.Message}");
                    isValidPassword = false;
                }

                if (!isValidPassword)
                {
                    ModelState.AddModelError(string.Empty, "ভুল ইমেইল/ফোন অথবা পাসওয়ার্ড।");
                    return View(model);
                }

                // ---------- Last Login Time আপডেট ----------
                user.LastLoginAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // ---------- Cookie দিয়ে Sign In ----------
                await SignInUserAsync(user, model.RememberMe);

                _logger.LogInformation($"✅ Sign in successful: {user.Email}");

                TempData["SuccessMessage"] = $"স্বাগতম, {user.FullName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"SignIn error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "সাইন ইন করতে সমস্যা হয়েছে। আবার চেষ্টা করুন।");
                return View(model);
            }
        }

        // ============================================================
        // SIGN UP (GET)
        // URL: /Account/SignUp
        // ============================================================
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ============================================================
        // SIGN UP (POST)
        // URL: /Account/SignUp
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            _logger.LogInformation("========== SIGN UP ATTEMPT ==========");
            _logger.LogInformation($"Email: {model.Email}, Phone: {model.Phone}");

            // ---------- Terms manually check ----------
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(nameof(model.AcceptTerms), "শর্তাবলীতে সম্মতি দিতে হবে");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // ---------- Email already exists? ----------
                var emailLower = model.Email.Trim().ToLower();

                bool emailExists = await _db.Users
                    .AnyAsync(u => u.Email.ToLower() == emailLower);

                if (emailExists)
                {
                    ModelState.AddModelError(nameof(model.Email), "এই ইমেইল দিয়ে ইতিমধ্যে অ্যাকাউন্ট আছে।");
                    return View(model);
                }

                // ---------- Phone already exists? ----------
                var phoneTrimmed = model.Phone.Trim();

                bool phoneExists = await _db.Users
                    .AnyAsync(u => u.Phone == phoneTrimmed);

                if (phoneExists)
                {
                    ModelState.AddModelError(nameof(model.Phone), "এই ফোন নম্বর দিয়ে ইতিমধ্যে অ্যাকাউন্ট আছে।");
                    return View(model);
                }

                // ---------- Password hash ----------
                string passwordHash;
                try
                {
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"BCrypt hash error: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "পাসওয়ার্ড প্রসেস করতে সমস্যা হয়েছে।");
                    return View(model);
                }

                // ---------- নতুন ইউজার তৈরি ----------
                var newUser = new ApplicationUser
                {
                    FullName = model.FullName.Trim(),
                    Email = emailLower,
                    Phone = phoneTrimmed,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = null,
                    IsActive = true,
                    Role = "Customer",
                    EmailVerified = false
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ User saved! ID = {newUser.Id}");

                TempData["SuccessMessage"] = "🎉 অ্যাকাউন্ট সফলভাবে তৈরি হয়েছে! এখন সাইন ইন করুন।";
                return RedirectToAction("SignIn", "Account");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"DbUpdateException: {dbEx.Message}");
                _logger.LogError($"Inner: {dbEx.InnerException?.Message}");
                ModelState.AddModelError(string.Empty, "অ্যাকাউন্ট তৈরি করতে সমস্যা হয়েছে। আবার চেষ্টা করুন।");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SignUp error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "সার্ভারে সমস্যা হয়েছে। পরে আবার চেষ্টা করুন।");
                return View(model);
            }
        }

        // ============================================================
        // SIGN OUT (POST)
        // URL: /Account/SignOut
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User signed out");
            TempData["SuccessMessage"] = "সফলভাবে সাইন আউট হয়েছে! আবার আসবেন।";
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // প্রোফাইল পেজ (GET)
        // URL: /Account/Profile
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Account",
                    new { returnUrl = "/Account/Profile" });
            }

            // অ্যাডমিন হলে Admin Dashboard-এ পাঠান
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            return View(user);
        }

        // ============================================================
        // প্রোফাইল আপডেট (POST)
        // URL: /Account/UpdateProfile
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            string fullName,
            string phone,
            string? address,
            string? district)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Account");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // ---------- Full Name Validation ----------
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 3)
            {
                TempData["Error"] = "নাম কমপক্ষে ৩ অক্ষরের হতে হবে";
                return RedirectToAction("Profile");
            }

            // ---------- Phone Validation ----------
            phone = phone?.Trim() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^01[3-9]\d{8}$"))
            {
                TempData["Error"] = "সঠিক ১১ ডিজিটের মোবাইল নম্বর দিন (যেমন: 01XXXXXXXXX)";
                return RedirectToAction("Profile");
            }

            // ---------- Phone Already Taken? ----------
            bool phoneTaken = await _db.Users
                .AnyAsync(u => u.Phone == phone && u.Id != user.Id);

            if (phoneTaken)
            {
                TempData["Error"] = "এই ফোন নম্বর অন্য একজন ব্যবহার করছেন";
                return RedirectToAction("Profile");
            }

            // ---------- Update ----------
            user.FullName = fullName.Trim();
            user.Phone = phone;
            user.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            user.District = string.IsNullOrWhiteSpace(district) ? null : district.Trim();

            await _db.SaveChangesAsync();

            // ---------- Cookie-তে নাম আপডেট (header-এ দেখাবে) ----------
            await SignInUserAsync(user, true);

            TempData["Success"] = "প্রোফাইল সফলভাবে আপডেট হয়েছে ✅";
            return RedirectToAction("Profile");
        }

        // ============================================================
        // My Orders পেজ
        // URL: /Account/Orders
        // ============================================================
        [HttpGet]
        public IActionResult Orders()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Account",
                    new { returnUrl = "/Account/Orders" });
            }

            // TODO: পরে ডাটাবেজ থেকে ইউজারের orders আনবেন
            return View();
        }

        // ============================================================
        // Change Password (GET)
        // URL: /Account/ChangePassword
        // ============================================================
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Account",
                    new { returnUrl = "/Account/ChangePassword" });
            }

            return View(new ChangePasswordViewModel());
        }

        // ============================================================
        // Change Password (POST)
        // URL: /Account/ChangePassword
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // ---------- Old Password Verify ----------
            bool isValidOld;
            try
            {
                isValidOld = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash);
            }
            catch
            {
                isValidOld = false;
            }

            if (!isValidOld)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "পুরনো পাসওয়ার্ড ভুল");
                return View(model);
            }

            // ---------- New Password Hash ----------
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, workFactor: 12);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChangePassword error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "পাসওয়ার্ড পরিবর্তনে সমস্যা হয়েছে। আবার চেষ্টা করুন।");
                return View(model);
            }

            TempData["Success"] = "পাসওয়ার্ড সফলভাবে পরিবর্তন হয়েছে ✅";
            return RedirectToAction("ChangePassword");
        }

        // ============================================================
        // DEBUG: সব ইউজার দেখার জন্য (Development-only)
        // URL: /Account/DebugUsers
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _db.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.IsActive,
                        u.Role,
                        u.CreatedAt,
                        u.LastLoginAt,
                        PasswordHashLength = u.PasswordHash.Length
                    })
                    .ToListAsync();

                return Json(new { success = true, count = users.Count, users = users });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // ============================================================
        // DEBUG: Database Connection Test
        // URL: /Account/DebugDb
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> DebugDb()
        {
            try
            {
                bool canConnect = await _db.Database.CanConnectAsync();
                int userCount = 0;
                if (canConnect) userCount = await _db.Users.CountAsync();

                return Json(new
                {
                    success = true,
                    canConnect = canConnect,
                    userCount = userCount,
                    dbName = _db.Database.GetDbConnection().Database,
                    dataSource = _db.Database.GetDbConnection().DataSource
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // ============================================================
        // HELPER: বর্তমান লগইন করা ইউজার বের করুন
        // ============================================================
        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            // Admin (Id = 0) হলে null ফেরত দিন
            if (userId == 0)
            {
                return null;
            }

            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // ============================================================
        // HELPER: Cookie দিয়ে Sign In করানোর মেথড
        // ============================================================
        private async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer"),
                new Claim("Phone", user.Phone)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(2),
                IssuedUtc = DateTimeOffset.UtcNow,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);
        }
    }
}