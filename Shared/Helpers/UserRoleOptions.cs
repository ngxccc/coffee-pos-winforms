using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Helpers;

public static class UserRoleOptions
{
    // WHY: Used to bind data source for UI ComboBoxes in WinForms
    public static List<RoleOptionItem> CreateDefault()
        =>
        [
            new(UserRole.Admin, "Admin"),
            new(UserRole.Cashier, "Thu ngân")
        ];

    // WHY: Used to format GridView cells or Display labels
    public static string ToDisplayName(this UserRole role)
        => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Cashier => "Thu ngân",
            _ => role.ToString()
        };
}

public sealed record RoleOptionItem(UserRole Value, string Name);
