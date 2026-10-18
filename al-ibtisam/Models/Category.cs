using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace al_ibtisam.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Navbar-এ দেখাবে কি না</summary>
        [Column("show_in_navbar")]
        public bool ShowInNavbar { get; set; } = true;

        /// <summary>ক্রম (ছোট আগে দেখাবে)</summary>
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>Special style (Flash Sale এর জন্য)</summary>
        [Column("is_flash_sale")]
        public bool IsFlashSale { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}