namespace CoffeePOS.Shared.Helpers;

public static class UserRoleOptions
{
    public static List<RoleOptionItem> CreateDefault()
        =>
        [
            new(1, "Thu ngân"),
            new(0, "Admin")
        ];
}

public sealed record RoleOptionItem(int Value, string Name);
