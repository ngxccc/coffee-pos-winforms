using FontAwesome.Sharp;

namespace CoffeePOS.Features.Billing;

public class UC_BillItem : Panel
{
    // --- 1. UI COMPONENTS (Class Level) ---
    private Label? _lblCount;
    private Label? _lblPrice;
    private Label? _lblNote;
    private Label? _lblName;

    // --- 2. DATA FIELDS ---
    private int _quantity;
    private readonly decimal _unitPrice;

    // --- 3. EVENTS ---
    public event EventHandler<decimal>? OnAmountChanged;
    public event EventHandler<UC_BillItem>? OnDeleteRequest;
    public event EventHandler<string>? OnNoteEditRequest;

    // --- 4. PROPERTIES ---
    public decimal TotalValue => _quantity * _unitPrice;
    public int ProductId { get; private set; }
    public string ItemName { get; private set; }
    public string Note { get; private set; }
    public int Quantity => _quantity;

    public UC_BillItem(int id, string foodName, int count, decimal price, string note = "", Image? foodImage = null)
    {
        // A. Gán Data
        ProductId = id;
        ItemName = foodName;
        _quantity = count;
        _unitPrice = price;
        Note = note;

        // B. Setup Container chính
        SetupMainContainer();

        // C. Tạo các mảnh ghép (Components)
        var picFood = BuildImagePanel(foodImage);
        var pnlQty = BuildQtyPanel();
        var pnlRight = BuildRightActionsPanel(price);
        var pnlInfo = BuildInfoPanel(foodName, note);

        // D. Ráp lại (Thứ tự quan trọng vì dùng Dock)
        // Add vào: Info (Fill) -> Right (Right) -> Qty (Left) -> Image (Left)
        Controls.Add(pnlInfo);
        Controls.Add(pnlRight);
        Controls.Add(pnlQty);
        Controls.Add(picFood);

        // E. Gán sự kiện DoubleClick thần thánh
        BindDoubleClickRecursive(this);
    }

    private void SetupMainContainer()
    {
        Size = new Size(400, 90);
        BackColor = Color.White;
        Padding = new Padding(5);
        Margin = new Padding(0, 0, 0, 10);
    }

    private static PictureBox BuildImagePanel(Image? img)
    {
        return new PictureBox
        {
            Image = img,
            SizeMode = PictureBoxSizeMode.StretchImage,
            Size = new Size(90, 90),
            Dock = DockStyle.Left,
            Cursor = Cursors.Hand,
            Padding = new Padding(0, 0, 5, 0),
        };
    }

    private Panel BuildQtyPanel()
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Left,
            Width = 80,
            Padding = new Padding(0, 20, 0, 20),
            Tag = "BLOCK_DOUBLE_CLICK"
        };

        var btnMinus = CreateQtyButton(IconChar.Minus);
        var btnPlus = CreateQtyButton(IconChar.Plus);

        _lblCount = new Label
        {
            Text = $"{_quantity}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

        btnPlus.Click += (s, e) => UpdateQty(1);
        btnMinus.Click += (s, e) => UpdateQty(-1);

        pnl.Controls.Add(_lblCount);
        pnl.Controls.Add(btnMinus);
        pnl.Controls.Add(btnPlus);
        return pnl;
    }

    private Panel BuildRightActionsPanel(decimal price)
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Right,
            Width = 100,
            BackColor = Color.Transparent
        };

        var btnDelete = new IconButton
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

        _lblPrice = new Label
        {
            Text = $"{price:N0} đ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
        };

        pnl.Controls.Add(_lblPrice);
        pnl.Controls.Add(btnDelete);
        return pnl;
    }

    private Panel BuildInfoPanel(string name, string note)
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5, 5, 0, 5)
        };

        _lblName = new Label
        {
            Text = name,
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoEllipsis = true
        };

        _lblNote = new Label
        {
            Text = string.IsNullOrEmpty(note) ? "" : $"{note}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.Gray,
            AutoEllipsis = true
        };

        pnl.Controls.Add(_lblNote);
        pnl.Controls.Add(_lblName);
        return pnl;
    }

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

    private void BindDoubleClickRecursive(Control control)
    {
        if (control.Tag?.ToString() == "BLOCK_DOUBLE_CLICK")
        {
            return;
        }

        if (control is not Button && control is not IconButton)
        {
            control.DoubleClick += (s, e) => OnNoteEditRequest?.Invoke(this, Note);
        }

        foreach (Control child in control.Controls)
        {
            BindDoubleClickRecursive(child);
        }
    }

    public void UpdateQty(int delta)
    {
        int oldQty = _quantity;
        _quantity += delta;
        if (_quantity < 1) _quantity = 1;
        if (oldQty == _quantity) return;

        if (_lblCount != null) _lblCount.Text = $"{_quantity}";
        if (_lblPrice != null) _lblPrice.Text = $"{TotalValue:N0}";
        OnAmountChanged?.Invoke(this, delta * _unitPrice);
    }

    public void SetNote(string newNote)
    {
        Note = newNote;
        if (_lblNote != null) _lblNote.Text = string.IsNullOrEmpty(newNote) ? "" : $"{newNote}";
    }
}
