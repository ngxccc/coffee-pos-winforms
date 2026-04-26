using CoffeePOS.Forms.Core;

namespace CoffeePOS.Features.Billing.Controls;

public partial class UC_StartingCash : UserControl, IValidatableComponent<decimal>
{
    public UC_StartingCash()
    {
        InitializeComponent();
    }

    public bool ValidateInput() => true;

    public decimal GetPayload() => _numCash.Value;
}
