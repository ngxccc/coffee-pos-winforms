using System.Collections.Concurrent;
using System.Reflection;

namespace CoffeePOS.Shared.Helpers;

public static class SqlFileLoader
{
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Assembly CurrentAssembly = typeof(SqlFileLoader).Assembly;
    private static readonly string[] ResourceNames = CurrentAssembly.GetManifestResourceNames();

    public static string Load(string sqlKey)
    {
        return Cache.GetOrAdd(sqlKey, static key =>
        {
            string expectedSuffix = $".Sql.{key}";
            string? resourceName = ResourceNames.FirstOrDefault(name =>
                name.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Embedded SQL resource not found for key: {key}");
            using Stream? stream = CurrentAssembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Cannot open embedded SQL resource stream: {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
    }

    public static void ValidateAllSqlKeys()
    {
        foreach (string sqlKey in EnumerateSqlKeys())
        {
            _ = Load(sqlKey);
        }
    }

    private static IEnumerable<string> EnumerateSqlKeys()
    {
        var nestedTypes = typeof(SqlKeys).GetNestedTypes(BindingFlags.Public);

        foreach (var nestedType in nestedTypes)
        {
            var fields = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string) && field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetRawConstantValue() as string;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}
