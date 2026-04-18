using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_ProductItem : UserControl
{
    public int ProductId { get; private set; }
    public new string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageIdentifier { get; private set; }

    public event EventHandler? OnProductClicked;

    public UC_ProductItem(int id, string name, decimal price, string? imageIdentifier = null)
    {
        ProductId = id;
        ProductName = name;
        Price = price;
        ImageIdentifier = imageIdentifier;

        InitializeComponent();
        BindData();
        WireEvents();
    }

    private void BindData()
    {
        _lblName.Text = ProductName;
        _lblPrice.Text = $"{Price:N0} đ";
    }

    private void WireEvents()
    {
        // PERF: Chuyển toàn bộ sự kiện click của các con về item chính
        // AntdUI Panel và Avatar bắt sự kiện rất nhạy
        _pnlCard.Click += HandleClick;
        _picImage.Click += HandleClick;
        _lblName.Click += HandleClick;
        _lblPrice.Click += HandleClick;

        // _pnlCard.MouseEnter += (s, e) =>
        // {
        //     _pnlCard.Shadow = 12;
        //     _pnlCard.BorderWidth = 1;
        //     _pnlCard.BorderColor = UiTheme.BrandPrimary;
        // };
        // _pnlCard.MouseLeave += (s, e) =>
        // {
        //     _pnlCard.Shadow = 6;
        //     _pnlCard.BorderWidth = 0;
        // };
    }

    private void HandleClick(object? sender, EventArgs e)
    {
        OnProductClicked?.Invoke(this, EventArgs.Empty);
    }

    public async void LoadImageAsync()
    {
        if (string.IsNullOrEmpty(ImageIdentifier))
        {
            // Nếu không có ảnh, Avatar sẽ tự hiển thị ký tự đầu của tên
            _picImage.Text = ProductName[..1].ToUpper();
            return;
        }

        try
        {
            await ImageHelper.LoadImageAsync(_picImage, ImageIdentifier, ProductName, ProductId);
        }
        catch
        {
            _picImage.Text = "!";
        }
    }
}
