namespace CoffeePOS.Shared.Dtos;

public record AuthUserDto(
    int Id,
    string Username,
    string FullName,
    int Role
);
