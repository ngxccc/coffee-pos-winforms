using AntdUI;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Features.Billing.ViewModels;

public class BillHistoryViewModel : NotifyProperty
{
    public BillHistoryDto OriginalDto { get; }

    public BillHistoryViewModel(BillHistoryDto dto)
    {
        OriginalDto = dto;

        Actions =
        [
            new CellButton("btnView", "Xem", TTypeMini.Primary) { Radius = 4 },
            new CellButton("btnReprint", "In lại", TTypeMini.Default) { Radius = 4 }
        ];
    }

    public int Id => OriginalDto.Id;
    public DateTime CreatedAt => OriginalDto.CreatedAt;
    public decimal TotalAmount => OriginalDto.TotalAmount;
    public int TotalItems => OriginalDto.TotalItems;

    public CellLink[] Actions { get; set; }
}
