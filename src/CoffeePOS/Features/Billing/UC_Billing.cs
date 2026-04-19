using System.ComponentModel;
using CoffeePOS.Shared.Dtos.Bill;

namespace CoffeePOS.Features.Billing;

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
    }

    public void AddItem(CartItemDto cartItem)
    {
        if (_flowBillItemList == null) return;

        string uniqueKey = BuildItemKey(cartItem);

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.LinkedCartItem!.Quantity += cartItem.Quantity;
            existingItem.SyncUI();

            UpdateTotal(cartItem.TotalLinePrice);

            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateBillItem(cartItem);
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

    public void UpdateItem(CartItemDto existingItem, CartItemDto updatedItem)
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

        string newKey = BuildItemKey(existingItem);
        UpdateTotal(-oldTotal);

        if (_billItemsDict.TryGetValue(newKey, out UC_BillItem? mergeTarget))
        {
            mergeTarget.LinkedCartItem!.Quantity += existingItem.Quantity;
            mergeTarget.SyncUI();
            UpdateTotal(existingItem.TotalLinePrice);

            _cartItems.Remove(existingItem);
            return;
        }

        UC_BillItem newBillItem = CreateBillItem(existingItem);
        _flowBillItemList.Controls.Add(newBillItem);
        _flowBillItemList.Controls.SetChildIndex(newBillItem, oldIndex);
        _billItemsDict.Add(newKey, newBillItem);

        newBillItem.LoadImage();
        UpdateTotal(newBillItem.TotalValue);
    }

    public List<CreateBillItemDto> GetCartItems()
    {
        return [.. _cartItems.Select(item => new CreateBillItemDto(
            item.ProductId,
            item.DisplayName,
            item.Quantity,
            GetUnitPrice(item),
            item.Note
        ))];
    }

    // --- PRIVATE HELPERS ---

    private UC_BillItem CreateBillItem(CartItemDto cartItem)
    {
        decimal unitPrice = GetUnitPrice(cartItem);

        UC_BillItem billItem = new(cartItem.ProductId, cartItem.DisplayName, cartItem.Quantity, unitPrice, cartItem.Note, cartItem.ImageUrl)
        {
            LinkedCartItem = cartItem
        };

        billItem.OnEditItemRequest += (s, item) => OnEditCartItem?.Invoke(this, item);
        billItem.OnDeleteRequest += (s, e) => HandleDeleteItem(billItem);

        billItem.OnAmountChanged += (s, moneyDiff) => UpdateTotal(moneyDiff);

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

    private void UpdateTotal(decimal amountToAdd)
    {
        _grandTotal += amountToAdd;
        if (_lblTotalPrice != null) _lblTotalPrice.Text = $"{_grandTotal:N0} đ";
    }

    private static string BuildItemKey(CartItemDto item)
    {
        string toppingIds = string.Join(",", item.Toppings.Select(t => t.Id).OrderBy(id => id));
        // HACK: Thêm Note vào Key. Note khác nhau = Key khác nhau = Dòng riêng biệt
        return $"{item.ProductId}_{item.SizeName}_{toppingIds}_{item.Note.ToLower()}";
    }

    private static decimal GetUnitPrice(CartItemDto item) => item.BasePrice + item.Toppings.Sum(t => t.Price);

    private string? FindKeyByItem(UC_BillItem item)
    {
        foreach (var pair in _billItemsDict)
        {
            if (ReferenceEquals(pair.Value, item)) return pair.Key;
        }
        return null;
    }
}
