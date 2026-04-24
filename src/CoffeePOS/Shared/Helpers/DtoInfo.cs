using System.ComponentModel;
using System.Reflection;

namespace CoffeePOS.Shared.Helpers;

public static class DtoInfo
{
    public static string GetName<T>(string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property == null) return propertyName;

        var attribute = property.GetCustomAttribute<DisplayNameAttribute>();
        return attribute != null ? attribute.DisplayName : propertyName;
    }
}
