namespace CoffeePOS.Shared.Dtos.Bill;

public record CreateBillDto(
    int BuzzerNumber,
    int CreatedByUserId,
    decimal TotalAmount,
    List<CreateBillItemDto> Items
);
