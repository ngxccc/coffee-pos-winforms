using NpgsqlTypes;

namespace CoffeePOS.Shared.Enums;

public enum BillOrderType
{
    [PgName("dine_in")]
    DineIn,
    [PgName("takeaway")]
    TakeAway
}
