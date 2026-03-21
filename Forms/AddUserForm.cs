using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class AddUserForm : Form
{
    private readonly TextBox _txtUsername;
    private readonly TextBox _txtFullName;
    private readonly ComboBox _cboRole;
    private readonly TextBox _txtPassword;
    private readonly TextBox _txtConfirm;

    public AddUserForm()
    {
        Text = "THÊM NHÂN VIÊN";
        Size = new Size(420, 390);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        _txtUsername = CreateTextBox(new Point(20, 45));
        _txtFullName = CreateTextBox(new Point(20, 100));

        _cboRole = new ComboBox
        {
            Location = new Point(20, 155),
            Width = 360,
            Font = new Font("Segoe UI", 11),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = new List<RoleOption>
            {
                new(1, "Thu ngân"),
                new(0, "Admin")
            },
            DisplayMember = nameof(RoleOption.Name),
            ValueMember = nameof(RoleOption.Value)
        };

        _txtPassword = CreatePasswordBox(new Point(20, 210));
        _txtConfirm = CreatePasswordBox(new Point(20, 265));

        var btnSave = new Button
        {
            Text = "LƯU",
            Location = new Point(230, 300),
            Size = new Size(70, 32),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => DialogResult = DialogResult.OK;

        var btnCancel = new Button
        {
            Text = "HỦY",
            Location = new Point(310, 300),
            Size = new Size(70, 32),
            BackColor = Color.Silver,
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(
        [
            CreateLabel("Tài khoản", new Point(20, 20)),
            _txtUsername,
            CreateLabel("Họ tên", new Point(20, 75)),
            _txtFullName,
            CreateLabel("Vai trò", new Point(20, 130)),
            _cboRole,
            CreateLabel("Mật khẩu", new Point(20, 185)),
            _txtPassword,
            CreateLabel("Xác nhận mật khẩu", new Point(20, 240)),
            _txtConfirm,
            btnSave,
            btnCancel
        ]);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    public CreateUserDto BuildCommand()
        => new(
            _txtUsername.Text,
            _txtFullName.Text,
            _cboRole.SelectedValue is int roleValue ? roleValue : 1,
            _txtPassword.Text,
            _txtConfirm.Text);

    private static Label CreateLabel(string text, Point location)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = location
        };

    private static TextBox CreateTextBox(Point location)
        => new()
        {
            Location = location,
            Width = 360,
            Font = new Font("Segoe UI", 11)
        };

    private static TextBox CreatePasswordBox(Point location)
        => new()
        {
            Location = location,
            Width = 360,
            Font = new Font("Segoe UI", 11),
            PasswordChar = '●'
        };

    private sealed record RoleOption(int Value, string Name);
}
