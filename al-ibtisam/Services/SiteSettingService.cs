using al_ibtisam.Data;
using al_ibtisam.Models;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Services
{
    /// <summary>
    /// Site Settings সার্ভিসের ইন্টারফেস।
    /// Key/Value স্টাইলে গ্লোবাল সেটিংস পড়া ও সেভ করার কাজ করে।
    /// </summary>
    public interface ISiteSettingService
    {
        /// <summary>
        /// একটি নির্দিষ্ট key-এর মান (string) ফেরত দেয়।
        /// না থাকলে null।
        /// </summary>
        Task<string?> GetValueAsync(string key);

        /// <summary>
        /// একটি key-এর মান সেভ করে (নতুন হলে তৈরি, পুরনো হলে update)।
        /// </summary>
        Task<SiteSetting> SetValueAsync(string key, string value);

        /// <summary>
        /// হিরো স্লাইডারের interval (সেকেন্ডে) ফেরত দেয়।
        /// Setting না থাকলে বা ভুল হলে ডিফল্ট 3 ফেরত দেয়।
        /// </summary>
        Task<int> GetSliderIntervalAsync();

        /// <summary>
        /// সব সেটিংস একসাথে Dictionary আকারে ফেরত দেয়।
        /// </summary>
        Task<Dictionary<string, string?>> GetAllAsync();

        /// <summary>
        /// একটি key মুছে ফেলে। সফল হলে true।
        /// </summary>
        Task<bool> RemoveAsync(string key);
    }

    /// <summary>
    /// SiteSettings টেবিলের সাথে কাজ করার মূল সার্ভিস।
    /// প্রতিটি key-এর জন্য একটি মাত্র entry রাখে (unique index এর উপর নির্ভরশীল)।
    /// </summary>
    public class SiteSettingService : ISiteSettingService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SiteSettingService> _logger;

        // ============================================================
        // Common Setting Keys — যেখানে ব্যবহার হবে সেখানে এগুলো ব্যবহার করুন
        // ============================================================
        public const string HeroSliderIntervalKey = "HeroSliderInterval";
        public const string SiteNameKey = "SiteName";
        public const string ContactPhoneKey = "ContactPhone";

        // Default fallback value
        private const int DefaultSliderInterval = 3;
        private const int MinSliderInterval = 1;
        private const int MaxSliderInterval = 60;

        // ============================================================
        // Constructor
        // ============================================================
        public SiteSettingService(
            AppDbContext db,
            ILogger<SiteSettingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ============================================================
        // 1. Get Value (string)
        // ============================================================
        public async Task<string?> GetValueAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                var setting = await _db.SiteSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SettingKey == key);

                return setting?.SettingValue;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetValueAsync error [{key}]: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // 2. Set Value (Insert or Update)
        // ============================================================
        public async Task<SiteSetting> SetValueAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key খালি রাখা যাবে না।", nameof(key));

            try
            {
                var setting = await _db.SiteSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key);

                if (setting == null)
                {
                    // ---------- নতুন তৈরি ----------
                    setting = new SiteSetting
                    {
                        SettingKey = key.Trim(),
                        SettingValue = value,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _db.SiteSettings.Add(setting);
                }
                else
                {
                    // ---------- পুরনো update ----------
                    setting.SettingValue = value;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();

                _logger.LogInformation($"Setting saved: {key} = {value}");

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SetValueAsync error [{key}]: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // 3. Get Slider Interval (সেকেন্ডে) — strong-typed
        // ============================================================
        public async Task<int> GetSliderIntervalAsync()
        {
            var raw = await GetValueAsync(HeroSliderIntervalKey);

            if (string.IsNullOrWhiteSpace(raw))
                return DefaultSliderInterval;

            if (!int.TryParse(raw, out var seconds))
            {
                _logger.LogWarning(
                    $"Slider interval value '{raw}' সঠিক সংখ্যা নয় — ডিফল্ট {DefaultSliderInterval} ব্যবহার করা হচ্ছে।");
                return DefaultSliderInterval;
            }

            // ---------- Range clamp ----------
            if (seconds < MinSliderInterval) seconds = MinSliderInterval;
            if (seconds > MaxSliderInterval) seconds = MaxSliderInterval;

            return seconds;
        }

        // ============================================================
        // 4. Get All Settings (Dictionary)
        // ============================================================
        public async Task<Dictionary<string, string?>> GetAllAsync()
        {
            try
            {
                return await _db.SiteSettings
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        s => s.SettingKey,
                        s => s.SettingValue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllAsync error: {ex.Message}");
                return new Dictionary<string, string?>();
            }
        }

        // ============================================================
        // 5. Remove Setting
        // ============================================================
        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                var setting = await _db.SiteSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key);

                if (setting == null)
                    return false;

                _db.SiteSettings.Remove(setting);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Setting removed: {key}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RemoveAsync error [{key}]: {ex.Message}");
                return false;
            }
        }
    }
}