using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Core;

public class UiFactory(IServiceProvider serviceProvider) : IUiFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public TForm CreateForm<TForm>() where TForm : Form
    {
        return _serviceProvider.GetRequiredService<TForm>();
    }

    public TControl CreateControl<TControl>() where TControl : UserControl
    {
        return _serviceProvider.GetRequiredService<TControl>();
    }
}
