using System.Reflection;

namespace CoffeePOS.Shared.Helpers;

public static class SortHelper
{
    // Dùng cho MỌI LOẠI DANH SÁCH (IEnumerable<T>)
    public static IEnumerable<T> DynamicSort<T>(this IEnumerable<T> source, string? columnName, bool ascending)
    {
        if (string.IsNullOrEmpty(columnName)) return source;

        // Lấy thông tin Property (Cột) dựa trên tên chuỗi (Ví dụ: "Price")
        PropertyInfo? prop = typeof(T).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (prop == null) return source; // Nếu gõ sai tên cột thì khỏi sort

        // Phép thuật OrderBy dùng Reflection (Lấy Value ra để so sánh)
        return ascending
            ? source.OrderBy(x => prop.GetValue(x, null))
            : source.OrderByDescending(x => prop.GetValue(x, null));
    }
}
