using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Forms;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Features.Admin;

public class UC_ManageCategories : UserControl
{
    private readonly ICategoryService _categoryService;
    private readonly ICategoryQueryService _categoryQueryService;
    private readonly IServiceProvider _serviceProvider;

    private UC_CategoriesHeaderToolbar _toolbar = null!;
    private DataGridView _dgvCategories = null!;
    private StatefulSortableGrid<CategoryGridDto> _categoriesGrid = null!;

    private List<CategoryGridDto> _allCategories = [];
    private List<CategoryGridDto> _filteredCategories = [];

    public UC_ManageCategories(ICategoryService categoryService, ICategoryQueryService categoryQueryService, IServiceProvider serviceProvider)
    {
        _categoryService = categoryService;
        _categoryQueryService = categoryQueryService;
        _serviceProvider = serviceProvider;
        InitializeUI();
        _ = LoadDataAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

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

        _categoriesGrid = new StatefulSortableGrid<CategoryGridDto>(_dgvCategories);
        _categoriesGrid.AttachSortHandler();
        _categoriesGrid.SortChanged += ApplyFilterAndSort;

        Controls.Add(_dgvCategories);
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
        var form = _serviceProvider.GetRequiredService<AddCategoryForm>();
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void EditCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)_dgvCategories.SelectedRows[0].Cells["Id"].Value;
        var cat = await _categoryQueryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var form = _serviceProvider.GetRequiredService<EditCategoryForm>();
        form.LoadCategory(cat);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void DeleteCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        string name = _dgvCategories.SelectedRows[0].Cells["Name"].Value.ToString()!;
        int id = (int)_dgvCategories.SelectedRows[0].Cells["Id"].Value;

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
