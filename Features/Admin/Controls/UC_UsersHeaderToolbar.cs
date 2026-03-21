using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_UsersHeaderToolbar : UserControl
{
    private readonly TextBox _txtSearch;

    public event EventHandler? AddClicked;
    public event EventHandler? ResetPasswordClicked;
    public event EventHandler? ToggleStatusClicked;
    public event Action? SearchChanged;

    public string SearchText => _txtSearch.Text;

    public UC_UsersHeaderToolbar()
    {
        Dock = DockStyle.Top;
        Height = 80;
        Padding = new Padding(0, 10, 0, 10);
        BackColor = Color.White;

        Label lblTitle = new()
        {
            Text = "QUẢN LÝ NHÂN VIÊN",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Location = new Point(0, 20)
        };

        _txtSearch = new TextBox
        {
            Width = 320,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = "Nhập tài khoản hoặc họ tên để tìm..."
        };
        _txtSearch.OnDebouncedTextChanged(300, () => SearchChanged?.Invoke());

        FlowLayoutPanel flpButtons = new()
        {
            Dock = DockStyle.Right,
            Width = 430,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var btnDelete = UIHelper.CreateActionButton("Khóa/Mở TK", IconChar.Lock, Color.FromArgb(231, 76, 60), (_, _) => ToggleStatusClicked?.Invoke(this, EventArgs.Empty));
        var btnEdit = UIHelper.CreateActionButton("Đổi Mật Khẩu", IconChar.Key, Color.FromArgb(243, 156, 18), (_, _) => ResetPasswordClicked?.Invoke(this, EventArgs.Empty));
        var btnAdd = UIHelper.CreateActionButton("Thêm NV", IconChar.UserPlus, Color.FromArgb(46, 204, 113), (_, _) => AddClicked?.Invoke(this, EventArgs.Empty));

        flpButtons.Controls.AddRange([btnDelete, btnEdit, btnAdd]);

        Controls.Add(lblTitle);
        Controls.Add(_txtSearch);
        Controls.Add(flpButtons);
    }
}
