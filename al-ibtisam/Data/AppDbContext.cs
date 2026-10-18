using al_ibtisam.Models;
using Microsoft.EntityFrameworkCore;

namespace al_ibtisam.Data
{
    /// <summary>
    /// Entity Framework Core DbContext।
    /// PostgreSQL ডাটাবেজের সাথে সংযোগ স্থাপন করে।
    /// </summary>
    public class AppDbContext : DbContext
    {
        // ============================================================
        // Constructor
        // ============================================================
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ============================================================
        // DbSets (Tables)
        // ============================================================

        /// <summary>
        /// Users টেবিল — অ্যাকাউন্টের তথ্য
        /// </summary>
        public DbSet<ApplicationUser> Users { get; set; }

        /// <summary>
        /// Products টেবিল — প্রোডাক্টের তথ্য
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Categories টেবিল — ক্যাটাগরি ও Navbar কন্ট্রোল
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// HeroSlides টেবিল — হোম পেইজের হিরো স্লাইডার ইমেজ ও ক্যাপশন
        /// </summary>
        public DbSet<HeroSlide> HeroSlides { get; set; }

        /// <summary>
        /// SiteSettings টেবিল — গ্লোবাল key/value সেটিংস (যেমন: Slider Interval)
        /// </summary>
        public DbSet<SiteSetting> SiteSettings { get; set; }

        // ============================================================
        // Model Configuration
        // ============================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------------------------------------------
            // ApplicationUser Configuration
            // --------------------------------------------------------
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnName("full_name");

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150)
                      .HasColumnName("email");

                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("ix_users_email_unique");

                entity.Property(u => u.Phone)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasColumnName("phone");

                entity.HasIndex(u => u.Phone)
                      .IsUnique()
                      .HasDatabaseName("ix_users_phone_unique");

                entity.Property(u => u.PasswordHash)
                      .IsRequired()
                      .HasColumnName("password_hash");

                entity.Property(u => u.CreatedAt)
                      .IsRequired()
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(u => u.LastLoginAt)
                      .HasColumnName("last_login_at");

                entity.Property(u => u.IsActive)
                      .IsRequired()
                      .HasColumnName("is_active")
                      .HasDefaultValue(true);

                entity.Property(u => u.ProfileImageUrl)
                      .HasMaxLength(500)
                      .HasColumnName("profile_image_url");

                entity.Property(u => u.Address)
                      .HasMaxLength(500)
                      .HasColumnName("address");

                entity.Property(u => u.District)
                      .HasMaxLength(100)
                      .HasColumnName("district");

                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasColumnName("role")
                      .HasDefaultValue("Customer");

                entity.Property(u => u.EmailVerified)
                      .IsRequired()
                      .HasColumnName("email_verified")
                      .HasDefaultValue(false);
            });

            // --------------------------------------------------------
            // Product Configuration
            // --------------------------------------------------------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(p => p.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.OldPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.CategoryName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Brand)
                      .HasMaxLength(100)
                      .HasDefaultValue("Believers");

                entity.Property(p => p.StockQuantity)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.Property(p => p.InStock)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(p => p.SizesString)
                      .HasMaxLength(500);

                entity.Property(p => p.GalleryImagesString)
                      .HasMaxLength(2000);

                entity.Property(p => p.ShortDescription)
                      .HasMaxLength(1000);

                entity.Property(p => p.SizeChartImageUrl)
                      .HasMaxLength(1000);

                entity.Property(p => p.ColorDisclaimer)
                      .HasMaxLength(1000);

                entity.Property(p => p.KeyFeaturesString)
                      .HasMaxLength(4000);

                entity.Property(p => p.WhyYouWillLoveItString)
                      .HasMaxLength(4000);
            });

            // --------------------------------------------------------
            // Category Configuration
            // --------------------------------------------------------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnName("name");

                entity.HasIndex(c => c.Name)
                      .IsUnique()
                      .HasDatabaseName("ix_categories_name_unique");

                entity.Property(c => c.ShowInNavbar)
                      .IsRequired()
                      .HasColumnName("show_in_navbar")
                      .HasDefaultValue(true);

                entity.Property(c => c.DisplayOrder)
                      .IsRequired()
                      .HasColumnName("display_order")
                      .HasDefaultValue(0);

                entity.Property(c => c.IsFlashSale)
                      .IsRequired()
                      .HasColumnName("is_flash_sale")
                      .HasDefaultValue(false);

                entity.Property(c => c.CreatedAt)
                      .IsRequired()
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // --------------------------------------------------------
            // HeroSlide Configuration (নতুন)
            // --------------------------------------------------------
            modelBuilder.Entity<HeroSlide>(entity =>
            {
                entity.ToTable("HeroSlides");
                entity.HasKey(h => h.Id);

                entity.Property(h => h.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(h => h.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(1000)
                      .HasColumnName("image_url");

                entity.Property(h => h.Label)
                      .HasMaxLength(100)
                      .HasColumnName("label");

                entity.Property(h => h.TitleLight)
                      .HasMaxLength(150)
                      .HasColumnName("title_light");

                entity.Property(h => h.TitleBold)
                      .HasMaxLength(150)
                      .HasColumnName("title_bold");

                entity.Property(h => h.Subtitle)
                      .HasMaxLength(300)
                      .HasColumnName("subtitle");

                entity.Property(h => h.IsActive)
                      .IsRequired()
                      .HasColumnName("is_active")
                      .HasDefaultValue(true);

                entity.Property(h => h.DisplayOrder)
                      .IsRequired()
                      .HasColumnName("display_order")
                      .HasDefaultValue(0);

                entity.Property(h => h.CreatedAt)
                      .IsRequired()
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // পারফরম্যান্স — active slides দ্রুত লোড হবে
                entity.HasIndex(h => new { h.IsActive, h.DisplayOrder })
                      .HasDatabaseName("ix_hero_slides_active_order");
            });

            // --------------------------------------------------------
            // SiteSetting Configuration (নতুন)
            // --------------------------------------------------------
            modelBuilder.Entity<SiteSetting>(entity =>
            {
                entity.ToTable("SiteSettings");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(s => s.SettingKey)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnName("setting_key");

                entity.HasIndex(s => s.SettingKey)
                      .IsUnique()
                      .HasDatabaseName("ix_site_settings_key_unique");

                entity.Property(s => s.SettingValue)
                      .HasMaxLength(500)
                      .HasColumnName("setting_value");

                entity.Property(s => s.UpdatedAt)
                      .IsRequired()
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}