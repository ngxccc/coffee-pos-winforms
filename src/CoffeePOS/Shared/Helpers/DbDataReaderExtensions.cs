using System.Data.Common;

namespace CoffeePOS.Shared.Helpers;

public static class DbDataReaderExtensions
{
    public static int GetRequiredIntFromScalar(this object? value, string context)
    {
        if (value is null || value is DBNull)
        {
            throw new InvalidOperationException($"Scalar result cannot be null: {context}");
        }

        return Convert.ToInt32(value);
    }

    public static string? GetNullableString(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value is DBNull ? null : Convert.ToString(value);
    }

    public static string GetRequiredString(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        if (value is DBNull)
        {
            throw new InvalidOperationException($"Column '{columnName}' cannot be null.");
        }

        return Convert.ToString(value) ?? string.Empty;
    }

    public static int GetRequiredInt(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value is DBNull
            ? throw new InvalidOperationException($"Column '{columnName}' cannot be null.")
            : Convert.ToInt32(value);
    }

    public static decimal GetRequiredDecimal(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value is DBNull
            ? throw new InvalidOperationException($"Column '{columnName}' cannot be null.")
            : Convert.ToDecimal(value);
    }

    public static bool GetRequiredBool(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value is DBNull
            ? throw new InvalidOperationException($"Column '{columnName}' cannot be null.")
            : Convert.ToBoolean(value);
    }

    public static DateTime? GetNullableDateTime(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];
        if (value is DBNull)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        return Convert.ToDateTime(value);
    }

    public static DateTime GetDateOnlyAsDateTime(this DbDataReader reader, string columnName)
    {
        object value = reader[columnName];

        return value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),

            DateTime dateTime => dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToLocalTime()
                : dateTime,

            DBNull => throw new InvalidOperationException($"Column '{columnName}' cannot be null."),

            _ => Convert.ToDateTime(value)
        };
    }
}
