using al_ibtisam.Models;
using Microsoft.AspNetCore.Mvc;

namespace al_ibtisam.Controllers
{
    public class AccountController : Controller
    {
        // ================= SIGN IN (GET) =================
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        // ================= SIGN IN (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ---------------------------------------------------------
            // এখানে আপনার আসল authentication logic বসান।
            // উদাহরণ: ডাটাবেজ থেকে ইউজার খুঁজে বের করা, পাসওয়ার্ড verify করা,
            //         CookieAuthentication দিয়ে সাইন ইন করানো ইত্যাদি।
            // ---------------------------------------------------------

            TempData["SuccessMessage"] = "সফলভাবে সাইন ইন হয়েছে!";
            return RedirectToAction("Index", "Home");
        }

        // ================= SIGN UP (GET) =================
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // ================= SIGN UP (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ---------------------------------------------------------
            // এখানে আপনার আসল registration logic বসান।
            // উদাহরণ:
            //   1. ইমেইল আগে থেকে আছে কি না চেক করুন
            //   2. পাসওয়ার্ড hash করুন (BCrypt / Identity)
            //   3. ডাটাবেজে ইউজার সেভ করুন
            // ---------------------------------------------------------

            TempData["SuccessMessage"] = "অ্যাকাউন্ট সফলভাবে তৈরি হয়েছে! এখন সাইন ইন করুন।";
            return RedirectToAction("SignIn", "Account");
        }

        // ================= SIGN OUT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignOut()
        {
            // এখানে CookieAuthentication.SignOutAsync() কল করুন
            TempData["SuccessMessage"] = "সফলভাবে সাইন আউট হয়েছে!";
            return RedirectToAction("Index", "Home");
        }
    }
}