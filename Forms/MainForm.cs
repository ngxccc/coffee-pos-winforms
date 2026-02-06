using CoffeePOS.Data.Repositories;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Features.Tables;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Forms;

public partial class MainForm : Form
{
    private readonly IBillRepository _billRepo;
    private UC_Sidebar? _ucSidebar;
    private UC_Billing? _ucBilling;
    private Panel? _pnlMainWorkspace;

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
        _ucSidebar = new UC_Sidebar();
        _ucSidebar.OnHomeClicked += (s, e) => SwitchToHome();

        // BILLING PANEL
        _ucBilling = new UC_Billing();

        _ucBilling.AddItemToBill(101, "Cafe Đen Đá", 1, 20000, "Ít đường");
        _ucBilling.AddItemToBill(101, "Cafe Đen Đá", 1, 20000, "Ít đường");
        _ucBilling.AddItemToBill(102, "Cafe Đen Đá XL", 1, 30000, "Nhiều đá");

        _ucBilling.OnPayClicked += (s, e) => ProcessPayment();

        // WORKSPACE
        _pnlMainWorkspace = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        // CONTROLS
        Controls.Add(_ucSidebar);

        Controls.Add(_ucBilling);

        LoadTableMap();

        Controls.Add(_pnlMainWorkspace);

        _pnlMainWorkspace.BringToFront();
    }

    private void LoadTableMap()
    {
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

        _pnlMainWorkspace?.Controls.Add(flowTableList);
    }

    private static void ProcessPayment()
    {
        MessageBox.Show("Thanh toán thành công");
    }

    private static void SwitchToHome()
    {
        MessageBox.Show("Đã chuyển sang trang Home");
    }
}
