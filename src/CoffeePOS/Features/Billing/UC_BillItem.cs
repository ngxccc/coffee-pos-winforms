
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillItem : UserControl
{
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

        InitializeComponent();

        _lblName.Text = foodName;
        _lblNote.Text = string.IsNullOrEmpty(note) ? "" : note;
        _lblCount.Text = $"{_quantity}";
        _lblPrice.Text = $"{TotalValue:N0} đ";

        WireEvents();
    }

    public void LoadImage()
    {
        _ = ImageHelper.LoadImageAsync(_picFood, ImageIdentifier, ItemName, ProductId);
    }

    private void WireEvents()
    {
        _btnPlus.Click += (s, e) => UpdateQty(1);
        _btnMinus.Click += (s, e) => UpdateQty(-1);
        _btnDelete.Click += (s, e) => OnDeleteRequest?.Invoke(this, this);

        // _pnlInfo.DoubleClick += TriggerEditMode;
        _lblName.DoubleClick += TriggerEditMode;
        _lblNote.DoubleClick += TriggerEditMode;
        DoubleClick += TriggerEditMode;
    }

    private void TriggerEditMode(object? sender, EventArgs e)
    {
        if (LinkedCartItem != null)
        {
            OnEditItemRequest?.Invoke(this, LinkedCartItem);
        }
        else
        {
            OnNoteEditRequest?.Invoke(this, Note);
        }
    }

    public void UpdateQty(int delta)
    {
        int oldQty = _quantity;
        _quantity += delta;
        if (_quantity < 1) _quantity = 1;

        if (oldQty == _quantity) return;

        _lblCount.Text = $"{_quantity}";
        _lblPrice.Text = $"{TotalValue:N0} đ";

        OnAmountChanged?.Invoke(this, delta * _unitPrice);
    }

    public void SetNote(string newNote)
    {
        Note = newNote;
        _lblNote.Text = string.IsNullOrEmpty(newNote) ? "" : newNote;
    }
}
