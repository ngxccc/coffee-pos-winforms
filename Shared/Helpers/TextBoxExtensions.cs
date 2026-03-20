namespace CoffeePOS.Shared.Helpers;

public static class TextBoxExtensions
{
    public static void OnDebouncedTextChanged(this TextBox textBox, int debounceTimeMs, Action action)
    {
        // Mỗi TextBox gọi hàm này sẽ tự đẻ ra 1 cái Timer riêng ngầm bên dưới
        var timer = new System.Windows.Forms.Timer { Interval = debounceTimeMs };

        timer.Tick += (sender, args) =>
        {
            timer.Stop();
            action(); // Gọi cái hàm Form truyền vào (ApplyFilterAndSort)
        };

        textBox.TextChanged += (sender, args) =>
        {
            timer.Stop();
            timer.Start();
        };

        // Tự sát khi TextBox bị hủy tránh Memory Leak
        textBox.Disposed += (sender, args) =>
        {
            timer.Stop();
            timer.Dispose();
        };
    }
}
