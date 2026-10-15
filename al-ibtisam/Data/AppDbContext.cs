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
        }
    }
}