using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace CoffeePOS.Shared.Helpers;

public static class ImageHelper
{
    private static readonly HttpClient HttpClient = new();
    private static readonly ConcurrentDictionary<string, byte[]> _byteCache = new();

    public static async Task LoadImageAsync(AntdUI.Avatar avatar, string? imageIdentifier, string fallbackName, int colorSeed)
    {
        if (avatar.IsDisposed) return;

        SafeInvoke(avatar, () => avatar.Loading = true);

        try
        {
            // PERF: Dùng ConfigureAwait(false) để dứt khoát giải phóng UI Thread trong lúc chờ I/O mạng
            Bitmap? realImage = await TryFetchImageSafeAsync(imageIdentifier).ConfigureAwait(false);

            SafeInvoke(avatar, () =>
            {
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

                avatar.Loading = false;

                avatar.Refresh();
            });
        }
        catch
        {
            SafeInvoke(avatar, () =>
            {
                if (!avatar.IsDisposed)
                {
                    ApplyPlaceholder(avatar, fallbackName, colorSeed);
                    avatar.Loading = false;
                }
            });
        }
    }

    // WHY: Hàm Helper thần thánh đảm bảo mọi thao tác UI bắt buộc phải được nắn về luồng chính (Main UI Thread).
    private static void SafeInvoke(Control control, Action action)
    {
        // Tránh lỗi văng app nếu Form đã bị tắt trước khi ảnh tải xong
        if (control.IsDisposed || !control.IsHandleCreated) return;

        if (control.InvokeRequired)
        {
            control.BeginInvoke(action);
        }
        else
        {
            action();
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
                // Gọi API lấy mảng Byte
                imageBytes = await HttpClient.GetByteArrayAsync(url);
                _byteCache.TryAdd(url, imageBytes);
            }
            else
            {
                imageBytes = cachedBytes;
            }

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
