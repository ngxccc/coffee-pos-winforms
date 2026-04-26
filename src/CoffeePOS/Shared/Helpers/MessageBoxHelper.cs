using AntdUI;

namespace CoffeePOS.Shared.Helpers;

public enum FeedbackType
{
    Modal,
    Message,
    Notification
}

public static class MessageBoxHelper
{
    public static DialogResult Success(string message, string title = "Thành công", IWin32Window? owner = null, FeedbackType type = FeedbackType.Modal)
        => DispatchFeedback(message, title, TType.Success, owner, type);

    public static DialogResult Info(string message, string title = "Thông báo", IWin32Window? owner = null, FeedbackType type = FeedbackType.Modal)
        => DispatchFeedback(message, title, TType.Info, owner, type);

    public static DialogResult Warning(string message, string title = "Cảnh báo", IWin32Window? owner = null, FeedbackType type = FeedbackType.Modal)
        => DispatchFeedback(message, title, TType.Warn, owner, type);

    public static DialogResult Error(string message, string title = "Lỗi", IWin32Window? owner = null, FeedbackType type = FeedbackType.Modal)
        => DispatchFeedback(message, title, TType.Error, owner, type);

    public static bool ConfirmYesNo(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, TType.Info, owner)
            .SetOk("ĐỒNG Ý", TTypeMini.Primary)
            .SetCancel("KHÔNG")
            .SetMaskClosable(false)
            .SetKeyboard(false);

        var result = Modal.open(config);
        return result == DialogResult.OK || result == DialogResult.Yes;
    }

    public static bool ConfirmWarning(string message, string title = "Xác nhận", IWin32Window? owner = null)
    {
        var config = BuildConfig(title, message, TType.Warn, owner)
            .SetOk("ĐỒNG Ý", TTypeMini.Warn)
            .SetCancel("KHÔNG")
            .SetMaskClosable(false)
            .SetKeyboard(false)
            .SetFont(UiTheme.BodyFont);

        var result = Modal.open(config);
        return result == DialogResult.OK || result == DialogResult.Yes;
    }

    // THE DISPATCHER
    private static DialogResult DispatchFeedback(string message, string title, TType icon, IWin32Window? owner, FeedbackType type)
    {
        Target target = ResolveTarget(owner);

        switch (type)
        {
            case FeedbackType.Message:
                ShowMessage(target, message, icon);
                return DialogResult.None;

            case FeedbackType.Notification:
                ShowNotification(target, title, message, icon);
                return DialogResult.None;

            case FeedbackType.Modal:
            default:
                var config = BuildConfig(title, message, icon, owner)
                    .SetOk("OK", GetOkType(icon))
                    .SetCancel((string?)null)
                    .SetMaskClosable(false)
                    .SetKeyboard(false);
                return Modal.open(config);
        }
    }

    private static void ShowMessage(Target target, string message, TType icon)
    {
        Font font = UiTheme.BodyFont;
        switch (icon)
        {
            case TType.Success: AntdUI.Message.success(target, message, font); break;
            case TType.Info: AntdUI.Message.info(target, message, font); break;
            case TType.Warn: AntdUI.Message.warn(target, message, font); break;
            case TType.Error: AntdUI.Message.error(target, message, font); break;
            default: AntdUI.Message.info(target, message, font); break;
        }
    }

    private static void ShowNotification(Target target, string title, string message, TType icon)
    {
        var config = new Notification.Config(target, title, message, icon, TAlignFrom.TR);
        Notification.open(config);
    }

    // UTILITIES
    private static Target ResolveTarget(IWin32Window? owner)
    {
        if (owner is Control control) return new Target(control);
        if (owner is Form form) return new Target(form);
        return new Target(new Form());
    }

    private static Modal.Config BuildConfig(string title, string message, TType icon, IWin32Window? owner)
    {
        if (owner is Control control) owner = control.FindForm();
        if (owner is Form form) return new Modal.Config(form, title, message, icon);
        return new Modal.Config(title, message, icon);
    }

    private static TTypeMini GetOkType(TType icon) => icon switch
    {
        TType.Warn => TTypeMini.Warn,
        TType.Error => TTypeMini.Error,
        _ => TTypeMini.Primary
    };
}
