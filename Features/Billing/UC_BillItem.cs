using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public class UC_BillItem : Panel
{
    // UI COMPONENTS
    private Label _lblCount = null!;
    private Label _lblPrice = null!;
    private Label _lblNote = null!;
    private Label _lblName = null!;
    private PictureBox _picFood = null!;

    // DATA FIELDS
    private int _quantity;
    private readonly decimal _unitPrice;

    // EVENTS
    public event EventHandler<decimal>? OnAmountChanged;
    public event EventHandler<UC_BillItem>? OnDeleteRequest;
    public event EventHandler<string>? OnNoteEditRequest;
    public event EventHandler<CartItemDto>? OnEditItemRequest;

    // PROPERTIES
    public decimal TotalValue => _quantity * _unitPrice;
    public int ProductId { get; private set; }
    public string ItemName { get; private set; }
    public string Note { get; private set; }
    public int Quantity => _quantity;
    public string? ImageIdentifier { get; private set; }
    public CartItemDto? LinkedCartItem { get; set; }

    public UC_BillItem(int id, string foodName, int count, decimal price, string note = "", string? imageIdentifier = null)
    {
        ProductId = id;
        ItemName = foodName;
        _quantity = count;
        _unitPrice = price;
        Note = note;
        ImageIdentifier = imageIdentifier;

        InitializeUI(foodName, note, price);
    }

    private void InitializeUI(string foodName, string note, decimal price)
    {
        Size = new Size(400, 90);
        BackColor = Color.White;
        Padding = new Padding(5);
        Margin = new Padding(0, 0, 0, 10);

        _picFood = BuildImagePanel();
        var pnlQty = BuildQtyPanel();
        var pnlRight = BuildRightActionsPanel(price);
        var pnlInfo = BuildInfoPanel(foodName, note);

        // Add vào: Info (Fill) -> Right (Right) -> Qty (Left) -> Image (Left)
        Controls.Add(pnlInfo);
        Controls.Add(pnlRight);
        Controls.Add(pnlQty);
        Controls.Add(_picFood);

        BindDoubleClickRecursive(this);

        _ = ImageHelper.LoadImageAsync(_picFood, ImageIdentifier, foodName, ProductId);
    }

    private static PictureBox BuildImagePanel()
    {
        return new PictureBox
        {
            SizeMode = PictureBoxSizeMode.CenterImage,
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

        var btnMinus = CreateQtyButton("-");
        var btnPlus = CreateQtyButton("+");

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

        var btnDelete = new AntdUI.Button
        {
            Text = "X",
            Type = AntdUI.TTypeMini.Error,
            Dock = DockStyle.Right,
            Width = 30,
            Radius = 6,
            Cursor = Cursors.Hand
        };
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

    private static AntdUI.Button CreateQtyButton(string text)
    {
        var btn = new AntdUI.Button
        {
            Text = text,
            Type = AntdUI.TTypeMini.Default,
            Width = 25,
            Dock = text == "-" ? DockStyle.Left : DockStyle.Right,
            BackColor = Color.FromArgb(240, 240, 240),
            Cursor = Cursors.Hand
        };
        return btn;
    }

    private void BindDoubleClickRecursive(Control control)
    {
        if (control.Tag?.ToString() == "BLOCK_DOUBLE_CLICK")
        {
            return;
        }

        if (control is not Button && control is not AntdUI.Button)
        {
            control.DoubleClick += (s, e) =>
            {
                // If this item has a linked CartItemDto, trigger edit mode
                if (LinkedCartItem != null)
                {
                    OnEditItemRequest?.Invoke(this, LinkedCartItem);
                }
                else
                {
                    // Fall back to note edit for legacy items
                    OnNoteEditRequest?.Invoke(this, Note);
                }
            };
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Khi BillItem bị xóa khỏi FlowLayoutPanel, phải tự tay đập nát cái ảnh bên trong
            if (_picFood != null && _picFood.Image != null)
            {
                _picFood.Image.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}
