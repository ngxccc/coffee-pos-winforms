using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Features.Billing;

public class UC_Billing : UserControl
{
    private readonly FlowLayoutPanel _flowBillItemList;
    public event EventHandler? OnPayClicked;

    public UC_Billing()
    {
        Width = 420;
        Dock = DockStyle.Right;
        BackColor = Color.White;

        Panel pnlBillingFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            Padding = new Padding(15),
            BackColor = Color.WhiteSmoke,
        };

        MaterialButton btnPay = new()
        {
            Text = "THANH TOÁN",
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand,
        };
        btnPay.Click += (s, e) => OnPayClicked?.Invoke(this, EventArgs.Empty);

        _flowBillItemList = new()
        {
            Dock = DockStyle.Fill,      // Lấp đầy khoảng trống phía trên Footer
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false        // Quan trọng: Không cho item nhảy lung tung
        };

        pnlBillingFooter.Controls.Add(btnPay);

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
        _flowBillItemList.Controls.Add(billItem);
    }
}
