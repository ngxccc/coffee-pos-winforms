using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Core;

public class FormFactory(IServiceProvider serviceProvider) : IFormFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public TForm CreateForm<TForm>() where TForm : Form
    {
        return _serviceProvider.GetRequiredService<TForm>();
    }
}
