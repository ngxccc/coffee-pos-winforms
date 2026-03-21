namespace CoffeePOS.Shared.Dtos;

public record ResetUserPasswordDto(
    int TargetUserId,
    string NewPassword,
    string ConfirmPassword
);
