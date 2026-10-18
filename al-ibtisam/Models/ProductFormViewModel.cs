using System.ComponentModel.DataAnnotations;

namespace al_ibtisam.Models
{
    /// <summary>
    /// Admin প্যানেলে প্রোডাক্ট Add/Edit করার জন্য ViewModel।
    /// 
    /// এই ViewModel-এ দুটি ধরনের ডাটা থাকে:
    /// ১. টেক্সট ডাটা (Name, Price, Category ইত্যাদি)
    /// ২. ফাইল আপলোড ডাটা (MainImageFile, GalleryFiles, SizeChartFile)
    /// 
    /// ফাইল আপলোডের জন্য HTML ফর্মে অবশ্যই 
    /// enctype="multipart/form-data" থাকতে হবে।
    /// </summary>
    public class ProductFormViewModel
    {
        // ============================================================
        // Primary Key (Edit-এর সময় দরকার)
        // ============================================================
        public int Id { get; set; }

        // ============================================================
        // Basic Info
        // ============================================================

        [Required(ErrorMessage = "প্রোডাক্টের নাম অবশ্যই দিতে হবে")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "প্রোডাক্টের নাম ২ থেকে ২০০ অক্ষরের মধ্যে হতে হবে")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ক্যাটাগরি সিলেক্ট করতে হবে")]
        [StringLength(100)]
        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = "Believers";

        // ============================================================
        // Pricing
        // ============================================================

        [Required(ErrorMessage = "প্রাইস দিতে হবে")]
        [Range(0, double.MaxValue,
            ErrorMessage = "প্রাইস ঋণাত্মক হতে পারবে না")]
        [Display(Name = "Current Price")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue,
            ErrorMessage = "পুরনো দাম ঋণাত্মক হতে পারবে না")]
        [Display(Name = "Old Price")]
        public decimal? OldPrice { get; set; }

        [Range(0, 100,
            ErrorMessage = "ডিসকাউন্ট ০ থেকে ১০০ শতাংশের মধ্যে হতে হবে")]
        [Display(Name = "Discount Percent")]
        public int? DiscountPercent { get; set; }

        // ============================================================
        // Stock
        // ============================================================

        [Range(0, int.MaxValue,
            ErrorMessage = "স্টক ঋণাত্মক হতে পারবে না")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        // ============================================================
        // Sizes
        // ============================================================

        [StringLength(500)]
        [Display(Name = "Available Sizes")]
        public string SizesString { get; set; } = "M,L,XL,XXL";

        // ============================================================
        // Description
        // ============================================================

        [StringLength(1000, ErrorMessage = "ছোট বর্ণনা ১০০০ অক্ষরের কম হতে হবে")]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Key Features")]
        public string? KeyFeaturesString { get; set; }

        [Display(Name = "Why You Will Love It")]
        public string? WhyYouWillLoveItString { get; set; }

        [Display(Name = "Color Disclaimer")]
        public string? ColorDisclaimer { get; set; }

        // ============================================================
        // 🖼️ MAIN IMAGE UPLOAD
        // ============================================================
        /// <summary>
        /// নতুন মেইন ইমেজ ফাইল (Add-এ বাধ্যতামূলক, Edit-এ ঐচ্ছিক)।
        /// শুধু JPG, JPEG, PNG, WEBP, GIF গ্রহণ করা হবে।
        /// </summary>
        [Display(Name = "Main Image")]
        public IFormFile? MainImageFile { get; set; }

        /// <summary>
        /// বর্তমান মেইন ইমেজের path (Edit-এর সময় পুরনো ইমেজ দেখানোর জন্য)।
        /// উদাহরণ: "/uploads/products/abc123.jpg"
        /// </summary>
        public string? ExistingImageUrl { get; set; }

        // ============================================================
        // 📏 SIZE CHART IMAGE UPLOAD
        // ============================================================
        /// <summary>
        /// নতুন সাইজ চার্ট ইমেজ ফাইল (ঐচ্ছিক)।
        /// </summary>
        [Display(Name = "Size Chart")]
        public IFormFile? SizeChartFile { get; set; }

        /// <summary>
        /// বর্তমান সাইজ চার্টের path (Edit-এর সময় দেখানোর জন্য)।
        /// </summary>
        public string? ExistingSizeChartUrl { get; set; }

        // ============================================================
        // 🖼️ GALLERY IMAGES UPLOAD (একাধিক)
        // ============================================================
        /// <summary>
        /// নতুন গ্যালারি ইমেজসমূহ (ঐচ্ছিক, একাধিক)।
        /// Edit-এর সময় নতুনগুলো পুরনোগুলোর সাথে যোগ হবে।
        /// </summary>
        [Display(Name = "Gallery Images")]
        public List<IFormFile>? GalleryFiles { get; set; }

        /// <summary>
        /// বর্তমান গ্যালারি ইমেজের path সমূহ, কমা দিয়ে separated।
        /// উদাহরণ: "/uploads/gallery/abc.jpg,/uploads/gallery/def.jpg"
        /// </summary>
        public string? ExistingGalleryImagesString { get; set; }
    }
}