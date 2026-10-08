using System.ComponentModel.DataAnnotations;

namespace al_ibtisam.Models
{
    public class CheckoutViewModel
    {
        // ============================================================
        // বিলিং ডিটেইল — Full Name
        // ============================================================
        [Required(ErrorMessage = "আপনার নাম লিখুন")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "নাম ৩ থেকে ১০০ অক্ষরের মধ্যে হতে হবে")]
        [Display(Name = "আপনার নাম")]
        public string FullName { get; set; } = string.Empty;

        // ============================================================
        // Mobile Number
        // ============================================================
        [Required(ErrorMessage = "আপনার মোবাইল নম্বরটি লিখুন")]
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "১১ ডিজিটের সঠিক মোবাইল নম্বর লিখুন (যেমন: 01XXXXXXXXX)")]
        [Display(Name = "আপনার মোবাইল নম্বরটি")]
        public string Phone { get; set; } = string.Empty;

        // ============================================================
        // District
        // ============================================================
        [Required(ErrorMessage = "জেলা সিলেক্ট করুন")]
        [Display(Name = "জেলা")]
        public string District { get; set; } = string.Empty;

        // ============================================================
        // Full Address
        // ============================================================
        [Required(ErrorMessage = "সম্পূর্ণ ঠিকানা লিখুন")]
        [StringLength(500, ErrorMessage = "ঠিকানা ৫০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "সম্পূর্ণ ঠিকানা")]
        public string Address { get; set; } = string.Empty;

        // ============================================================
        // Email (Optional)
        // ============================================================
        [EmailAddress(ErrorMessage = "সঠিক ইমেইল লিখুন")]
        [Display(Name = "ইমেইল")]
        public string? Email { get; set; }

        // ============================================================
        // WhatsApp Number (Optional)
        // ============================================================
        [RegularExpression(@"^01[3-9]\d{8}$", ErrorMessage = "১১ ডিজিটের সঠিক হোয়াটসঅ্যাপ নম্বর লিখুন")]
        [Display(Name = "হোয়াটসঅ্যাপ নম্বর")]
        public string? WhatsApp { get; set; }

        // ============================================================
        // Promo Code (Optional)
        // ============================================================
        [Display(Name = "প্রোমো কোড")]
        public string? PromoCode { get; set; }

        // ============================================================
        // Payment Method (Default: COD)
        // ============================================================
        [Display(Name = "পেমেন্ট মেথড")]
        public string PaymentMethod { get; set; } = "COD";

        // ============================================================
        // Order Note (Optional)
        // ============================================================
        [StringLength(500, ErrorMessage = "নোট ৫০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "অর্ডার নোট")]
        public string? OrderNote { get; set; }
    }
}