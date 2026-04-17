using System.ComponentModel;
using CoffeePOS.Shared.Dtos;
using Microsoft.VisualBasic;
using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Features.Billing;

public class UC_Billing : UserControl
{
    // UI COMPONENTS
    private FlowLayoutPanel _flowBillItemList = null!;
    private Label _lblTotalPrice = null!;
    private Label _lblOrderTitle = null!;

    // DATA
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
        InitializeUI();
    }

    // UI CONSTRUCTION METHODS

    private void InitializeUI()
    {
        Width = 420;
        Dock = DockStyle.Right;
        BackColor = Color.White;

        var pnlHeader = BuildHeaderPanel();

        var pnlFooter = BuildFooterPanel();

        _flowBillItemList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(5)
        };

        Controls.Add(pnlHeader);         // Top
        Controls.Add(pnlFooter);         // Bottom
        Controls.Add(_flowBillItemList); // Fill

        pnlHeader.SendToBack();       // Đẩy lên cùng (Dock Top ưu tiên)
        _flowBillItemList.BringToFront();
    }

    private Panel BuildHeaderPanel()
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(0, 122, 204),
            Padding = new Padding(15, 0, 0, 0)
        };

        _lblOrderTitle = new Label
        {
            Text = "Khu vực order",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        pnl.Controls.Add(_lblOrderTitle);
        return pnl;
    }

    private Panel BuildFooterPanel()
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Bottom,
            Height = 110,
            Padding = new Padding(15, 5, 15, 15),
            BackColor = Color.WhiteSmoke,
        };

        // Panel con chứa thông tin tổng tiền
        Panel pnlTotalInfo = new()
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.Transparent,
        };

        Label lblTitle = new()
        {
            Text = "Tổng cộng:",
            Dock = DockStyle.Left,
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 100
        };

        _lblTotalPrice = new Label
        {
            Text = "0 đ",
            Dock = DockStyle.Right,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60),
            TextAlign = ContentAlignment.MiddleRight,
            Width = 200
        };

        pnlTotalInfo.Controls.Add(_lblTotalPrice);
        pnlTotalInfo.Controls.Add(lblTitle);

        MaterialButton btnPay = new()
        {
            Text = "THANH TOÁN",
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand,
        };
        btnPay.Click += (s, e) => OnPayClicked?.Invoke(this, EventArgs.Empty);

        pnl.Controls.Add(btnPay);     // Fill
        pnl.Controls.Add(pnlTotalInfo); // Top
        return pnl;
    }

    // PUBLIC METHODS

    public void ClearOrder()
    {
        if (_flowBillItemList == null)
            return;

        // Dọn dẹp memory cho các control cũ
        foreach (Control c in _flowBillItemList.Controls) c.Dispose();

        _flowBillItemList.Controls.Clear();
        _billItemsDict.Clear();
        _cartItems.Clear();
        _grandTotal = 0;
        if (_lblTotalPrice != null)
            _lblTotalPrice.Text = "0 đ";
        if (_lblOrderTitle != null)
            _lblOrderTitle.Text = "Vui lòng chọn bàn";
    }

    public void AddItemToBill(int productId, string name, int qty, decimal price, string? imageIdentifier = null)
    {
        if (_flowBillItemList == null)
            return;

        string uniqueKey = BuildLegacyKey(productId, string.Empty);

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(qty);
            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateBillItem(productId, name, qty, price, imageIdentifier);

        // Add to UI & Data
        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);

        UpdateTotal(qty * price);
    }

    // PUBLIC METHODS - New (CartItemDto-based)

    public void AddCustomizedItemToBill(CartItemDto cartItem)
    {
        if (_flowBillItemList == null)
            return;

        string uniqueKey = BuildCustomizedKey(cartItem);

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(cartItem.Quantity);
            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateCustomizedBillItem(cartItem);

        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);

        // Also add to CartItemDto collection for later retrieval
        _cartItems.Add(cartItem);

        UpdateTotal(cartItem.TotalLinePrice);
    }

    public void UpdateCustomizedItem(CartItemDto existingItem, CartItemDto updatedItem)
    {
        if (_flowBillItemList == null)
            return;

        var oldBillItem = _billItemsDict.Values.FirstOrDefault(x => ReferenceEquals(x.LinkedCartItem, existingItem));
        if (oldBillItem == null)
            return;

        string? oldKey = FindKeyByItem(oldBillItem);
        decimal oldTotal = oldBillItem.TotalValue;
        int oldIndex = _flowBillItemList.Controls.GetChildIndex(oldBillItem);

        if (oldKey != null)
            _billItemsDict.Remove(oldKey);

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

        UpdateTotal(newBillItem.TotalValue);
    }

    private async Task HandleEditCartItemAsync(CartItemDto cartItem)
    {
        // This will be implemented in the form integration
        OnEditCartItem?.Invoke(this, cartItem);
    }

    // HELPER LOGIC

    private UC_BillItem CreateBillItem(int productId, string name, int qty, decimal price, string? imageIdentifier)
    {
        UC_BillItem billItem = new(productId, name, qty, price, imageIdentifier: imageIdentifier);

        // Wiring Events
        billItem.OnNoteEditRequest += BillItem_OnNoteEditRequest;
        billItem.OnAmountChanged += (s, moneyDiff) => UpdateTotal(moneyDiff);
        billItem.OnDeleteRequest += (s, e) => HandleDeleteItem(billItem);

        return billItem;
    }

    private void HandleDeleteItem(UC_BillItem item)
    {
        if (_flowBillItemList == null)
            return;

        UpdateTotal(-item.TotalValue);
        _flowBillItemList.Controls.Remove(item);

        string? keyToDelete = FindKeyByItem(item);
        if (keyToDelete != null)
            _billItemsDict.Remove(keyToDelete);

        if (item.LinkedCartItem != null)
            _cartItems.Remove(item.LinkedCartItem);

        item.Dispose();
    }

    private void BillItem_OnNoteEditRequest(object? sender, string currentNote)
    {
        if (sender is not UC_BillItem currentItem) return;

        string newNote = Interaction.InputBox("Nhập ghi chú mới:", "Sửa Ghi Chú", currentNote);
        if (newNote == currentNote) return;

        // Tính toán Key
        string oldKey = $"{currentItem.ProductId}_{currentItem.Note}";
        string newKey = $"{currentItem.ProductId}_{newNote}";

        // Logic Merge hoặc Rename
        if (_billItemsDict.TryGetValue(newKey, out UC_BillItem? targetItem))
        {
            // MERGE
            targetItem.UpdateQty(currentItem.Quantity);
            HandleDeleteItem(currentItem); // Xóa item cũ đi
            _flowBillItemList?.ScrollControlIntoView(targetItem);
        }
        else
        {
            // RENAME
            _billItemsDict.Remove(oldKey);
            currentItem.SetNote(newNote);
            _billItemsDict.Add(newKey, currentItem);
        }
    }

    private void UpdateTotal(decimal amountToAdd)
    {
        _grandTotal += amountToAdd;
        if (_lblTotalPrice != null)
            _lblTotalPrice.Text = $"{_grandTotal:N0} đ";
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

    private static string BuildLegacyKey(int productId, string note)
    {
        return $"{productId}_{note}";
    }

    private static string BuildCustomizedKey(CartItemDto item)
    {
        string toppingIds = string.Join(",", item.Toppings.Select(t => t.ToppingId).OrderBy(id => id));
        return $"{item.ProductId}_{item.SizeName}_{toppingIds}";
    }

    private static decimal GetCustomizedUnitPrice(CartItemDto item)
    {
        return item.BasePrice + item.Toppings.Sum(t => t.Price);
    }

    private static int GetQuantityDelta(decimal moneyDiff, decimal unitPrice)
    {
        if (unitPrice <= 0)
            return 0;

        return (int)Math.Round(moneyDiff / unitPrice, MidpointRounding.AwayFromZero);
    }

    private string? FindKeyByItem(UC_BillItem item)
    {
        foreach (var pair in _billItemsDict)
        {
            if (ReferenceEquals(pair.Value, item))
                return pair.Key;
        }

        return null;
    }

    private UC_BillItem CreateCustomizedBillItem(CartItemDto cartItem)
    {
        decimal unitPrice = GetCustomizedUnitPrice(cartItem);

        UC_BillItem billItem = CreateBillItem(
            cartItem.ProductId,
            cartItem.DisplayName,
            cartItem.Quantity,
            unitPrice,
            string.Empty);

        billItem.LinkedCartItem = cartItem;
        billItem.OnEditItemRequest += async (s, item) => await HandleEditCartItemAsync(item);
        billItem.OnAmountChanged += (s, moneyDiff) =>
        {
            int quantityDelta = GetQuantityDelta(moneyDiff, unitPrice);
            if (quantityDelta != 0)
                cartItem.Quantity += quantityDelta;
        };

        return billItem;
    }
}
