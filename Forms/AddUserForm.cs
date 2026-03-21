using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class AddUserForm() : BaseUserAccountForm(
            "THÊM NHÂN VIÊN",
            new Size(420, 390),
            "Mật khẩu",
            "LƯU",
            Color.FromArgb(46, 204, 113),
            new Point(230, 310),
            new Point(310, 310))
{
    public CreateUserDto BuildCommand()
        => new(
            TxtUsername.Text,
            TxtFullName.Text,
            SelectedRoleValue,
            TxtPassword.Text,
            TxtConfirmPassword.Text);
}
