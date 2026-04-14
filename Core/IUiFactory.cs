namespace CoffeePOS.Core;

public interface IUiFactory
{
    // Gọi Form truyền thống (Ví dụ: ShiftReportForm)
    TForm CreateForm<TForm>() where TForm : Form;

    // Gọi UserControl phức tạp (Ví dụ: một UC cần load danh sách từ DB lúc khởi tạo)
    TControl CreateControl<TControl>() where TControl : UserControl;
}
