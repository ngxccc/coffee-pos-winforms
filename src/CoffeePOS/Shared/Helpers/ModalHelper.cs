using AntdUI;

namespace CoffeePOS.Shared.Helpers;

/// <summary>
/// Helper để tổng hợp logic mở modal + spin + reload dữ liệu cho các form.
/// Giảm lặp code (DRY) giữa UC_ManageProducts, UC_ManageCategories, UC_ManageToppings, v.v.
/// </summary>
public static class ModalHelper
{
    /// <summary>
    /// Mở modal với xác thực input và tự động thực thi hành động sau khi confirm.
    /// </summary>
    /// <param name="owner">UserControl chứa modal (để tìm parent form)</param>
    /// <param name="title">Tiêu đề modal</param>
    /// <param name="content">Nội dung editor control</param>
    /// <param name="validator">Hàm xác thực input (return false để hủy save)</param>
    /// <param name="onOkAction">Hàm thực thi sau khi user click OK (nên là async DB action)</param>
    public static void OpenModal(
        UserControl owner,
        string title,
        UserControl content,
        Func<bool> validator,
        Func<Task> onOkAction)
    {
        Form form = owner.FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

        var config = new Modal.Config(form, title, content)
        {
            Font = UiTheme.BodyFont,
            OkText = "Lưu",
            CancelText = "Hủy",
            OnOk = (cfg) =>
            {
                if (!validator()) return false;
                ExecuteActionWithSpin(owner, onOkAction);
                return true;
            }
        };

        Modal.open(config);
    }

    /// <summary>
    /// Mở modal chỉ để xem/đóng (không có hành động save).
    /// </summary>
    public static void OpenModalReadOnly(
        UserControl owner,
        string title,
        UserControl content)
    {
        Form form = owner.FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

        var config = new Modal.Config(form, title, content)
        {
            Font = UiTheme.BodyFont,
            OkText = "Đóng",
            CancelText = null,
            OnOk = (cfg) => true
        };

        Modal.open(config);
    }

    /// <summary>
    /// Thực thi hành động async với Message.loading overlay + tự động reload dữ liệu sau.
    /// </summary>
    /// <param name="owner">UserControl chứa table (để hiển thị loading overlay)</param>
    /// <param name="action">Async action để thực thi (thường là DB command)</param>
    /// <param name="reloadAction">Async action để reload dữ liệu (mặc định là null)</param>
    public static async void ExecuteActionWithSpin(
        UserControl owner,
        Func<Task> action,
        Func<Task>? reloadAction = null)
    {
        Target target = new(owner);
        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "admin_action";
            try
            {
                await action();
                if (reloadAction != null)
                {
                    await reloadAction();
                }
            }
            catch (Exception ex)
            {
                owner.Invoke(() =>
                    MessageBoxHelper.Error($"Lỗi thao tác: {ex.Message}", owner: owner));
            }
            finally
            {
                owner.Invoke(() => AntdUI.Message.close_id("admin_action"));
            }
        }, UiTheme.BodyFont);
    }

    /// <summary>
    /// Thực thi hành động async trong Spin + xác nhận trước + reload dữ liệu.
    /// </summary>
    public static async void ExecuteActionWithConfirmAndSpin(
        UserControl owner,
        string confirmMessage,
        Func<Task> action,
        Func<Task>? reloadAction = null)
    {
        if (!MessageBoxHelper.ConfirmWarning(confirmMessage, "Xác nhận", owner))
            return;

        ExecuteActionWithSpin(owner, action, reloadAction);
    }

    /// <summary>
    /// Mở modal với logic xác thực phức tạp (lấy payload từ content, có Invoke/cross-thread).
    /// </summary>
    /// <param name="owner">UserControl chứa modal</param>
    /// <param name="title">Tiêu đề modal</param>
    /// <param name="content">Nội dung editor control</param>
    /// <param name="complexValidator">Callback xác thực, return (isValid: bool, payload: extracted data)</param>
    /// <param name="onOkAction">Async action nhận payload đã extract để thực thi</param>
    public static void OpenModalWithComplexValidator<T>(
        UserControl owner,
        string title,
        UserControl content,
        Func<(bool isValid, T? payload)> complexValidator,
        Func<T, Task> onOkAction) where T : class
    {
        Form form = owner.FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

        var config = new Modal.Config(form, title, content)
        {
            Font = UiTheme.BodyFont,
            OkText = "Lưu",
            CancelText = "Hủy",
            OnOk = (cfg) =>
            {
                var (isValid, payload) = complexValidator();
                if (!isValid || payload == null)
                    return false;

                ExecuteActionWithSpin(owner, () => onOkAction(payload));
                return true;
            }
        };

        Modal.open(config);
    }
}
