using System.Data.Common;

namespace CoffeePOS.Shared.Helpers;

public static class DbDataReaderExtensions
{
    public static T GetRequired<T>(this DbDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            throw new InvalidOperationException($"Cột '{columnName}' không được phép NULL.");

        return reader.GetFieldValue<T>(ordinal);
    }

    public static T? GetNullable<T>(this DbDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            return default;

        return reader.GetFieldValue<T>(ordinal);
    }

    public static int GetRequiredIntFromScalar(this object? value, string context)
    {
        if (value is null || value is DBNull)
            throw new InvalidOperationException($"Scalar result cannot be null: {context}");

        return Convert.ToInt32(value);
    }
}
