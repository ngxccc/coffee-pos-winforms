using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public class UC_PaymentConfirm : UserControl, ControlEvent
{
    private readonly InputNumber _numBuzzer;
    public int BuzzerNumber => int.TryParse(_numBuzzer.Text, out int val) ? val : 0;

    public UC_PaymentConfirm(decimal finalAmount)
    {
        Size = new Size(380, 230);
        BackColor = UiTheme.Surface;

        var lblAmountTitle = new AntdUI.Label
        {
            Text = "TỔNG TIỀN CẦN THU:",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(20, 20),
            AutoSize = true
        };

        var lblAmountValue = new AntdUI.Label
        {
            Text = $"{finalAmount:N0} đ",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Location = new Point(20, 45),
            AutoSize = true
        };

        var lblBuzzerTitle = new AntdUI.Label
        {
            Text = "Nhập số Thẻ Rung / Số thứ tự:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            Location = new Point(20, 110),
            AutoSize = true
        };

        _numBuzzer = new InputNumber
        {
            Location = new Point(20, 140),
            Width = 340,
            Height = 50,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Minimum = 0,
            Maximum = 999,
            Value = 1
        };

        Controls.Add(lblAmountTitle);
        Controls.Add(lblAmountValue);
        Controls.Add(lblBuzzerTitle);
        Controls.Add(_numBuzzer);
    }

    public void LoadCompleted()
    {
        _numBuzzer.Focus();
    }
}
