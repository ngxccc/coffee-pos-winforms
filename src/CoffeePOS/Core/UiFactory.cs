using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Core;

public class UiFactory(IServiceProvider serviceProvider) : IUiFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    // PERF: Supports dynamic runtime parameters alongside DI services
    public TForm CreateForm<TForm>(params object[] parameters) where TForm : Form
    {
        if (parameters.Length > 0)
            return ActivatorUtilities.CreateInstance<TForm>(_serviceProvider, parameters);

        return _serviceProvider.GetRequiredService<TForm>();
    }

    public TControl CreateControl<TControl>(params object[] parameters) where TControl : UserControl
    {
        if (parameters.Length > 0)
            return ActivatorUtilities.CreateInstance<TControl>(_serviceProvider, parameters);

        return _serviceProvider.GetRequiredService<TControl>();
    }
}
