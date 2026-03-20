using System.Net.Http;
using FontAwesome.Sharp;

namespace CoffeePOS.Features.Products;

public class UC_ProductItem : UserControl
{
    // UI Components
    private IconPictureBox _iconFood = null!;
    private Label _lblName = null!;
    private Label _lblPrice = null!;

    // Data
    public int ProductId { get; private set; }
    public new string ProductName { get; private set; }
    public decimal Price { get; private set; }
    private string? ImageUrl { get; set; }

    private static readonly HttpClient HttpClient = new();

    public event EventHandler? OnProductClicked;

    public UC_ProductItem(int id, string name, decimal price, string? imageUrl = null)
    {
        ProductId = id;
        ProductName = name;
        Price = price;
        ImageUrl = imageUrl;

        InitializeUI();

        BindClickRecursive(this);
    }

    private void InitializeUI()
    {
        Size = new Size(130, 180);
        BackColor = Color.White;
        Cursor = Cursors.Hand;
        BorderStyle = BorderStyle.FixedSingle;

        _iconFood = BuildFoodIcon();
        _lblName = BuildNameLabel(ProductName);
        _lblPrice = BuildPriceLabel(Price);

        Controls.Add(_lblName);
        Controls.Add(_iconFood);
        Controls.Add(_lblPrice);
    }

    private static Label BuildNameLabel(string name)
    {
        return new()
        {
            Text = name,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoEllipsis = true,
        };
    }

    private static Label BuildPriceLabel(decimal price)
    {
        return new()
        {
            Text = $"{price:N0} đ",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60)
        };
    }

    private static IconPictureBox BuildFoodIcon()
    {
        return new IconPictureBox
        {
            IconChar = IconChar.Spinner,
            IconSize = 40,
            IconColor = Color.Gray,
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(245, 245, 245),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
    }

    public async void LoadImageAsync()
    {
        // Chạy task ngầm để không đơ UI
        await Task.Run(async () =>
        {
            Bitmap realImage = await TryLoadImageFromUrlOrPathAsync(ImageUrl) ?? CreatePlaceholderImage(ProductName, ProductId);

            // 3. Cập nhật UI (Bắt buộc phải Invoke vì đang ở thread khác)
            if (!IsDisposed && IsHandleCreated)
            {
                Invoke(() =>
                {
                    _iconFood.IconChar = IconChar.None; // Tắt icon loading
                    _iconFood.Image = realImage;        // Gắn ảnh thật
                    _iconFood.SizeMode = PictureBoxSizeMode.StretchImage; // Full ảnh
                });
            }
        });
    }

    private static async Task<Bitmap?> TryLoadImageFromUrlOrPathAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        try
        {
            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                byte[] bytes = await HttpClient.GetByteArrayAsync(uri);
                using var ms = new MemoryStream(bytes);
                using var image = Image.FromStream(ms);
                return new Bitmap(image);
            }

            if (File.Exists(imageUrl))
            {
                using var fs = new FileStream(imageUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var image = Image.FromStream(fs);
                return new Bitmap(image);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static Bitmap CreatePlaceholderImage(string productName, int colorSeed)
    {
        Bitmap placeholder = new(100, 100);
        using Graphics g = Graphics.FromImage(placeholder);

        Random rnd = new(colorSeed);
        Color randomColor = Color.FromArgb(rnd.Next(200, 255), rnd.Next(200, 255), rnd.Next(200, 255));
        g.Clear(randomColor);

        string initials = string.IsNullOrWhiteSpace(productName) ? "?" : productName[..1].ToUpperInvariant();
        g.DrawString(initials, new Font("Arial", 30, FontStyle.Bold), Brushes.DimGray, 35, 25);

        return placeholder;
    }

    private void BindClickRecursive(Control ctrl)
    {
        ctrl.Click += (s, e) => OnProductClicked?.Invoke(this, EventArgs.Empty);
        foreach (Control child in ctrl.Controls) BindClickRecursive(child);
    }
}
