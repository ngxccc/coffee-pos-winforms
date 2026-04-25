using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Category;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageCategories : UserControl
{
    private readonly ICategoryService _categoryService;
    private readonly ICategoryQueryService _categoryQueryService;

    private List<CategoryGridDto> _allCategories = [];
    private List<CategoryGridDto> _filteredCategories = [];

    public UC_ManageCategories(ICategoryService categoryService, ICategoryQueryService categoryQueryService)
    {
        _categoryService = categoryService;
        _categoryQueryService = categoryQueryService;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        Load += async (s, e) => await LoadDataAsync();
    }

    private void SetupTable()
    {
        _tableCategories.Columns =
        [
            DtoHelper.CreateCol<CategoryGridDto>(nameof(CategoryGridDto.Id), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<CategoryGridDto>(nameof(CategoryGridDto.Name), c => c.SortOrder = true),
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,

                Render = (value, record, rowIndex) => {
                    if (_switchTrash.Checked)
                        return new CellButton("restore", "Khôi phục")
                        {
                            Type = TTypeMini.Success
                        };
                    return new CellButton[] {
                        new("edit", "Sửa") {
                            Type = TTypeMini.Primary,
                        },
                        new("delete", "Xoá") {
                            Type = TTypeMini.Error
                        }
                    };
                }
            }
        ];

        _tableCategories.CellButtonClick += TableCategories_CellButtonClick;
    }

    private void SetupEvents()
    {
        _txtSearch.TextChanged += ApplyFilterAndSort;
        _switchTrash.CheckedChanged += HandleTrashModeChanged;
        _btnAdd.Click += HandleAddCategory;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await Spin.open(_tableCategories, async cfg =>
            {
                _allCategories = await _categoryQueryService.GetCategoryGridAsync(_switchTrash.Checked);
                ApplyFilterAndSort(this, EventArgs.Empty);
            });
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", type: FeedbackType.Message);
        }
    }

    private void ApplyFilterAndSort(object? sender, EventArgs e)
    {
        string keyword = _txtSearch.Text.Trim();

        _filteredCategories = string.IsNullOrEmpty(keyword)
            ? [.. _allCategories]
            : [.. _allCategories.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _tableCategories.SuspendLayout();
        _tableCategories.DataSource = _filteredCategories;
        _tableCategories.ResumeLayout(true);
    }

    private void TableCategories_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not CategoryGridDto selectedItem) return;

        if (e.Btn.Id == "edit") HandleEditCategory(selectedItem);
        if (e.Btn.Id == "delete") HandleDeleteCategory(selectedItem);
        if (e.Btn.Id == "restore") HandleRestoreCategory(selectedItem);
    }

    private async void HandleTrashModeChanged(object? sender, BoolEventArgs e)
    {
        _tableCategories.BackColor = _switchTrash.Checked ? Color.MistyRose : UiTheme.Surface;
        _tableCategories.DataSource = null;
        await LoadDataAsync();
    }

    private void HandleAddCategory(object? sender, EventArgs e)
    {
        try
        {
            var catEditor = new UC_CategoryEditor();

            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");

            var config = new Modal.Config(form, "THÊM DANH MỤC MỚI", catEditor)
            {
                Font = UiTheme.BodyFont,
                OkText = "Lưu",
                CancelText = "Huỷ",
                OnOk = (cfg) =>
                {
                    if (!catEditor.ValidateInput()) return false;

                    ExecuteSaveCategoryAsync(catEditor.GetPayload(), isUpdate: false, categoryId: 0);
                    return true;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private async void HandleEditCategory(CategoryGridDto selectedItem)
    {
        try
        {
            CategoryDetailDto? cat = null;

            await Spin.open(this, async cfg =>
            {
                cat = await _categoryQueryService.GetCategoryByIdAsync(selectedItem.Id);
            });

            if (cat == null)
            {
                MessageBoxHelper.Error("Không tìm thấy danh mục!", type: FeedbackType.Message);
                return;
            }

            var uiFields = new UC_CategoryEditor(cat.Name);
            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

            var config = new Modal.Config(form, $"CẬP NHẬT: {cat.Name}", uiFields)
            {
                Font = UiTheme.BodyFont,
                OkText = "Cập nhật",
                CancelText = "Huỷ",
                OnOk = (modalCfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;

                    ExecuteSaveCategoryAsync(uiFields.GetPayload(), isUpdate: true, categoryId: cat.Id);
                    return true;
                }
            };

            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private async void HandleDeleteCategory(CategoryGridDto selectedItem)
    {
        try
        {
            if (MessageBoxHelper.ConfirmWarning($"Xóa danh mục '{selectedItem.Name}'?\n(Hành động này sẽ giấu danh mục khỏi Menu)", "Xác nhận", this))
            {
                await _categoryService.DeleteCategoryAsync(selectedItem.Id);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
    }


    private async void HandleRestoreCategory(CategoryGridDto selectedItem)
    {
        try
        {
            if (MessageBoxHelper.ConfirmWarning($"Khôi phục '{selectedItem.Name}' trở lại phần mềm?", "Xác nhận", this))
            {
                await _categoryService.RestoreCategoryAsync(selectedItem.Id);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
    }

    private void ExecuteSaveCategoryAsync(CategoryPayload payload, bool isUpdate, int categoryId)
    {
        Target target = new(this);

        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "save_cat";

            try
            {
                var dto = new UpsertCategoryDto(categoryId, payload.Name);

                if (isUpdate) await _categoryService.UpdateCategoryAsync(dto);
                else await _categoryService.AddCategoryAsync(dto);

                Invoke(() => MessageBoxHelper.Success("Lưu thành công!", owner: this, type: FeedbackType.Message));
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error(ex.Message, type: FeedbackType.Message));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("save_cat"));
            }
        }, UiTheme.BodyFont);
    }
}
