using FontAwesome.Sharp;

namespace CoffeePOS.Features.Products;

public class UC_ProductItem : UserControl
{
    // Data
    public int ProductId { get; private set; }
    public new string ProductName { get; private set; }
    public decimal Price { get; private set; }

    public event EventHandler? OnProductClicked;

    public UC_ProductItem(int id, string name, decimal price)
    {
        ProductId = id;
        ProductName = name;
        Price = price;

        Size = new Size(130, 180);
        BackColor = Color.White;
        Cursor = Cursors.Hand;

        BorderStyle = BorderStyle.FixedSingle;

        IconPictureBox iconFood = new()
        {
            IconChar = IconChar.Coffee, // Hoặc MugHot
            IconSize = 60,
            IconColor = Color.FromArgb(108, 92, 231),
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(245, 245, 245),
            SizeMode = PictureBoxSizeMode.CenterImage
        };

        Label lblName = new()
        {
            Text = name,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(5)
        };

        Label lblPrice = new()
        {
            Text = $"{price:N0} đ",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60)
        };

        Controls.Add(lblName);
        Controls.Add(iconFood);
        Controls.Add(lblPrice);

        BindClickRecursive(this);
    }

    private void BindClickRecursive(Control ctrl)
    {
        ctrl.Click += (s, e) => OnProductClicked?.Invoke(this, EventArgs.Empty);
        foreach (Control child in ctrl.Controls) BindClickRecursive(child);
    }
}
