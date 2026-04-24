using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.User;

public record AuthUserDto(
    int Id,
    string Username,
    string FullName,
    UserRole Role
);
