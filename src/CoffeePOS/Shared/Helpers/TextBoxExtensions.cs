namespace CoffeePOS.Shared.Helpers;

public static class TextBoxExtensions
{
    public static void OnDebouncedTextChanged(this AntdUI.Input input, int debounceTimeMs, Action action)
    {
        var timer = new System.Windows.Forms.Timer { Interval = debounceTimeMs };

        timer.Tick += (sender, args) =>
        {
            timer.Stop();
            action();
        };

        input.TextChanged += (sender, args) =>
        {
            timer.Stop();
            timer.Start();
        };

        input.Disposed += (sender, args) =>
        {
            timer.Stop();
            timer.Dispose();
        };
    }
}
