using Microsoft.VisualBasic;
using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Features.Billing;

public class UC_Billing : UserControl
{
    // UI COMPONENTS
    private FlowLayoutPanel? _flowBillItemList;
    private Label? _lblTotalPrice;
    private Label? _lblTableName;

    // DATA
    private decimal _grandTotal = 0;
    private readonly Dictionary<string, UC_BillItem> _billItemsDict = [];
    public int CurrentTableId { get; private set; } = 0;

    // EVENTS
    public event EventHandler? OnPayClicked;

    public UC_Billing()
    {
        InitializeLayout();
        InitializeComponents();
    }

    // UI CONSTRUCTION METHODS

    private void InitializeLayout()
    {
        Width = 420;
        Dock = DockStyle.Right;
        BackColor = Color.White;
    }

    private void InitializeComponents()
    {
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

        _lblTableName = new Label
        {
            Text = "Vui lòng chọn bàn",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        pnl.Controls.Add(_lblTableName);
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

    public void SetTableInfo(int tableId, string tableName)
    {
        ClearOrder();
        CurrentTableId = tableId;
        if (_lblTableName != null)
            _lblTableName.Text = tableName;
    }

    public void ClearOrder()
    {
        if (_flowBillItemList == null)
            return;

        // Dọn dẹp memory cho các control cũ
        foreach (Control c in _flowBillItemList.Controls) c.Dispose();

        _flowBillItemList.Controls.Clear();
        _billItemsDict.Clear();
        _grandTotal = 0;
        if (_lblTotalPrice != null)
            _lblTotalPrice.Text = "0 đ";
        CurrentTableId = 0;
        if (_lblTableName != null)
            _lblTableName.Text = "Vui lòng chọn bàn";
    }

    public void AddItemToBill(int productId, string name, int qty, decimal price, string note = "")
    {
        if (_flowBillItemList == null)
            return;

        string uniqueKey = $"{productId}_{note}";

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(qty);
            _flowBillItemList.ScrollControlIntoView(existingItem);
            return;
        }

        UC_BillItem billItem = CreateBillItem(productId, name, qty, price, note);

        // Add to UI & Data
        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);

        UpdateTotal(qty * price);
    }

    // HELPER LOGIC

    private UC_BillItem CreateBillItem(int productId, string name, int qty, decimal price, string note)
    {
        // Mock image (Sau này thay bằng logic lấy ảnh thật)
        Bitmap dummyImg = new(100, 100);
        using (Graphics g = Graphics.FromImage(dummyImg))
        {
            g.Clear(Color.Bisque);
            g.DrawString(name[..1], new Font("Arial", 20), Brushes.Brown, 10, 30);
        }

        UC_BillItem billItem = new(productId, name, qty, price, note, dummyImg);

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

        string keyToDelete = $"{item.ProductId}_{item.Note}";
        _billItemsDict.Remove(keyToDelete);

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
}
