using AntdUI;

namespace CoffeePOS.Shared.Helpers;

public class AntdLanguageProvider : ILocalization
{
    private readonly Dictionary<string, string> _viDict = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Today"] = "Hôm nay",
        ["Now"] = "Bây giờ",
        ["Select date"] = "Chọn ngày",
        ["Sun"] = "CN",
        ["Mon"] = "T2",
        ["Tue"] = "T3",
        ["Wed"] = "T4",
        ["Thu"] = "T5",
        ["Fri"] = "T6",
        ["Sat"] = "T7",

        ["Year"] = "Năm",
        ["Month"] = "Tháng",

        ["No data"] = "Chưa có dữ liệu",
        ["Please select"] = "Vui lòng chọn",
        ["OK"] = "Đồng ý",
        ["Cancel"] = "Hủy bỏ"
    };

    public string GetLocalizedString(string key)
    {
        if (_viDict.TryGetValue(key, out string? translated))
        {
            return translated;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[MISSING TRANSLATION KEY]: '{key}'");
            return key;
        }
    }
}
