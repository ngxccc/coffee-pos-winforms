using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace CoffeePOS.Shared.Helpers;

public static class ImageHelper
{
    private static readonly HttpClient HttpClient = new();

    // PERF: Thread-safe In-Memory Cache O(1) lookup to prevent redundant I/O and flickering
    private static readonly ConcurrentDictionary<string, byte[]> _byteCache = new();

    public static async Task LoadImageAsync(AntdUI.Avatar avatar, string? imageIdentifier, string fallbackName, int colorSeed)
    {
        if (avatar.IsDisposed) return;

        // WHY: Use built-in Loading state instead of manual "..." bitmaps
        avatar.Loading = true;

        try
        {
            Bitmap? realImage = await TryFetchImageSafeAsync(imageIdentifier);

            if (avatar.IsDisposed)
            {
                realImage?.Dispose();
                return;
            }

            if (realImage != null)
            {
                var oldImage = avatar.Image;
                avatar.Image = realImage;
                oldImage?.Dispose();
            }
            else
            {
                ApplyPlaceholder(avatar, fallbackName, colorSeed);
            }
        }
        catch
        {
            ApplyPlaceholder(avatar, fallbackName, colorSeed);
        }
        finally
        {
            if (!avatar.IsDisposed) avatar.Loading = false;
        }
    }

    private static void ApplyPlaceholder(AntdUI.Avatar avatar, string text, int seed)
    {
        var oldImage = avatar.Image;
        avatar.Image = CreatePlaceholderImage(text, seed);
        oldImage?.Dispose();
    }

    public static Bitmap CreatePlaceholderImage(string text, int colorSeed)
    {
        // WHY: Standard square size, AntdUI.Avatar will handle the Radius/Circle clipping
        Bitmap placeholder = new(100, 100);
        using Graphics g = Graphics.FromImage(placeholder);

        g.SmoothingMode = SmoothingMode.AntiAlias;

        Random rnd = new(colorSeed);
        Color randomColor = Color.FromArgb(rnd.Next(150, 230), rnd.Next(150, 230), rnd.Next(150, 230));
        g.Clear(randomColor);

        string initials = string.IsNullOrWhiteSpace(text) ? "?" : text[..1].ToUpper();

        using Font font = new("Segoe UI", 36, FontStyle.Bold);
        using Brush brush = new SolidBrush(Color.White);

        StringFormat sf = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        g.DrawString(initials, font, brush, new Rectangle(0, 0, 100, 100), sf);

        return placeholder;
    }

    private static async Task<Bitmap?> TryFetchImageSafeAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            byte[] imageBytes;
            if (!_byteCache.TryGetValue(url, out byte[]? cachedBytes))
            {
                imageBytes = await HttpClient.GetByteArrayAsync(url);
                _byteCache.TryAdd(url, imageBytes);
            }
            else
            {
                imageBytes = cachedBytes;
            }

            // WHY: Convert to Bitmap in a GDI+ safe way via MemoryStream
            using var ms = new MemoryStream(imageBytes);
            using var originalImage = Image.FromStream(ms);
            return new Bitmap(originalImage);
        }
        catch
        {
            return null;
        }
    }
}
