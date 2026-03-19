using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record ProductGridDto(
    [property: DisplayName("Mã")] int Id,
    [property: DisplayName("Tên Sản Phẩm")] string Name,
    [property: DisplayName("Giá Bán")] decimal Price,
    [property: DisplayName("Danh Mục")] string CategoryName,
    // Gắn Browsable(false) để AutoGenerateColumns của DataGridView sẽ tự động ẨN nó đi
    [property: Browsable(false)] int CategoryId,
    [property: Browsable(false)] bool IsDeleted,
    [property: Browsable(false)] string? ImageUrl
);
