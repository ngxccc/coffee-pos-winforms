using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using AntdUI;

namespace CoffeePOS.Shared.Helpers;

internal static class DtoPropertyCache<T>
{
    // Dùng Dictionary thường thay vì Concurrent vì sau khi khởi tạo nó chỉ là Read-only
    public static readonly Dictionary<string, string> DisplayNames;

    // HACK: Static Constructor sẽ tự động chạy đúng 1 lần duy nhất khi T được gọi lần đầu tiên.
    // Quét toàn bộ Reflection một phát ăn ngay!
    static DtoPropertyCache()
    {
        DisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<DisplayNameAttribute>();
            DisplayNames[prop.Name] = attr != null ? attr.DisplayName : prop.Name;
        }
    }
}

public static class DtoHelper
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> Cache = new();

    public static string GetName<T>(string propertyName)
    {
        if (DtoPropertyCache<T>.DisplayNames.TryGetValue(propertyName, out var displayName))
        {
            return displayName;
        }
        return propertyName;
    }

    public static Column CreateCol<T>(string propertyName, Action<Column>? configure = null)
    {
        var col = new Column(propertyName, GetName<T>(propertyName));
        configure?.Invoke(col);
        return col;
    }
}
