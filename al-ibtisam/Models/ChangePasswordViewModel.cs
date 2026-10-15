using System.ComponentModel.DataAnnotations;

namespace al_ibtisam.Models
{
    /// <summary>
    /// Password পরিবর্তনের জন্য ViewModel।
    /// User তার বর্তমান password দিয়ে নতুন password সেট করতে পারবে।
    /// </summary>
    public class ChangePasswordViewModel
    {
        // ============================================================
        // Old Password (বর্তমান পাসওয়ার্ড)
        // ============================================================
        [Required(ErrorMessage = "পুরনো পাসওয়ার্ড দিন")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; } = string.Empty;

        // ============================================================
        // New Password (নতুন পাসওয়ার্ড)
        // ============================================================
        [Required(ErrorMessage = "নতুন পাসওয়ার্ড দিন")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "পাসওয়ার্ড কমপক্ষে ৬ অক্ষরের হতে হবে")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        // ============================================================
        // Confirm Password (নতুন পাসওয়ার্ড নিশ্চিত)
        // ============================================================
        [Required(ErrorMessage = "নতুন পাসওয়ার্ড নিশ্চিত করুন")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "দুইটি পাসওয়ার্ড মিলছে না")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}