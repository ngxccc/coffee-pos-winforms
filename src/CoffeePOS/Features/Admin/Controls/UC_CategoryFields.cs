using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record CategoryPayload(string Name);

public class UC_CategoryFields : UserControl, IValidatableComponent<CategoryPayload>
{
    private readonly TextBox _txtName;

    public UC_CategoryFields(string? existingName = null)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        Controls.Add(new Label
        {
            Text = "Tên danh mục",
            Location = new Point(20, 20),
            AutoSize = true
        });

        _txtName = new TextBox
        {
            Location = new Point(20, 45),
            Width = 350,
            Font = new Font("Segoe UI", 11)
        };
        Controls.Add(_txtName);

        if (!string.IsNullOrWhiteSpace(existingName))
        {
            _txtName.Text = existingName;
        }
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBoxHelper.Warning("Tên danh mục không được để trống!", owner: this);
            _txtName.Focus();
            return false;
        }

        return true;
    }

    public CategoryPayload GetPayload()
        => new(_txtName.Text.Trim());
}
