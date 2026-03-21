using FontAwesome.Sharp;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_UsersHeaderToolbar : BaseAdminHeaderToolbar
{
    public event EventHandler? ResetPasswordClicked;
    public event EventHandler? ToggleStatusClicked;

    protected override string Title => "QUẢN LÝ NHÂN VIÊN";
    protected override string SearchPlaceholder => "Nhập tài khoản hoặc họ tên để tìm...";
    protected override bool ShowTrashMode => false;

    protected override (string addLabel, IconChar addIcon, string editLabel, IconChar editIcon, string deleteLabel, IconChar deleteIcon) GetButtonConfig() =>
        ("Thêm NV", IconChar.UserPlus, "Sửa TK", IconChar.UserEdit, "Khóa/Mở TK", IconChar.Lock);

    public UC_UsersHeaderToolbar()
    {
        // Wire custom events from base class button events
        EditClicked += (s, e) => ResetPasswordClicked?.Invoke(s, e);
        DeleteClicked += (s, e) => ToggleStatusClicked?.Invoke(s, e);
    }
}
