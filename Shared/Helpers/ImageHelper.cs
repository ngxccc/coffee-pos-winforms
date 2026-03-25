using FontAwesome.Sharp;
using Serilog;

namespace CoffeePOS.Shared.Helpers;

public static class ImageHelper
{
    private static readonly HttpClient HttpClient = new();
    private static readonly string BaseImageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");

    public static async Task LoadImageAsync(IconPictureBox pictureBox, string? imageIdentifier, string fallbackName, int colorSeed)
    {
        if (pictureBox.IsDisposed) return;

        pictureBox.IconChar = IconChar.Spinner;
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

        try
        {
            Bitmap? realImage = await TryFetchImageSafeAsync(imageIdentifier);

            if (pictureBox.IsDisposed)
            {
                realImage?.Dispose(); // Tránh leak nếu user đóng form lúc đang tải
                return;
            }

            if (realImage != null)
            {
                pictureBox.IconChar = IconChar.None;

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
            string urlOrPath = identifier.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? identifier
                : Path.Combine(BaseImageDir, identifier);

            byte[] imageBytes;

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

            using var ms = new MemoryStream(imageBytes);
            using var originalImage = Image.FromStream(ms);
            return new Bitmap(originalImage); // Deep copy an toàn
        }
        catch
        {
            return null;
        }
    }

    private static void ApplyPlaceholder(IconPictureBox pictureBox, string text, int seed)
    {
        pictureBox.IconChar = IconChar.None;
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

        var oldImage = pictureBox.Image;
        pictureBox.Image = CreatePlaceholderImage(text, seed);
        oldImage?.Dispose();
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
