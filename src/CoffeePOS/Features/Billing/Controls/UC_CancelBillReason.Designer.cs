using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing.Controls;

partial class UC_CancelBillReason
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.Input _txtReason = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Size = new Size(400, 160);

        AntdUI.Label lblReason = new()
        {
            Text = "Lý do hủy đơn (Bắt buộc):",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtReason = new()
        {
            Font = UiTheme.BodyFont,
            PlaceholderText = "VD: Khách đổi ý, Nhập sai món...",
            AllowClear = true,
            Multiline = true,
            Height = 100,
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
        };

        mainLayout.Controls.Add(_txtReason);
        mainLayout.Controls.Add(lblReason);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
