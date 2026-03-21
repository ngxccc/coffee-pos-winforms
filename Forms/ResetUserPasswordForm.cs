namespace CoffeePOS.Forms;

public class ResetUserPasswordForm : Form
{
    private readonly TextBox _txtNewPassword;
    private readonly TextBox _txtConfirmPassword;

    public string NewPassword => _txtNewPassword.Text;
    public string ConfirmPassword => _txtConfirmPassword.Text;

    public ResetUserPasswordForm(string username)
    {
        Text = $"ĐỔI MẬT KHẨU - {username}";
        Size = new Size(420, 240);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        _txtNewPassword = new TextBox
        {
            Location = new Point(20, 45),
            Width = 360,
            Font = new Font("Segoe UI", 11),
            PasswordChar = '●'
        };

        _txtConfirmPassword = new TextBox
        {
            Location = new Point(20, 110),
            Width = 360,
            Font = new Font("Segoe UI", 11),
            PasswordChar = '●'
        };

        var btnSave = new Button
        {
            Text = "LƯU",
            Location = new Point(230, 160),
            Size = new Size(70, 32),
            BackColor = Color.FromArgb(243, 156, 18),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => DialogResult = DialogResult.OK;

        var btnCancel = new Button
        {
            Text = "HỦY",
            Location = new Point(310, 160),
            Size = new Size(70, 32),
            BackColor = Color.Silver,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(
        [
            new Label
            {
                Text = "Mật khẩu mới",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20)
            },
            _txtNewPassword,
            new Label
            {
                Text = "Xác nhận mật khẩu",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 85)
            },
            _txtConfirmPassword,
            btnSave,
            btnCancel
        ]);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }
}
