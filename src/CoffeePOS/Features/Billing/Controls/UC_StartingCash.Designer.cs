using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing.Controls;

public partial class UC_StartingCash
{
    private AntdUI.InputNumber _numCash = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Size = new Size(350, 110);

        AntdUI.StackPanel root = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Padding = new Padding(10),
        };

        AntdUI.Label lblDesc = new()
        {
            Text = "Vui lòng đếm và nhập số tiền lẻ đang có trong két:",
            AutoSize = true,
            Font = UiTheme.BodyFont
        };

        _numCash = new AntdUI.InputNumber
        {
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Height = 45,
            Minimum = 0,
            Maximum = 1000000000,
            Value = 500000,
            ThousandsSeparator = true,
        };

        root.Controls.Add(_numCash);
        root.Controls.Add(lblDesc);

        Controls.Add(root);
        ResumeLayout(false);
    }
}
