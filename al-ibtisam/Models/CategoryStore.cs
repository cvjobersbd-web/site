using System.Collections.Generic;

namespace al_ibtisam.Models
{
    public static class CategoryStore
    {
        // শুরুতে ডিফল্ট ক্যাটাগরিগুলোর লিস্ট
        private static List<string> _categories = new List<string>
        {
            "Panjabi",
            "Thobe",
            "Jubbah",
            "T-Shirt",
            "Polo Shirt",
            "Pant & Trouser",
            "Women's Clothing",
            "Attar",
            "Perfumes",
            "Watch",
            "Wallet",
            "Sneakers",
            "Waistcoat",
            "Tupi",
            "Combo Offers",
            "Katua",
            "Flash Sale"
        };

        // সব ক্যাটাগরির লিস্ট পাওয়ার মেথড
        public static List<string> GetAll() => _categories;

        // নতুন ক্যাটাগরি অ্যাড করার মেথড
        public static void Add(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                // চেক করা হচ্ছে ক্যাটাগরিটি আগে থেকেই আছে কি না (Case-insensitive)
                bool exists = _categories.Exists(c => c.Equals(categoryName.Trim(), System.StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    _categories.Add(categoryName.Trim());
                }
            }
        }

        // ক্যাটাগরি ডিলিট করার মেথড
        public static void Delete(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var itemToRemove = _categories.Find(c => c.Equals(categoryName.Trim(), System.StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                {
                    _categories.Remove(itemToRemove);
                }
            }
        }
    }
}