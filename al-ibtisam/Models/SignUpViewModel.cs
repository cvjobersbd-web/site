using System.ComponentModel.DataAnnotations;

namespace al_ibtisam.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "পূর্ণ নাম দিন")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "নাম ৩ থেকে ১০০ অক্ষরের মধ্যে হতে হবে")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ইমেইল দিন")]
        [EmailAddress(ErrorMessage = "সঠিক ইমেইল দিন")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "ফোন নম্বর দিন")]
        [Phone(ErrorMessage = "সঠিক ফোন নম্বর দিন")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "পাসওয়ার্ড দিন")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "পাসওয়ার্ড কমপক্ষে ৬ অক্ষরের হতে হবে")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "পাসওয়ার্ড নিশ্চিত করুন")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "পাসওয়ার্ড দুইটি মিলছে না")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "শর্তাবলীতে সম্মতি দিতে হবে")]
        [Display(Name = "Terms & Conditions")]
        public bool AcceptTerms { get; set; }
    }
}