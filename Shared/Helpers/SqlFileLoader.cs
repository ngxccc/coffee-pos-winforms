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
}
