namespace CoffeePOS.Shared.Dtos;

public record UpdateUserAccountDto(
    int TargetUserId,
    string Username,
    string FullName,
    int Role,
    string NewPassword,
    string ConfirmPassword
);
