using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.Category;

public record CategoryGridDto(
    [property: DisplayName("Mã")] int Id,
    [property: DisplayName("Tên Danh Mục")] string Name
);
