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

    public static string GetRequiredString(this DbDataReader reader, string col) => reader.GetRequired<string>(col);
    public static int GetRequiredInt(this DbDataReader reader, string col) => reader.GetRequired<int>(col);
    public static decimal GetRequiredDecimal(this DbDataReader reader, string col) => reader.GetRequired<decimal>(col);
    public static bool GetRequiredBool(this DbDataReader reader, string col) => reader.GetRequired<bool>(col);
    public static DateTime GetRequiredDateTime(this DbDataReader reader, string col) => reader.GetRequired<DateTime>(col);

    public static string? GetNullableString(this DbDataReader reader, string col) => reader.GetNullable<string?>(col);
    public static DateTime? GetNullableDateTime(this DbDataReader reader, string col) => reader.GetNullable<DateTime?>(col);

    public static int GetRequiredIntFromScalar(this object? value, string context)
    {
        if (value is null || value is DBNull)
            throw new InvalidOperationException($"Scalar result cannot be null: {context}");

        return Convert.ToInt32(value);
    }

    public static DateTime GetDateOnlyAsDateTime(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];

        return value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            DateTime dateTime => dateTime.Kind == DateTimeKind.Utc ? dateTime.ToLocalTime() : dateTime,
            DBNull => throw new InvalidOperationException($"Column '{columnName}' cannot be null."),
            _ => Convert.ToDateTime(value)
        };
    }
}
