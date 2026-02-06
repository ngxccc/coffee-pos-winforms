namespace CoffeePOS.Features.Products;

public class UC_Menu : UserControl
{
    private readonly FlowLayoutPanel _flowProducts;

    public event Action<int, string, decimal>? OnProductSelected;

    public event EventHandler? OnBackClicked;

    public UC_Menu()
    {
        Dock = DockStyle.Fill;

        Panel pnlHeader = new() { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };

        Button btnBack = new()
        {
            Text = "Quay lại Bàn",
            Dock = DockStyle.Right,
            Width = 120,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(230, 230, 230)
        };
        btnBack.Click += (s, e) => OnBackClicked?.Invoke(this, EventArgs.Empty);

        // (Sau này thêm các nút Lọc Cafe, Trà, Bánh ở đây)

        pnlHeader.Controls.Add(btnBack);

        _flowProducts = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(245, 245, 245)
        };

        Controls.Add(_flowProducts);
        Controls.Add(pnlHeader);

        LoadMockProducts();
    }

    private void LoadMockProducts()
    {
        for (int i = 1; i <= 15; i++)
        {
            var p = new UC_ProductItem(i, $"Món ngon {i}", 25000 + (i * 1000));

            p.OnProductClicked += (s, e) =>
            {
                OnProductSelected?.Invoke(p.ProductId, p.ProductName, p.Price);
            };

            _flowProducts.Controls.Add(p);
        }
    }
}
