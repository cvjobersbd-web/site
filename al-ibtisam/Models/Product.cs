using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "প্রোডাক্টের নাম অবশ্যই দিতে হবে")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "মেইন ইমেজের লিঙ্ক দিতে হবে")]
        [Url(ErrorMessage = "সঠিক ইমেজ URL দিন")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "প্রাইস দিতে হবে")]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int? DiscountPercent { get; set; }

        [Required(ErrorMessage = "ক্যাটাগরি সিলেক্ট করতে হবে")]
        public string CategoryName { get; set; } = string.Empty;

        public string Brand { get; set; } = "Believers";

        public bool InStock { get; set; } = true;

        // ============================================================
        // Database String Properties
        // ============================================================
        public string SizesString { get; set; } = "M,L,XL,XXL";
        public string? GalleryImagesString { get; set; }

        // নতুন যোগ করা প্রপার্টি (যা এরর দিচ্ছিল)
        public string? KeyFeaturesString { get; set; }
        public string? WhyYouWillLoveItString { get; set; }

        // ============================================================
        // Descriptions & Extra Details
        // ============================================================
        [StringLength(1000)]
        public string? ShortDescription { get; set; }

        public string? SizeChartImageUrl { get; set; }

        public string? ColorDisclaimer { get; set; }

        // ============================================================
        // [NotMapped] Properties (ডাটাবেজে সেভ হবে না, শুধু ভিউতে দেখাবে)
        // ============================================================

        [NotMapped]
        public string[] Sizes => string.IsNullOrEmpty(SizesString)
            ? Array.Empty<string>()
            : SizesString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        [NotMapped]
        public string[] GalleryImages => string.IsNullOrEmpty(GalleryImagesString)
            ? Array.Empty<string>()
            : GalleryImagesString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        [NotMapped]
        public string[] KeyFeatures => string.IsNullOrEmpty(KeyFeaturesString)
            ? Array.Empty<string>()
            : KeyFeaturesString.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        [NotMapped]
        public string[] WhyYouWillLoveIt => string.IsNullOrEmpty(WhyYouWillLoveItString)
            ? Array.Empty<string>()
            : WhyYouWillLoveItString.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}