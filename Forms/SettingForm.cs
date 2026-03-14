using CoffeePOS.Core;

namespace CoffeePOS.Forms;

public partial class SettingForm : Form
{
    public SettingForm(IUserSession session)
    {
        Text = "Cài đặt hệ thống";
        Size = new Size(400, 300);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        Label lblInfo = new()
        {
            Text = $"Xin chào, {session.CurrentUser?.FullName}!\n\nChức năng Cài đặt đang được phát triển...",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.DimGray
        };

        Controls.Add(lblInfo);
    }
}
