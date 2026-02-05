using CoffeePOS.Data.Repositories;
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
            BackColor = Color.FromArgb(30, 30, 30), // Màu đen xám Deep
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
        Panel pnlBilling = new()
        {
            Width = 400,
            Dock = DockStyle.Right,
            BackColor = Color.White,
        };

        Panel pnlBillingFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(10),
        };

        // Panel pnlShadow = new() { Width = 1, Dock = DockStyle.Left, BackColor = Color.LightGray };

        MaterialButton btnPay = new()
        {
            Text = "THANH TOÁN",
            Dock = DockStyle.Fill,
        };

        // WORKSPACE
        Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245) // Xám trắng nhạt
        };

        // CONTROLS
        pnlSidebar.Controls.Add(btnHome);
        Controls.Add(pnlSidebar);

        // pnlBilling.Controls.Add(pnlShadow);

        pnlBillingFooter.Controls.Add(btnPay);
        pnlBilling.Controls.Add(pnlBillingFooter);

        Controls.Add(pnlBilling);
        Controls.Add(pnlMain);

        pnlMain.BringToFront();
    }
}
