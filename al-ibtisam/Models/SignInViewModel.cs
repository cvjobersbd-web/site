using System.ComponentModel.DataAnnotations;

namespace al_ibtisam.Models
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "ইমেইল বা ফোন নম্বর দিন")]
        [Display(Name = "Email or Phone")]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "পাসওয়ার্ড দিন")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}