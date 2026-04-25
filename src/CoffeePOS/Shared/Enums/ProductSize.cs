using NpgsqlTypes;

namespace CoffeePOS.Shared.Enums;

public enum ProductSize
{
    [PgName("S")]
    S,

    [PgName("M")]
    M,

    [PgName("L")]
    L,

    [PgName("XL")]
    XL
}
