using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;

using CoffeePOS.Shared.Dtos.Category;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public class UC_ManageCategories : UserControl
{
    private readonly ICategoryService _categoryService;
    private readonly ICategoryQueryService _categoryQueryService;

    private UC_CategoriesHeaderToolbar _toolbar = null!;
    private DataGridView _dgvCategories = null!;
    private StatefulSortableGrid<CategoryGridDto> _categoriesGrid = null!;

    private List<CategoryGridDto> _allCategories = [];
    private List<CategoryGridDto> _filteredCategories = [];

    public UC_ManageCategories(ICategoryService categoryService, ICategoryQueryService categoryQueryService)
    {
        _categoryService = categoryService;
        _categoryQueryService = categoryQueryService;
        InitializeUI();
        _ = LoadDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        _toolbar = new UC_CategoriesHeaderToolbar();
        _toolbar.SearchChanged += ApplyFilterAndSort;
        _toolbar.AddClicked += AddCategoryAsync;
        _toolbar.EditClicked += EditCategoryAsync;
        _toolbar.DeleteClicked += DeleteCategoryAsync;
        _toolbar.TrashModeChanged += ChkTrashMode_CheckedChanged;

        _dgvCategories = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        _dgvCategories.ApplyStandardAdminStyle();
        _dgvCategories.CellDoubleClick += EditCategoryAsync;

        var hostPanel = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Back = UiTheme.Surface,
            Padding = new Padding(UiTheme.BlockGap)
        };
        hostPanel.Controls.Add(_dgvCategories);

        _categoriesGrid = new StatefulSortableGrid<CategoryGridDto>(_dgvCategories);
        _categoriesGrid.AttachSortHandler();
        _categoriesGrid.SortChanged += ApplyFilterAndSort;

        Controls.Add(hostPanel);
        Controls.Add(_toolbar);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _categoriesGrid.CapturePosition();

            _allCategories = await _categoryQueryService.GetCategoryGridAsync(_toolbar.IsTrashMode);
            ApplyFilterAndSort();

            _categoriesGrid.RestorePosition();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", owner: this);
        }
    }

    private async void ChkTrashMode_CheckedChanged(object? sender, EventArgs e)
    {
        _dgvCategories.BackgroundColor = _toolbar.IsTrashMode ? Color.MistyRose : Color.WhiteSmoke;

        await LoadDataAsync();
    }

    private async void AddCategoryAsync(object? s, EventArgs e)
    {
        var uiFields = new UC_CategoryFields();
        using var shell = new DynamicModalShell<CategoryPayload>("THÊM DANH MỤC MỚI", uiFields, new Size(420, 220));

        if (shell.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var payload = shell.ExtractData();
            await _categoryService.AddCategoryAsync(new UpsertCategoryDto(0, payload.Name));
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error(ex.Message, owner: this);
        }
    }

    private async void EditCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)_dgvCategories.SelectedRows[0].Cells[nameof(CategoryGridDto.Id)].Value;
        var cat = await _categoryQueryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var uiFields = new UC_CategoryFields(cat.Name);
        using var shell = new DynamicModalShell<CategoryPayload>($"SỬA DANH MỤC: {cat.Name}", uiFields, new Size(420, 220));

        if (shell.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var payload = shell.ExtractData();
            await _categoryService.UpdateCategoryAsync(new UpsertCategoryDto(cat.Id, payload.Name));
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error(ex.Message, owner: this);
        }
    }

    private async void DeleteCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        string name = _dgvCategories.SelectedRows[0].Cells[nameof(CategoryGridDto.Name)].Value.ToString()!;
        int id = (int)_dgvCategories.SelectedRows[0].Cells[nameof(CategoryGridDto.Id)].Value;

        if (_toolbar.IsTrashMode)
        {
            if (MessageBoxHelper.ConfirmWarning($"Khôi phục danh mục '{name}'?", "Xác nhận", this))
            {
                try
                {
                    await _categoryService.RestoreCategoryAsync(id);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
                }
            }
        }
        else
        {
            if (MessageBoxHelper.ConfirmWarning($"Bạn có chắc chắn muốn xóa danh mục '{name}'?", "Xác nhận", this))
            {
                try
                {
                    await _categoryService.DeleteCategoryAsync(id);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.Error(ex.Message, owner: this);
                }
            }
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = _toolbar.SearchText.Trim();

        _filteredCategories = string.IsNullOrEmpty(keyword)
            ? [.. _allCategories]
            : [.. _allCategories.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _categoriesGrid.Bind(_filteredCategories);
    }
}
