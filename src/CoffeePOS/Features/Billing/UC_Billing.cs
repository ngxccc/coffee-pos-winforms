using System.ComponentModel;
using CoffeePOS.Shared.Dtos;
using Microsoft.VisualBasic;

namespace CoffeePOS.Features.Billing;

// FIX MẠNH: Thêm từ khóa partial
public partial class UC_Billing : UserControl
{
    // DATA STATE
    private decimal _grandTotal = 0;
    public decimal GrandTotal => _grandTotal;
    private readonly Dictionary<string, UC_BillItem> _billItemsDict = [];
    private readonly BindingList<CartItemDto> _cartItems = [];
    public bool HasUnpaidItems => _billItemsDict.Count > 0 || _cartItems.Count > 0;

    // EVENTS
    public event EventHandler? OnPayClicked;
    public event EventHandler<CartItemDto>? OnEditCartItem;

    public UC_Billing()
    {
        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
        _btnPay.Click += (s, e) => OnPayClicked?.Invoke(this, EventArgs.Empty);
    }

    // --- PUBLIC METHODS ---

    public void ClearOrder()
    {
        if (_flowBillItemList == null) return;

        foreach (Control c in _flowBillItemList.Controls) c.Dispose();

        _flowBillItemList.Controls.Clear();
        _billItemsDict.Clear();
        _cartItems.Clear();
        _grandTotal = 0;

        if (_lblTotalPrice != null) _lblTotalPrice.Text = "0 đ";
        if (_lblOrderTitle != null) _lblOrderTitle.Text = "Vui lòng chọn bàn";
    }

    public void AddItemToBill(int productId, string name, int qty, decimal price, string? imageIdentifier = null)
    {
        if (_flowBillItemList == null) throw new InvalidOperationException("_flowBillItemList is null!");

        string uniqueKey = BuildLegacyKey(productId, string.Empty);

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(qty);
            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateBillItem(productId, name, qty, price, imageIdentifier);
        if (_flowBillItemList.ClientSize.Width > 0)
        {
            billItem.Width = _flowBillItemList.ClientSize.Width - 8;
            billItem.Margin = new Padding(0, 0, 0, 5);
        }

        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);

        billItem.LoadImage();
        UpdateTotal(qty * price);
    }

    public void AddCustomizedItemToBill(CartItemDto cartItem)
    {
        if (_flowBillItemList == null) return;

        string uniqueKey = BuildCustomizedKey(cartItem);

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(cartItem.Quantity);
            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateCustomizedBillItem(cartItem);
        if (_flowBillItemList.ClientSize.Width > 0)
        {
            billItem.Width = _flowBillItemList.ClientSize.Width - 8;
            billItem.Margin = new Padding(0, 0, 0, 5);
        }

        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);
        _cartItems.Add(cartItem);

        billItem.LoadImage();
        UpdateTotal(cartItem.TotalLinePrice);
    }

    public void UpdateCustomizedItem(CartItemDto existingItem, CartItemDto updatedItem)
    {
        if (_flowBillItemList == null) return;

        var oldBillItem = _billItemsDict.Values.FirstOrDefault(x => ReferenceEquals(x.LinkedCartItem, existingItem));
        if (oldBillItem == null) return;

        string? oldKey = FindKeyByItem(oldBillItem);
        decimal oldTotal = oldBillItem.TotalValue;
        int oldIndex = _flowBillItemList.Controls.GetChildIndex(oldBillItem);

        if (oldKey != null) _billItemsDict.Remove(oldKey);

        _flowBillItemList.Controls.Remove(oldBillItem);
        oldBillItem.Dispose();

        existingItem.ProductId = updatedItem.ProductId;
        existingItem.ProductName = updatedItem.ProductName;
        existingItem.SizeName = updatedItem.SizeName;
        existingItem.BasePrice = updatedItem.BasePrice;
        existingItem.Quantity = updatedItem.Quantity;
        existingItem.Toppings = [.. updatedItem.Toppings];

        string newKey = BuildCustomizedKey(existingItem);
        UpdateTotal(-oldTotal);

        if (_billItemsDict.TryGetValue(newKey, out UC_BillItem? mergeTarget))
        {
            mergeTarget.UpdateQty(existingItem.Quantity);
            _cartItems.Remove(existingItem);
            return;
        }

        UC_BillItem newBillItem = CreateCustomizedBillItem(existingItem);
        _flowBillItemList.Controls.Add(newBillItem);
        _flowBillItemList.Controls.SetChildIndex(newBillItem, oldIndex);
        _billItemsDict.Add(newKey, newBillItem);

        newBillItem.LoadImage();
        UpdateTotal(newBillItem.TotalValue);
    }

    private async Task HandleEditCartItemAsync(CartItemDto cartItem)
    {
        OnEditCartItem?.Invoke(this, cartItem);
        await Task.CompletedTask;
    }

    private UC_BillItem CreateBillItem(int productId, string name, int qty, decimal price, string? imageIdentifier)
    {
        UC_BillItem billItem = new(productId, name, qty, price, imageIdentifier: imageIdentifier);

        billItem.OnNoteEditRequest += BillItem_OnNoteEditRequest;
        billItem.OnAmountChanged += (s, moneyDiff) => UpdateTotal(moneyDiff);
        billItem.OnDeleteRequest += (s, e) => HandleDeleteItem(billItem);

        return billItem;
    }

    private UC_BillItem CreateCustomizedBillItem(CartItemDto cartItem)
    {
        decimal unitPrice = GetCustomizedUnitPrice(cartItem);

        UC_BillItem billItem = CreateBillItem(cartItem.ProductId, cartItem.DisplayName, cartItem.Quantity, unitPrice, cartItem.ImageUrl);
        billItem.LinkedCartItem = cartItem;

        billItem.OnEditItemRequest += async (s, item) => await HandleEditCartItemAsync(item);
        billItem.OnAmountChanged += (s, moneyDiff) =>
        {
            int quantityDelta = GetQuantityDelta(moneyDiff, unitPrice);
            if (quantityDelta != 0) cartItem.Quantity += quantityDelta;
        };

        return billItem;
    }

    private void HandleDeleteItem(UC_BillItem item)
    {
        if (_flowBillItemList == null) return;

        UpdateTotal(-item.TotalValue);
        _flowBillItemList.Controls.Remove(item);

        string? keyToDelete = FindKeyByItem(item);
        if (keyToDelete != null) _billItemsDict.Remove(keyToDelete);

        if (item.LinkedCartItem != null) _cartItems.Remove(item.LinkedCartItem);

        item.Dispose();
    }

    private void BillItem_OnNoteEditRequest(object? sender, string currentNote)
    {
        if (sender is not UC_BillItem currentItem) return;

        // HACK: VisualBasic InputBox. Replace with native UI later.
        string newNote = Interaction.InputBox("Nhập ghi chú mới:", "Sửa Ghi Chú", currentNote);
        if (newNote == currentNote) return;

        string oldKey = $"{currentItem.ProductId}_{currentItem.Note}";
        string newKey = $"{currentItem.ProductId}_{newNote}";

        if (_billItemsDict.TryGetValue(newKey, out UC_BillItem? targetItem))
        {
            targetItem.UpdateQty(currentItem.Quantity);
            HandleDeleteItem(currentItem);
            _flowBillItemList?.ScrollControlIntoView(targetItem);
        }
        else
        {
            _billItemsDict.Remove(oldKey);
            currentItem.SetNote(newNote);
            _billItemsDict.Add(newKey, currentItem);
        }
    }

    private void UpdateTotal(decimal amountToAdd)
    {
        _grandTotal += amountToAdd;
        if (_lblTotalPrice != null) _lblTotalPrice.Text = $"{_grandTotal:N0} đ";
    }

    public List<CreateBillItemDto> GetCartItems()
    {
        var list = new List<CreateBillItemDto>();
        foreach (var item in _billItemsDict.Values)
        {
            if (item.LinkedCartItem != null)
            {
                var linked = item.LinkedCartItem;
                decimal unitPrice = GetCustomizedUnitPrice(linked);

                list.Add(new CreateBillItemDto(
                    linked.ProductId,
                    linked.DisplayName,
                    linked.Quantity,
                    unitPrice,
                    string.Empty));
                continue;
            }

            list.Add(new CreateBillItemDto(
                item.ProductId,
                item.ItemName,
                item.Quantity,
                item.TotalValue / item.Quantity,
                item.Note));
        }
        return list;
    }

    private static string BuildLegacyKey(int productId, string note) => $"{productId}_{note}";

    private static string BuildCustomizedKey(CartItemDto item)
    {
        string toppingIds = string.Join(",", item.Toppings.Select(t => t.Id).OrderBy(id => id));
        return $"{item.ProductId}_{item.SizeName}_{toppingIds}";
    }

    private static decimal GetCustomizedUnitPrice(CartItemDto item) => item.BasePrice + item.Toppings.Sum(t => t.Price);

    private static int GetQuantityDelta(decimal moneyDiff, decimal unitPrice)
    {
        if (unitPrice <= 0) return 0;
        return (int)Math.Round(moneyDiff / unitPrice, MidpointRounding.AwayFromZero);
    }

    private string? FindKeyByItem(UC_BillItem item)
    {
        foreach (var pair in _billItemsDict)
        {
            if (ReferenceEquals(pair.Value, item)) return pair.Key;
        }
        return null;
    }
}
