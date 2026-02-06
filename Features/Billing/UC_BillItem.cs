using FontAwesome.Sharp;

namespace CoffeePOS.Features.Billing;

public class UC_BillItem : Panel
{
    private readonly Label lblCount;
    private int _quantity;
    private readonly decimal _unitPrice;
    private readonly Label lblPrice;

    public event EventHandler<decimal>? OnAmountChanged;
    public event EventHandler<UC_BillItem>? OnDeleteRequest;
    public decimal TotalValue => _quantity * _unitPrice;

    public UC_BillItem(string foodName, int count, decimal price, Image foodImage)
    {
        _quantity = count;
        _unitPrice = price;

        Size = new Size(400, 90);
        BackColor = Color.White;
        Padding = new Padding(5);
        Margin = new Padding(0, 0, 0, 10);

        PictureBox picFood = new()
        {
            Image = foodImage,
            SizeMode = PictureBoxSizeMode.StretchImage,
            Size = new Size(90, 90),
            Dock = DockStyle.Left,
            Cursor = Cursors.Hand,
            Padding = new Padding(0, 0, 5, 0),
        };

        Panel pnlQty = new()
        {
            Dock = DockStyle.Left,
            Width = 80,
            Padding = new Padding(0, 20, 0, 20)
        };

        IconButton btnMinus = CreateQtyButton(IconChar.Minus);
        IconButton btnPlus = CreateQtyButton(IconChar.Plus);

        lblCount = new Label
        {
            Text = $"{_quantity}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

        // Event dummy
        btnPlus.Click += (s, e) => UpdateQty(1);
        btnMinus.Click += (s, e) => UpdateQty(-1);

        pnlQty.Controls.Add(lblCount);
        pnlQty.Controls.Add(btnMinus);
        pnlQty.Controls.Add(btnPlus);

        // GIÁ & XÓA
        Panel pnlRightActions = new()
        {
            Dock = DockStyle.Right,
            Width = 100,
            BackColor = Color.Transparent
        };

        IconButton btnDelete = new()
        {
            IconChar = IconChar.TrashAlt,
            IconSize = 18,
            IconColor = Color.Red,
            Dock = DockStyle.Right,
            Width = 30,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += (s, e) => OnDeleteRequest?.Invoke(this, this);

        lblPrice = new Label
        {
            Text = $"{price:N0}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
        };

        pnlRightActions.Controls.Add(lblPrice);
        pnlRightActions.Controls.Add(btnDelete);

        // TÊN MÓN
        Label lblName = new()
        {
            Text = foodName,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Padding = new Padding(5, 0, 0, 0),
            AutoEllipsis = true,
        };

        // Dock Fill (Name) add cuối cùng
        // Dock Left/Right add trước

        Controls.Add(lblName);         // Fill: Lấp đầy khoảng trống còn lại
        Controls.Add(pnlRightActions); // Right: Giá
        Controls.Add(pnlQty);          // Left 2: Số lượng (nằm sau ảnh)
        Controls.Add(picFood);         // Left 1: Ảnh (nằm ngoài cùng bên trái)
    }

    private void UpdateQty(int delta)
    {
        int oldQty = _quantity;

        _quantity += delta;
        if (_quantity < 1) _quantity = 1;

        if (oldQty == _quantity) return;

        lblCount.Text = $"{_quantity}";
        lblPrice.Text = $"{TotalValue:N0}";

        decimal moneyDiff = delta * _unitPrice;

        OnAmountChanged?.Invoke(this, moneyDiff);
    }

    // Helper tạo nút tròn nhỏ
    private static IconButton CreateQtyButton(IconChar icon)
    {
        var btn = new IconButton
        {
            IconChar = icon,
            IconSize = 12,
            IconColor = Color.Black,
            Width = 25,
            Dock = (icon == IconChar.Minus) ? DockStyle.Left : DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(240, 240, 240),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}
