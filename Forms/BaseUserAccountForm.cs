using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public abstract class BaseUserAccountForm : BaseCrudForm
{
    protected readonly TextBox TxtUsername;
    protected readonly TextBox TxtFullName;
    protected readonly ComboBox CboRole;
    protected readonly TextBox TxtPassword;
    protected readonly TextBox TxtConfirmPassword;

    protected BaseUserAccountForm(
        string title,
        Size formSize,
        string passwordLabel,
        string saveButtonText,
        Color saveButtonColor,
        Point saveButtonLocation,
        Point cancelButtonLocation)
        : base(title, formSize)
    {
        TxtUsername = CreateTextBox(new Point(20, 45), 370);
        TxtFullName = CreateTextBox(new Point(20, 110), 370);

        CboRole = new ComboBox
        {
            Location = new Point(20, 175),
            Width = 370,
            Font = new Font("Segoe UI", 11),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(RoleOptionItem.Name),
            ValueMember = nameof(RoleOptionItem.Value),
            DataSource = UserRoleOptions.CreateDefault()
        };

        TxtPassword = CreatePasswordBox(new Point(20, 240), 370);
        TxtConfirmPassword = CreatePasswordBox(new Point(20, 305), 370);

        var btnSave = CreatePrimaryButton(saveButtonText, saveButtonLocation, new Size(100, 32), saveButtonColor);
        btnSave.Click += (_, _) => DialogResult = DialogResult.OK;

        var btnCancel = CreateCancelButton(cancelButtonLocation);
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(
        [
            CreateLabel("Tài khoản", new Point(20, 20)),
            TxtUsername,
            CreateLabel("Họ tên", new Point(20, 85)),
            TxtFullName,
            CreateLabel("Vai trò", new Point(20, 150)),
            CboRole,
            CreateLabel(passwordLabel, new Point(20, 215)),
            TxtPassword,
            CreateLabel("Xác nhận mật khẩu", new Point(20, 280)),
            TxtConfirmPassword,
            btnSave,
            btnCancel
        ]);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    protected int SelectedRoleValue => CboRole.SelectedValue is int roleValue ? roleValue : 1;
}
