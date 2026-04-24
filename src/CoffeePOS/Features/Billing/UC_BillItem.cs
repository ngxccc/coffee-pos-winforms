using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillItem : UserControl
{
    private decimal _unitPrice;

    public event EventHandler<decimal>? OnAmountChanged;
    public event EventHandler<UC_BillItem>? OnDeleteRequest;
    public event EventHandler<string>? OnNoteEditRequest;
    public event EventHandler<CartItemDto>? OnEditItemRequest;

    public decimal TotalValue => LinkedCartItem?.TotalLinePrice ?? ((int)_numQty.Value * _unitPrice);
    public int Quantity => LinkedCartItem?.Quantity ?? (int)_numQty.Value;
    public int ProductId { get; private set; }
    public string ItemName { get; private set; }
    public string Note { get; private set; }
    public string? ImageIdentifier { get; private set; }
    public CartItemDto? LinkedCartItem { get; set; }

    public UC_BillItem(int id, string foodName, int count, decimal price, string note = "", string? imageIdentifier = null)
    {
        ProductId = id;
        ItemName = foodName;
        _unitPrice = price;
        Note = note;
        ImageIdentifier = imageIdentifier;

        InitializeComponent();

        _numQty.Value = count;
        _lblName.Text = foodName;
        _lblNote.Text = string.IsNullOrEmpty(note) ? "" : note;
        _lblPrice.Text = $"{TotalValue:N0} đ";

        WireEvents();
    }

    public void LoadImage()
    {
        _ = ImageHelper.LoadImageAsync(_picFood, ImageIdentifier, ItemName, ProductId);
    }

    private void WireEvents()
    {
        _numQty.ValueChanged += (s, e) =>
        {
            if (LinkedCartItem == null) return;

            int newQty = (int)_numQty.Value;
            int diffQty = newQty - LinkedCartItem.Quantity;

            if (diffQty == 0) return;

            decimal moneyDiff = diffQty * _unitPrice;
            LinkedCartItem.Quantity = newQty;
            _lblPrice.Text = $"{LinkedCartItem.TotalLinePrice:N0} đ";

            OnAmountChanged?.Invoke(this, moneyDiff);
        };

        _btnDelete.Click += (s, e) => OnDeleteRequest?.Invoke(this, this);

        _lblName.DoubleClick += TriggerEditMode;
        _lblNote.DoubleClick += TriggerEditMode;
        _picFood.DoubleClick += TriggerEditMode;
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

    public void SyncUI()
    {
        if (LinkedCartItem == null) return;

        _lblNote.Text = LinkedCartItem.Note;
        _numQty.Value = LinkedCartItem.Quantity;
        _lblPrice.Text = $"{LinkedCartItem.TotalLinePrice:N0} đ";
    }

    public void UpdatePrice(decimal newPrice)
    {
        _unitPrice = newPrice;
    }
}
