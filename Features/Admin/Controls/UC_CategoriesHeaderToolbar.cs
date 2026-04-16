using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_CategoriesHeaderToolbar : BaseAdminHeaderToolbar
{
    protected override string Title => "QUẢN LÝ DANH MỤC";
    protected override string SearchPlaceholder => "Nhập tên danh mục để tìm...";
    protected override bool ShowTrashMode => true;
}
