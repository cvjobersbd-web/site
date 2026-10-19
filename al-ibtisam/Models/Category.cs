using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    [Table("Categories")]
    public class Category
    {
        // ============================================================
        // Primary Key
        // ============================================================
        [Key]
        public int Id { get; set; }

        // ============================================================
        // Category Name
        // ============================================================
        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Navbar-এ দেখাবে কি না</summary>
        [Column("show_in_navbar")]
        public bool ShowInNavbar { get; set; } = true;

        /// <summary>ক্রম (ছোট আগে দেখাবে)</summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>Special style (Flash Sale এর জন্য)</summary>
        [Column("is_flash_sale")]
        public bool IsFlashSale { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================================
        // 🆕 Shop By Category Icon
        // ============================================================
        /// <summary>
        /// ক্যাটাগরির আইকনের URL (অ্যাডমিন প্যানেল থেকে আপলোড করা হবে)।
        /// শুধুমাত্র "Shop By Category" সাইডবারে দেখানোর জন্য ব্যবহৃত হবে।
        /// উদাহরণ: "/uploads/category-icons/abc123.png"
        /// </summary>
        [StringLength(500)]
        [Column("icon_url")]
        public string? IconUrl { get; set; }
    }
}