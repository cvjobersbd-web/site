namespace al_ibtisam.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Brand { get; set; } = "Believers";
        public bool InStock { get; set; } = true;
        public string[] Sizes { get; set; } = new[] { "M", "L", "XL", "XXL" };
        public string[]? GalleryImages { get; set; }

        // ============ নতুন প্রপার্টি ============
        public string? ShortDescription { get; set; }         // ছোট বর্ণনা
        public string? SizeChartImageUrl { get; set; }        // সাইজ চার্টের ছবি
        public string? Fabric { get; set; }                   // Fabric
        public string? Pattern { get; set; }                  // Pattern
        public string? CollarType { get; set; }               // Collar Type
        public string? Closure { get; set; }                  // Closure
        public string? FitType { get; set; }                  // Fit Type
        public string? SleeveType { get; set; }               // Sleeve Type
        public string[]? KeyFeatures { get; set; }            // Key Features লিস্ট
        public string[]? WhyYouWillLoveIt { get; set; }       // Why You'll Love It লিস্ট
        public string? ColorDisclaimer { get; set; }          // Color Disclaimer
    }
}