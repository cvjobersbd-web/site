using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    /// <summary>
    /// নেভবারের মেনুগুলো ডাইনামিকভাবে ম্যানেজ করার মডেল।
    /// অ্যাডমিন প্যানেল থেকে মেনুর নাম, লিংক, স্ট্যাটাস এবং অ্যালাইনমেন্ট (বাম, মাঝখানে, ডান) কন্ট্রোল করা যাবে।
    /// </summary>
    [Table("NavigationMenus")]
    public class NavigationMenu
    {
        // ============================================================
        // Primary Key
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // Menu Title
        // ============================================================
        /// <summary>
        /// মেনুর নাম (যেমন: হোম, শপ, ব্লগ ইত্যাদি)
        /// </summary>
        [Required(ErrorMessage = "মেনুর নাম দিতে হবে")]
        [StringLength(100, ErrorMessage = "মেনুর নাম ১০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "Menu Title")]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        // ============================================================
        // Menu URL / Link
        // ============================================================
        /// <summary>
        /// মেনুতে ক্লিক করলে যে লিংকে যাবে (যেমন: /, /Category/Load?name=All)
        /// </summary>
        [Required(ErrorMessage = "লিংক দিতে হবে")]
        [StringLength(500, ErrorMessage = "লিংক ৫০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "URL")]
        [Column("url")]
        public string Url { get; set; } = "#";

        // ============================================================
        // Is Active (Status)
        // ============================================================
        /// <summary>
        /// মেনুটি ওয়েবসাইটে দেখাবে কি না।
        /// false হলে নেভবারে মেনুটি লুকিয়ে থাকবে।
        /// </summary>
        [Display(Name = "Is Active")]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // Display Order
        // ============================================================
        /// <summary>
        /// মেনুর ক্রম (ছোট সংখ্যা আগে দেখাবে)।
        /// </summary>
        [Display(Name = "Display Order")]
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        // ============================================================
        // Alignment (Left / Center / Right)
        // ============================================================
        /// <summary>
        /// মেনুটি নেভবারের কোন দিকে থাকবে।
        /// সম্ভাব্য মান: "Left", "Center", "Right"
        /// </summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Alignment")]
        [Column("alignment")]
        public string Alignment { get; set; } = "Left";
    }
}