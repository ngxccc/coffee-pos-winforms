using CoffeePOS.Shared.Helpers;
using Serilog;

namespace CoffeePOS.Features.Products;

public class UC_ProductItem : UserControl
{
    // UI Components
    private PictureBox _iconFood = null!;
    private Label _lblName = null!;
    private Label _lblPrice = null!;

    // Data
    public int ProductId { get; private set; }
    public new string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageIdentifier { get; private set; }

    private static readonly HttpClient HttpClient = new();

    public event EventHandler? OnProductClicked;

    public UC_ProductItem(int id, string name, decimal price, string? imageIdentifier = null)
    {
        ProductId = id;
        ProductName = name;
        Price = price;
        ImageIdentifier = imageIdentifier;

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

    private static PictureBox BuildFoodIcon()
    {
        return new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(245, 245, 245),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
    }

    public async void LoadImageAsync()
    {
        _ = ImageHelper.LoadImageAsync(_iconFood, ImageIdentifier, ProductName, ProductId);
    }

    private void BindClickRecursive(Control ctrl)
    {
        ctrl.Click += (s, e) => OnProductClicked?.Invoke(this, EventArgs.Empty);
        foreach (Control child in ctrl.Controls) BindClickRecursive(child);
    }
}
