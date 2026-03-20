namespace CoffeePOS.Shared.Helpers;

public static class MessageBoxHelper
{
    public static DialogResult Info(string message, string title = "Thông báo", IWin32Window? owner = null)
    {
        return owner is null
            ? MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information)
            : MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static DialogResult Warning(string message, string title = "Cảnh báo", IWin32Window? owner = null)
    {
        return owner is null
            ? MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            : MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static DialogResult Error(string message, string title = "Lỗi", IWin32Window? owner = null)
    {
        return owner is null
            ? MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error)
            : MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static bool ConfirmYesNo(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var result = owner is null
            ? MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            : MessageBox.Show(owner, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        return result == DialogResult.Yes;
    }

    public static bool ConfirmWarning(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var result = owner is null
            ? MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            : MessageBox.Show(owner, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }
}
