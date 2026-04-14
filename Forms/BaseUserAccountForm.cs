using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public abstract class BaseUserAccountForm : BaseCrudForm
{
    protected readonly TextBox TxtUsername;
    protected readonly TextBox TxtFullName;
    protected readonly ComboBox CboRole;
    protected readonly TextBox TxtPassword;
    protected readonly TextBox TxtConfirmPassword;

    protected readonly Button BtnSave;
    protected readonly Button BtnCancel;

    // FIXME: Removed excessive parameters. Base class should only enforce structure, not layout specifics.
    protected BaseUserAccountForm(string title, Size formSize, string passwordLabel)
        : base(title, formSize)
    {
        // TODO: Replace absolute Points with TableLayoutPanel for DPI awareness in production
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

        // HACK: Base layout locations. Subclasses can modify these properties if strictly needed.
        BtnSave = CreatePrimaryButton("LƯU", new Point(20, 370), new Size(100, 32), Color.Blue);
        BtnCancel = CreateCancelButton(new Point(130, 370));

        // PERF: Native DialogResult routing. Zero lambda allocation.
        BtnCancel.DialogResult = DialogResult.Cancel;

        // WHY: Intercept the click to run validation before officially setting DialogResult.OK
        BtnSave.Click += BtnSave_Click;

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
            BtnSave,
            BtnCancel
        ]);

        AcceptButton = BtnSave;
        CancelButton = BtnCancel;
    }

    protected UserRole SelectedRole => CboRole.SelectedValue is UserRole roleValue ? roleValue : UserRole.Cashier;

    // WHY: Force subclasses to implement their own business rules before allowing the form to close
    protected abstract bool ValidateInput();

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (ValidateInput())
        {
            DialogResult = DialogResult.OK;
        }
    }
}
