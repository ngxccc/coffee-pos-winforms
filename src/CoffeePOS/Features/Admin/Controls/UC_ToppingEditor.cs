using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public partial class UC_ToppingEditor : UserControl, IValidatableComponent<UpsertToppingDto>
{
    private readonly int _toppingId;

    public UC_ToppingEditor(int id = 0, string name = "", decimal price = 0)
    {
        InitializeComponent();

        _toppingId = id;

        if (!string.IsNullOrWhiteSpace(name))
        {
            _txtName.Text = name;
        }

        _numPrice.Value = price;
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBoxHelper.Warning("Tên topping không được để trống!", owner: this);
            _txtName.Focus();
            return false;
        }

        if (_numPrice.Value < 0)
        {
            MessageBoxHelper.Warning("Giá bán không được nhỏ hơn 0!", owner: this);
            _numPrice.Focus();
            return false;
        }

        return true;
    }

    public UpsertToppingDto GetPayload()
        => new(_toppingId, _txtName.Text.Trim(), _numPrice.Value);
}
