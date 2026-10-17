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
        public DbSet<Product> Products { get; set; } // ⬅️ নতুন যোগ করা হলো

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
                // ---------- Table Name ----------
                entity.ToTable("Users");

                // ---------- Primary Key ----------
                entity.HasKey(u => u.Id);

                // ---------- Auto Increment Id ----------
                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd();

                // ---------- Full Name ----------
                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnName("full_name");

                // ---------- Email ----------
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150)
                      .HasColumnName("email");

                // Email Unique Index
                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("ix_users_email_unique");

                // ---------- Phone ----------
                entity.Property(u => u.Phone)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasColumnName("phone");

                // Phone Unique Index
                entity.HasIndex(u => u.Phone)
                      .IsUnique()
                      .HasDatabaseName("ix_users_phone_unique");

                // ---------- Password Hash ----------
                entity.Property(u => u.PasswordHash)
                      .IsRequired()
                      .HasColumnName("password_hash");

                // ---------- Created At ----------
                entity.Property(u => u.CreatedAt)
                      .IsRequired()
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // ---------- Last Login At ----------
                entity.Property(u => u.LastLoginAt)
                      .HasColumnName("last_login_at");

                // ---------- Is Active ----------
                entity.Property(u => u.IsActive)
                      .IsRequired()
                      .HasColumnName("is_active")
                      .HasDefaultValue(true);

                // ---------- Profile Image URL ----------
                entity.Property(u => u.ProfileImageUrl)
                      .HasMaxLength(500)
                      .HasColumnName("profile_image_url");

                // ---------- Address ----------
                entity.Property(u => u.Address)
                      .HasMaxLength(500)
                      .HasColumnName("address");

                // ---------- District ----------
                entity.Property(u => u.District)
                      .HasMaxLength(100)
                      .HasColumnName("district");

                // ---------- Role ----------
                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasColumnName("role")
                      .HasDefaultValue("Customer");

                // ---------- Email Verified ----------
                entity.Property(u => u.EmailVerified)
                      .IsRequired()
                      .HasColumnName("email_verified")
                      .HasDefaultValue(false);
            });

            // --------------------------------------------------------
            // Product Configuration (⬅️ নতুন যোগ করা হলো)
            // --------------------------------------------------------
            modelBuilder.Entity<Product>(entity =>
            {
                // ---------- Table Name ----------
                entity.ToTable("Products");

                // ---------- Primary Key ----------
                entity.HasKey(p => p.Id);

                // ---------- Auto Increment Id ----------
                entity.Property(p => p.Id)
                      .ValueGeneratedOnAdd();

                // ---------- Name ----------
                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                // ---------- Image Url ----------
                entity.Property(p => p.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(1000);

                // ---------- Price ----------
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                // ---------- Old Price ----------
                entity.Property(p => p.OldPrice)
                      .HasColumnType("decimal(18,2)");

                // ---------- Category Name ----------
                entity.Property(p => p.CategoryName)
                      .IsRequired()
                      .HasMaxLength(100);

                // ---------- Brand ----------
                entity.Property(p => p.Brand)
                      .HasMaxLength(100)
                      .HasDefaultValue("Believers");

                // ---------- In Stock ----------
                entity.Property(p => p.InStock)
                      .HasDefaultValue(true);

                // ---------- Sizes String ----------
                entity.Property(p => p.SizesString)
                      .HasMaxLength(500);

                // ---------- Gallery Images String ----------
                entity.Property(p => p.GalleryImagesString)
                      .HasMaxLength(2000);

                // ---------- Short Description ----------
                entity.Property(p => p.ShortDescription)
                      .HasMaxLength(1000);

                // ---------- Size Chart Image Url ----------
                entity.Property(p => p.SizeChartImageUrl)
                      .HasMaxLength(1000);
            });
        }
    }
}