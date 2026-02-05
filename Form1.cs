namespace CoffeePOS;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        DashboardForm();
    }

    public void DashboardForm()
    {
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;

        Panel pnlSidebar = new()
        {
            Width = 80,
            Dock = DockStyle.Left,
            BackColor = Color.FromArgb(45, 52, 54) // Dark Theme Sidebar
        };
        Controls.Add(pnlSidebar);

        Panel pnlBill = new()
        {
            Width = 400,
            Dock = DockStyle.Right,
            BackColor = Color.White
        };

        Panel border = new() { Width = 1, Dock = DockStyle.Left, BackColor = Color.LightGray };
        pnlBill.Controls.Add(border);
        Controls.Add(pnlBill);

        Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(244, 247, 252) // Xám xanh nhạt hiện đại
        };
        Controls.Add(pnlMain);
    }
}
