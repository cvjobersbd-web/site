using Microsoft.AspNetCore.Http;

namespace al_ibtisam.Services
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string folder);
        void DeleteImage(string? imagePath);
        void EnsureUploadFolders(); // ⬅️ নতুন — অ্যাপ স্টার্টে অটো ফোল্ডার তৈরি
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageService> _logger;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        // ============================================================
        // অ্যাপ স্টার্টে সব প্রয়োজনীয় ফোল্ডার তৈরি করে
        // ============================================================
        public void EnsureUploadFolders()
        {
            try
            {
                var root = _env.WebRootPath;

                // WebRootPath কখনো null হয় না যদি wwwroot থাকে
                if (string.IsNullOrEmpty(root))
                {
                    _logger.LogWarning("WebRootPath is null — wwwroot ফোল্ডার তৈরি করা হচ্ছে।");
                    root = Path.Combine(_env.ContentRootPath, "wwwroot");
                }

                var folders = new[]
                {
                    Path.Combine(root, "uploads"),
                    Path.Combine(root, "uploads", "products"),
                    Path.Combine(root, "uploads", "gallery"),
                    Path.Combine(root, "uploads", "sizecart")
                };

                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                        _logger.LogInformation($"✅ ফোল্ডার তৈরি: {folder}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EnsureUploadFolders error: {ex.Message}");
            }
        }

        // ============================================================
        // একক ইমেজ আপলোড
        // ============================================================
        public async Task<string?> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning($"File too large: {file.FileName}");
                return null;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning($"Invalid extension: {ext}");
                return null;
            }

            try
            {
                var root = _env.WebRootPath;
                if (string.IsNullOrEmpty(root))
                    root = Path.Combine(_env.ContentRootPath, "wwwroot");

                var uploadFolder = Path.Combine(root, "uploads", folder);

                // ✅ ফোল্ডার না থাকলে অটো তৈরি
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var uniqueName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{folder}/{uniqueName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload error: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // একাধিক ইমেজ আপলোড
        // ============================================================
        public async Task<List<string>> UploadMultipleImagesAsync(
            List<IFormFile> files, string folder)
        {
            var paths = new List<string>();
            if (files == null || files.Count == 0) return paths;

            foreach (var file in files)
            {
                var path = await UploadImageAsync(file, folder);
                if (!string.IsNullOrEmpty(path))
                    paths.Add(path);
            }
            return paths;
        }

        // ============================================================
        // ইমেজ ডিলিট
        // ============================================================
        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            if (!imagePath.StartsWith("/uploads/")) return;

            try
            {
                var root = _env.WebRootPath;
                if (string.IsNullOrEmpty(root))
                    root = Path.Combine(_env.ContentRootPath, "wwwroot");

                var fullPath = Path.Combine(
                    root,
                    imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Delete error: {ex.Message}");
            }
        }
    }
}