using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class EditUserForm : Form
{
    private int _targetUserId;
    private readonly TextBox _txtUsername;
    private readonly TextBox _txtFullName;
    private readonly ComboBox _cboRole;
    private readonly TextBox _txtNewPassword;
    private readonly TextBox _txtConfirmPassword;

    public EditUserForm()
    {
        Text = "SỬA TÀI KHOẢN";
        Size = new Size(430, 440);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        _txtUsername = CreateTextBox(new Point(20, 45));

        _txtFullName = CreateTextBox(new Point(20, 110));

        _cboRole = new ComboBox
        {
            Location = new Point(20, 175),
            Width = 370,
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

        _txtNewPassword = CreatePasswordBox(new Point(20, 240));

        _txtConfirmPassword = CreatePasswordBox(new Point(20, 305));

        var btnSave = new Button
        {
            Text = "CẬP NHẬT",
            Location = new Point(210, 355),
            Size = new Size(100, 32),
            BackColor = Color.FromArgb(243, 156, 18),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => DialogResult = DialogResult.OK;

        var btnCancel = new Button
        {
            Text = "HỦY",
            Location = new Point(320, 355),
            Size = new Size(70, 32),
            BackColor = Color.Silver,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(
        [
            CreateLabel("Tài khoản", new Point(20, 20)),
            _txtUsername,
            CreateLabel("Họ tên", new Point(20, 85)),
            _txtFullName,
            CreateLabel("Vai trò", new Point(20, 150)),
            _cboRole,
            CreateLabel("Mật khẩu mới (để trống nếu không đổi)", new Point(20, 215)),
            _txtNewPassword,
            CreateLabel("Xác nhận mật khẩu", new Point(20, 280)),
            _txtConfirmPassword,
            btnSave,
            btnCancel
        ]);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    public void LoadUser(UserGridDto user)
    {
        _targetUserId = user.Id;
        _txtUsername.Text = user.Username;
        _txtFullName.Text = user.FullName;
        _cboRole.SelectedValue = user.Role;

        Text = $"SỬA TÀI KHOẢN - {user.Username}";
        _txtNewPassword.Clear();
        _txtConfirmPassword.Clear();
    }

    public UpdateUserAccountDto BuildCommand()
        => new(
            _targetUserId,
            _txtUsername.Text,
            _txtFullName.Text,
            _cboRole.SelectedValue is int roleValue ? roleValue : 1,
            _txtNewPassword.Text,
            _txtConfirmPassword.Text);

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
        Width = 370,
        Font = new Font("Segoe UI", 11)
    };

    private static TextBox CreatePasswordBox(Point location)
        => new()
        {
            Location = location,
            Width = 370,
            Font = new Font("Segoe UI", 11),
            PasswordChar = '●'
        };

    private sealed record RoleOption(int Value, string Name);
}
