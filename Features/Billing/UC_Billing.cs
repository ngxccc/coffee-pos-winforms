using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Features.Billing;

public class UC_Billing : UserControl
{
    private readonly FlowLayoutPanel _flowBillItemList;
    private readonly Label _lblTotalPrice;
    private decimal _grandTotal = 0;
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

    public void AddItemToBill(string name, int qty, decimal price)
    {
        Bitmap dummyImg = new(100, 100);
        using (Graphics g = Graphics.FromImage(dummyImg))
        {
            g.Clear(Color.Bisque); // Màu kem
                                   // Vẽ chữ cái đầu của tên món vào ảnh
            g.DrawString(name, new Font("Arial", 20), Brushes.Brown, 10, 30);
        }
        UC_BillItem billItem = new(name, qty, price, dummyImg);

        billItem.OnAmountChanged += (sender, moneyDiff) =>
        {
            UpdateTotal(moneyDiff);
        };

        billItem.OnDeleteRequest += (sender, e) =>
        {
            UpdateTotal(-billItem.TotalValue);
            _flowBillItemList.Controls.Remove(billItem);
            billItem.Dispose();
        };

        _flowBillItemList.Controls.Add(billItem);
        UpdateTotal(qty * price);
    }

    private void UpdateTotal(decimal amountToAdd)
    {
        _grandTotal += amountToAdd;
        _lblTotalPrice.Text = $"{_grandTotal:N0} đ";
    }
}
