using Microsoft.VisualBasic;
using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Features.Billing;

public class UC_Billing : UserControl
{
    private readonly FlowLayoutPanel _flowBillItemList;
    private readonly Label _lblTotalPrice;
    private decimal _grandTotal = 0;
    private readonly Dictionary<string, UC_BillItem> _billItemsDict = [];
    public event EventHandler? OnPayClicked;

    public UC_Billing()
    {
        Width = 420;
        Dock = DockStyle.Right;
        BackColor = Color.White;

        Panel pnlBillingFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 110,
            Padding = new Padding(15, 5, 15, 15),
            BackColor = Color.WhiteSmoke,
        };

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

        _lblTotalPrice = new()
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

        pnlBillingFooter.Controls.Add(btnPay);
        pnlBillingFooter.Controls.Add(pnlTotalInfo);

        _flowBillItemList = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        Controls.Add(pnlBillingFooter);
        Controls.Add(_flowBillItemList);

        _flowBillItemList.BringToFront();
    }

    public void AddItemToBill(int productId, string name, int qty, decimal price, string note = "")
    {
        string uniqueKey = $"{productId}_{note}";

        if (_billItemsDict.TryGetValue(uniqueKey, out UC_BillItem? existingItem))
        {
            existingItem.UpdateQty(qty);
            _flowBillItemList.ScrollControlIntoView(existingItem);

            return;
        }

        Bitmap dummyImg = new(100, 100);
        using (Graphics g = Graphics.FromImage(dummyImg))
        {
            g.Clear(Color.Bisque); // Màu kem
                                   // Vẽ chữ cái đầu của tên món vào ảnh
            g.DrawString(name, new Font("Arial", 20), Brushes.Brown, 10, 30);
        }

        UC_BillItem billItem = new(productId, name, qty, price, note, dummyImg);
        billItem.OnNoteEditRequest += BillItem_OnNoteEditRequest;

        billItem.OnAmountChanged += (sender, moneyDiff) =>
        {
            UpdateTotal(moneyDiff);
        };

        billItem.OnDeleteRequest += (sender, e) =>
        {
            UpdateTotal(-billItem.TotalValue);
            _flowBillItemList.Controls.Remove(billItem);

            string keyToDelete = $"{billItem.ProductId}_{billItem.Note}";
            _billItemsDict.Remove(keyToDelete);

            billItem.Dispose();
        };

        _flowBillItemList.Controls.Add(billItem);
        _billItemsDict.Add(uniqueKey, billItem);
        UpdateTotal(qty * price);
    }

    private void UpdateTotal(decimal amountToAdd)
    {
        _grandTotal += amountToAdd;
        _lblTotalPrice.Text = $"{_grandTotal:N0} đ";
    }

    private void BillItem_OnNoteEditRequest(object? sender, string currentNote)
    {
        if (sender is not UC_BillItem currentItem) return;

        // 1. Hiện InputBox hỏi Note mới (Dùng tạm VB InputBox cho lẹ)
        string newNote = Interaction.InputBox("Nhập ghi chú mới:", "Sửa Ghi Chú", currentNote);

        // Nếu user bấm Cancel hoặc không đổi gì
        if (newNote == currentNote) return;

        // 2. Tính toán Key
        string oldKey = $"{currentItem.ProductId}_{currentItem.Note}";
        string newKey = $"{currentItem.ProductId}_{newNote}";

        // 3. KIỂM TRA LOGIC
        if (_billItemsDict.TryGetValue(newKey, out UC_BillItem? targetItem))
        {

            // A. Cộng dồn số lượng từ món cũ sang món đích
            targetItem.UpdateQty(currentItem.Quantity);

            // B. Xóa món cũ đi
            // (Phải trừ tiền của món cũ ra khỏi tổng trước khi xóa)
            // Lưu ý: Hàm UpdateQty ở trên đã TỰ CỘNG thêm tiền vào tổng rồi.
            // Nhưng món cũ vẫn đang nằm đó chiếm tiền -> Phải trừ tiền món cũ đi.
            UpdateTotal(-currentItem.TotalValue);

            // Xóa khỏi UI và Dict
            _flowBillItemList.Controls.Remove(currentItem);
            _billItemsDict.Remove(oldKey);
            currentItem.Dispose();

            // C. Scroll tới món đích cho user thấy
            _flowBillItemList.ScrollControlIntoView(targetItem);
        }
        else
        {
            // --- TRƯỜNG HỢP RENAME (ĐỔI TÊN) ---
            // Chưa có món nào trùng Note mới -> Chỉ cần cập nhật Key và UI

            // A. Xóa Key cũ
            _billItemsDict.Remove(oldKey);

            // B. Update Item
            currentItem.SetNote(newNote); // Update UI + Property

            // C. Add Key mới trỏ về Item hiện tại
            _billItemsDict.Add(newKey, currentItem);
        }
    }
}
