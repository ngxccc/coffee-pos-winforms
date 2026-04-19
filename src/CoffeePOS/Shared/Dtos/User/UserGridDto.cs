using System.ComponentModel;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.User;

public record UserGridDto(
    [property: DisplayName("Mã")] int Id,
    [property: DisplayName("Tài khoản")] string Username,
    [property: DisplayName("Họ tên")] string FullName,
    [property: Browsable(false)] UserRole Role,
    [property: DisplayName("Vai trò")] string RoleName,
    [property: Browsable(false)] bool IsActive,
    [property: DisplayName("Trạng thái")] string Status
);
