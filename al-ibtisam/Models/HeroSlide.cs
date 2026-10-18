using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    /// <summary>
    /// হোম পেইজের হিরো স্লাইডারের প্রতিটি স্লাইড।
    /// PostgreSQL-এর "HeroSlides" টেবিলের সাথে ম্যাপ করা হয়েছে।
    /// </summary>
    [Table("HeroSlides")]
    public class HeroSlide
    {
        // ============================================================
        // Primary Key
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // Main Image
        // ============================================================
        /// <summary>
        /// স্লাইডারের মেইন ইমেজ URL।
        /// Admin প্যানেল থেকে আপলোড করা ইমেজের path এখানে সেভ হবে।
        /// উদাহরণ: "/uploads/slides/abc123.jpg"
        /// </summary>
        [StringLength(1000, ErrorMessage = "Image URL ১০০০ অক্ষরের কম হতে হবে")]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        // ============================================================
        // Caption Text Fields (বর্তমানে ব্যবহৃত হচ্ছে না, ভবিষ্যতের জন্য রাখা)
        // ============================================================

        [StringLength(100)]
        [Column("label")]
        public string? Label { get; set; }

        [StringLength(150)]
        [Column("title_light")]
        public string? TitleLight { get; set; }

        [StringLength(150)]
        [Column("title_bold")]
        public string? TitleBold { get; set; }

        [StringLength(300)]
        [Column("subtitle")]
        public string? Subtitle { get; set; }

        // ============================================================
        // Active / Display Status
        // ============================================================

        /// <summary>
        /// স্লাইডটি হোম পেইজে দেখাবে কি না।
        /// false হলে হোম পেইজের স্লাইডারে এটি লোড হবে না।
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ডিসপ্লে অর্ডার — ছোট সংখ্যা আগে দেখাবে।
        /// Admin থেকে Auto-assigned হবে (max + 1)।
        /// </summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        // ============================================================
        // Timestamps
        // ============================================================
        /// <summary>
        /// কবে তৈরি হয়েছিল (UTC)।
        /// Admin থেকে auto-set হবে।
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}