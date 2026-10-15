using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    /// <summary>
    /// ডাটাবেজে সংরক্ষিত ইউজার মডেল।
    /// PostgreSQL-এর "Users" টেবিলের সাথে ম্যাপ করা হবে।
    /// </summary>
    [Table("Users")]
    public class ApplicationUser
    {
        // ============================================================
        // Primary Key
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // Full Name
        // ============================================================
        [Required(ErrorMessage = "পূর্ণ নাম দিন")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "নাম ৩ থেকে ১০০ অক্ষরের মধ্যে হতে হবে")]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        // ============================================================
        // Email (Unique)
        // ============================================================
        [Required(ErrorMessage = "ইমেইল দিন")]
        [EmailAddress(ErrorMessage = "সঠিক ইমেইল দিন")]
        [StringLength(150, ErrorMessage = "ইমেইল ১৫০ অক্ষরের কম হতে হবে")]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        // ============================================================
        // Phone Number (Unique)
        // ============================================================
        [Required(ErrorMessage = "ফোন নম্বর দিন")]
        [Phone(ErrorMessage = "সঠিক ফোন নম্বর দিন")]
        [StringLength(20, ErrorMessage = "ফোন নম্বর ২০ অক্ষরের কম হতে হবে")]
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        // ============================================================
        // Password Hash (BCrypt)
        // ============================================================
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        // ============================================================
        // Account Created Date
        // ============================================================
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================================
        // Last Login Date (Nullable)
        // ============================================================
        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // ============================================================
        // Is Active (সফট ডিলিট / একাউন্ট disable করার জন্য)
        // ============================================================
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // ============================================================
        // Optional: Extra Fields (ভবিষ্যতে দরকার হতে পারে)
        // ============================================================

        /// <summary>
        /// প্রোফাইল ছবির URL (ঐচ্ছিক)
        /// </summary>
        [StringLength(500)]
        [Column("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// ইউজারের ঠিকানা (ঐচ্ছিক)
        /// </summary>
        [StringLength(500)]
        [Column("address")]
        public string? Address { get; set; }

        /// <summary>
        /// জেলা (ঐচ্ছিক)
        /// </summary>
        [StringLength(100)]
        [Column("district")]
        public string? District { get; set; }

        /// <summary>
        /// ইউজার রোল (Default: "Customer")
        /// ভবিষ্যতে "Admin" রোল যোগ করার জন্য প্রস্তুত
        /// </summary>
        [StringLength(50)]
        [Column("role")]
        public string Role { get; set; } = "Customer";

        /// <summary>
        /// ইমেইল verify হয়েছে কি না
        /// </summary>
        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;
    }
}