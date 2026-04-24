using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record CategoryPayload(string Name);

public partial class UC_CategoryEditor : UserControl, IValidatableComponent<CategoryPayload>
{
    public UC_CategoryEditor(string? existingName = null)
    {
        InitializeComponent();

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
