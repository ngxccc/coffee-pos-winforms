using CoffeePOS.Data.Repositories;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Tables;
using FontAwesome.Sharp;
using ReaLTaiizor.Controls;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Forms;

public partial class MainForm : Form
{
    private readonly IBillRepository _billRepo;

    private void InitializeComponent()
    {
        Text = "CoffeePOS - Code Chay Edition";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
    }

    public MainForm(IBillRepository billRepo)
    {
        InitializeComponent();
        _billRepo = billRepo;
        InitLayout();
    }

    private new void InitLayout()
    {
        // SIDE PANEL
        Panel pnlSidebar = new()
        {
            Width = 80,
            Dock = DockStyle.Left,
            BackColor = Color.FromArgb(30, 30, 30),
        };

        IconButton btnHome = new()
        {
            IconChar = IconChar.Home,
            IconColor = Color.White,
            IconSize = 32,
            Dock = DockStyle.Top,
            Height = 80,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Transparent,
            TextImageRelation = TextImageRelation.Overlay // Chỉ hiện Icon
        };
        btnHome.FlatAppearance.BorderSize = 0;

        // BILLING PANEL
        Panel pnlRightContainer = new()
        {
            Width = 410,
            Dock = DockStyle.Right,
            BackColor = Color.White,
        };

        Panel pnlShadow = new()
        {
            Width = 1,
            Dock = DockStyle.Left,
            BackColor = Color.LightGray
        };
        pnlRightContainer.Controls.Add(pnlShadow);

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

        // WORKSPACE
        Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        FlowLayoutPanel flowTableList = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
        };

        for (int i = 1; i <= 50; i++)
        {
            // Random trạng thái cho vui mắt
            var status = (i % 3 == 0) ? TableStatus.Occupied : TableStatus.Empty;

            UCTable table = new(i, $"Bàn {i:00}", status);

            table.Click += (s, e) =>
            {
                MessageBox.Show($"Bạn vừa chọn Bàn {table.TableId}!");
            };

            flowTableList.Controls.Add(table);
        }

        FlowLayoutPanel flowBillItemList = new()
        {
            Dock = DockStyle.Fill,      // Lấp đầy khoảng trống phía trên Footer
            AutoScroll = true,
            Padding = new Padding(10),  // Padding để nội dung không dính mép trái/phải
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false        // Quan trọng: Không cho item nhảy lung tung
        };

        for (int i = 1; i <= 20; i++)
        {
            Bitmap dummyImg = new(100, 100);
            using (Graphics g = Graphics.FromImage(dummyImg))
            {
                g.Clear(Color.Bisque); // Màu kem
                // Vẽ chữ cái đầu của tên món vào ảnh
                g.DrawString($"CF{i}", new Font("Arial", 20), Brushes.Brown, 10, 30);
            }
            UCBillItem billItem = new($"Cafe Rang Xay Thượng Hạng {i}", 1, 25000 + (i * 1000), dummyImg);
            flowBillItemList.Controls.Add(billItem);
        }

        // CONTROLS
        pnlSidebar.Controls.Add(btnHome);
        Controls.Add(pnlSidebar);

        pnlBillingFooter.Controls.Add(btnPay);
        pnlRightContainer.Controls.Add(pnlBillingFooter);
        pnlRightContainer.Controls.Add(flowBillItemList);
        flowBillItemList.BringToFront();

        Controls.Add(pnlRightContainer);

        pnlMain.Controls.Add(flowTableList);
        Controls.Add(pnlMain);

        pnlMain.BringToFront();
    }
}
