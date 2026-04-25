using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record ProductSizePayload(string SizeName, decimal PriceAdjustment);

public partial class UC_ProductSizeEditor : UserControl, IValidatableComponent<ProductSizePayload>
{
    public UC_ProductSizeEditor(string? existingSizeName = null, decimal existingPrice = 0)
    {
        InitializeComponent();
        SetupSizeDropdown();

        if (!string.IsNullOrEmpty(existingSizeName))
        {
            _cboSizeName.SelectedValue = existingSizeName;
            _cboSizeName.Enabled = false;
            _nudPriceAdjustment.Value = existingPrice;
        }
    }

    private void SetupSizeDropdown()
    {
        _cboSizeName.Items.AddRange(Enum.GetNames<ProductSize>());
        _cboSizeName.SelectedIndex = 0;
    }

    public bool ValidateInput()
    {
        if (_cboSizeName.SelectedValue == null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn Size!", owner: this);
            return false;
        }
        return true;
    }

    public ProductSizePayload GetPayload()
        => new(_cboSizeName.SelectedValue?.ToString()!, _nudPriceAdjustment.Value);
}
