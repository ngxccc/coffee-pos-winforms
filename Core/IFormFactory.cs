namespace CoffeePOS.Core;

public interface IFormFactory
{
    TForm CreateForm<TForm>() where TForm : Form;
}
