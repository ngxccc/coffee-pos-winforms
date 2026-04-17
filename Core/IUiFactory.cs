namespace CoffeePOS.Core;

public interface IUiFactory
{
    TForm CreateForm<TForm>(params object[] parameters) where TForm : Form;
    TControl CreateControl<TControl>(params object[] parameters) where TControl : UserControl;
}
