namespace CoffeePOS.Shared.Helpers;

public static class MessageBoxHelper
{
    public static DialogResult Info(string message, string title = "Thông báo", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, AntdUI.TType.Info, owner)
            .SetOk("OK", AntdUI.TTypeMini.Primary)
            .SetCancel((string?)null)
            .SetMaskClosable(false)
            .SetKeyboard(false);

        return AntdUI.Modal.open(config);
    }

    public static DialogResult Warning(string message, string title = "Cảnh báo", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, AntdUI.TType.Warn, owner)
            .SetOk("OK", AntdUI.TTypeMini.Warn)
            .SetCancel((string?)null)
            .SetMaskClosable(false)
            .SetKeyboard(false);

        return AntdUI.Modal.open(config);
    }

    public static DialogResult Error(string message, string title = "Lỗi", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, AntdUI.TType.Error, owner)
            .SetOk("OK", AntdUI.TTypeMini.Error)
            .SetCancel((string?)null)
            .SetMaskClosable(false)
            .SetKeyboard(false);

        return AntdUI.Modal.open(config);
    }

    public static bool ConfirmYesNo(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, AntdUI.TType.Info, owner)
            .SetOk("ĐỒNG Ý", AntdUI.TTypeMini.Primary)
            .SetCancel("KHÔNG")
            .SetMaskClosable(false)
            .SetKeyboard(false);

        var result = AntdUI.Modal.open(config);

        return result == DialogResult.OK || result == DialogResult.Yes;
    }

    public static bool ConfirmWarning(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, AntdUI.TType.Warn, owner)
            .SetOk("ĐỒNG Ý", AntdUI.TTypeMini.Warn)
            .SetCancel("KHÔNG")
            .SetMaskClosable(false)
            .SetKeyboard(false);

        var result = AntdUI.Modal.open(config);

        return result == DialogResult.OK || result == DialogResult.Yes;
    }

    private static AntdUI.Modal.Config BuildConfig(string title, string message, AntdUI.TType icon, IWin32Window? owner)
    {
        if (owner is Form form)
        {
            return new AntdUI.Modal.Config(form, title, message, icon);
        }

        if (owner is Control control)
        {
            return new AntdUI.Modal.Config(new AntdUI.Target(control), title, message, icon);
        }

        return new AntdUI.Modal.Config(title, message, icon);
    }
}
