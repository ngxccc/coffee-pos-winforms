using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing.Controls;

public partial class UC_CancelBillReason : UserControl, IValidatableComponent<string>
{
    public UC_CancelBillReason()
    {
        InitializeComponent();
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtReason.Text))
        {
            Invoke(() =>
            {
                MessageBoxHelper.Warning("Vui lòng nhập lý do hủy đơn!", owner: this, type: FeedbackType.Message);
                _txtReason.Focus();
            });
            return false;
        }
        return true;
    }

    public string GetPayload() => _txtReason.Text.Trim();
}
