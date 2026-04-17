using System.Collections.Concurrent;
using Serilog;

namespace CoffeePOS.Shared.Helpers;

public static class ImageHelper
{
    private static readonly HttpClient HttpClient = new();
    private static readonly string BaseImageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");

    // PERF: Thread-safe In-Memory Cache lưu mảng byte để chống giật lag và tiết kiệm băng thông
    private static readonly ConcurrentDictionary<string, byte[]> _byteCache = new();

    public static async Task LoadImageAsync(PictureBox pictureBox, string? imageIdentifier, string fallbackName, int colorSeed)
    {
        if (pictureBox.IsDisposed) return;

        var oldLoading = pictureBox.Image;
        pictureBox.Image = CreateLoadingPlaceholderImage();
        oldLoading?.Dispose();
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

        try
        {
            Bitmap? realImage = await TryFetchImageSafeAsync(imageIdentifier);

            if (pictureBox.IsDisposed)
            {
                realImage?.Dispose();
                return;
            }

            if (realImage != null)
            {
                var oldImage = pictureBox.Image;
                pictureBox.Image = realImage;
                oldImage?.Dispose();

                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                ApplyPlaceholder(pictureBox, fallbackName, colorSeed);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Image Error] Lỗi tải {fallbackName}: {ex.Message}");
            if (!pictureBox.IsDisposed) ApplyPlaceholder(pictureBox, fallbackName, colorSeed);
        }
    }

    private static async Task<Bitmap?> TryFetchImageSafeAsync(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return null;

        try
        {
            byte[] imageBytes;

            // 1. Kiểm tra RAM trước (Cache Hit)
            if (_byteCache.TryGetValue(identifier, out var cachedBytes))
            {
                imageBytes = cachedBytes;
            }
            else
            {
                // 2. Không có trong RAM thì mới đi tải (Cache Miss)
                string urlOrPath = identifier.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? identifier
                    : Path.Combine(BaseImageDir, identifier);

                if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out Uri? uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    imageBytes = await HttpClient.GetByteArrayAsync(uri);
                }
                else if (File.Exists(urlOrPath))
                {
                    imageBytes = await File.ReadAllBytesAsync(urlOrPath);
                }
                else
                {
                    return null;
                }

                // Lưu vào RAM cho lần sau
                _byteCache.TryAdd(identifier, imageBytes);
            }

            // Mất vài ms để decode byte[] thành Bitmap nhưng cực kỳ an toàn cho GDI+
            using var ms = new MemoryStream(imageBytes);
            using var originalImage = Image.FromStream(ms);
            return new Bitmap(originalImage);
        }
        catch
        {
            return null;
        }
    }

    private static void ApplyPlaceholder(PictureBox pictureBox, string text, int seed)
    {
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

        var oldImage = pictureBox.Image;
        pictureBox.Image = CreatePlaceholderImage(text, seed);
        oldImage?.Dispose();
    }

    private static Bitmap CreateLoadingPlaceholderImage()
    {
        Bitmap loading = new(100, 100);
        using Graphics g = Graphics.FromImage(loading);
        g.Clear(Color.FromArgb(245, 245, 245));
        g.DrawString("...", new Font("Segoe UI", 22, FontStyle.Bold), Brushes.Gray, 30, 28);
        return loading;
    }

    public static Bitmap CreatePlaceholderImage(string text, int colorSeed)
    {
        Bitmap placeholder = new(100, 100);
        using Graphics g = Graphics.FromImage(placeholder);

        Random rnd = new(colorSeed);
        Color randomColor = Color.FromArgb(rnd.Next(200, 255), rnd.Next(200, 255), rnd.Next(200, 255));
        g.Clear(randomColor);

        string initials = string.IsNullOrWhiteSpace(text) ? "?" : text[..1].ToUpperInvariant();
        g.DrawString(initials, new Font("Arial", 30, FontStyle.Bold), Brushes.DimGray, 35, 25);

        return placeholder;
    }
}
