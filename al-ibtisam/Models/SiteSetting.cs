using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    /// <summary>
    /// সাইটের গ্লোবাল সেটিংস — Key/Value স্টাইলে।
    /// 
    /// উদাহরণ:
    ///   SettingKey = "HeroSliderInterval"  →  SettingValue = "3"   (সেকেন্ড)
    ///   SettingKey = "SiteName"            →  SettingValue = "Al Ibtisam"
    /// 
    /// প্রতিটি SettingKey ইউনিক — একই key একাধিকবার থাকতে পারবে না।
    /// PostgreSQL-এর "SiteSettings" টেবিলের সাথে ম্যাপ করা হয়েছে।
    /// </summary>
    [Table("SiteSettings")]
    public class SiteSetting
    {
        // ============================================================
        // Primary Key
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // Setting Key (Unique)
        // ============================================================
        /// <summary>
        /// সেটিংয়ের নাম/কী (unique)।
        /// উদাহরণ: "HeroSliderInterval", "SiteName", "ContactPhone"
        /// </summary>
        [Required(ErrorMessage = "Setting Key দিতে হবে")]
        [StringLength(100, ErrorMessage = "Setting Key ১০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "Setting Key")]
        [Column("setting_key")]
        public string SettingKey { get; set; } = string.Empty;

        // ============================================================
        // Setting Value
        // ============================================================
        /// <summary>
        /// সেটিংয়ের মান (string হিসাবে সংরক্ষিত)।
        /// প্রকার (int, bool, JSON) পরে convert করে ব্যবহার করতে হবে।
        /// উদাহরণ: "3", "Al Ibtisam", "09638090000"
        /// </summary>
        [StringLength(500, ErrorMessage = "Setting Value ৫০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "Setting Value")]
        [Column("setting_value")]
        public string? SettingValue { get; set; }

        // ============================================================
        // Timestamp
        // ============================================================
        /// <summary>
        /// সর্বশেষ কবে আপডেট হয়েছিল (UTC)।
        /// প্রতিবার SetValueAsync() কল হলে auto-refresh হবে।
        /// </summary>
        [Display(Name = "Updated At")]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ============================================================
        // [NotMapped] Helper Properties
        // ============================================================

        /// <summary>
        /// SettingValue-কে int-এ convert করার চেষ্টা করে।
        /// ব্যর্থ হলে null ফেরত দেয়।
        /// HeroSliderInterval পড়ার সময় কাজে লাগে।
        /// </summary>
        [NotMapped]
        public int? ValueAsInt =>
            int.TryParse(SettingValue, out var v) ? v : null;

        /// <summary>
        /// SettingValue-কে bool-এ convert করার চেষ্টা করে।
        /// "true"/"false", "1"/"0" — সবই সাপোর্ট করে।
        /// </summary>
        [NotMapped]
        public bool? ValueAsBool =>
            bool.TryParse(SettingValue, out var b) ? b : null;

        /// <summary>
        /// SettingValue ফাঁকা কি না।
        /// </summary>
        [NotMapped]
        public bool IsEmpty => string.IsNullOrWhiteSpace(SettingValue);
    }
}