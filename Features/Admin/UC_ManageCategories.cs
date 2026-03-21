using CoffeePOS.Forms;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Features.Admin;

public class UC_ManageCategories : UserControl
{
    private readonly ICategoryService _categoryService;
    private readonly ICategoryQueryService _categoryQueryService;
    private readonly IServiceProvider _serviceProvider;

    private DataGridView _dgvCategories = null!;
    private StatefulSortableGrid<CategoryGridDto> _categoriesGrid = null!;
    private TextBox _txtSearch = null!;
    private IconButton _btnDelete = null!;
    private IconButton _btnEdit = null!;
    private IconButton _btnAdd = null!;
    private CheckBox _chkTrashMode = null!;

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

        Panel pnlTop = new()
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(0, 10, 0, 10)
        };
        Label lblTitle = new()
        {
            Text = "QUẢN LÝ DANH MỤC",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Location = new Point(0, 20)
        };

        _txtSearch = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = "Nhập tên danh mục để tìm..."
        };
        _txtSearch.OnDebouncedTextChanged(300, ApplyFilterAndSort);

        _chkTrashMode = new CheckBox
        {
            Text = "Xem Thùng Rác",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(570, 25),
            Cursor = Cursors.Hand
        };
        _chkTrashMode.CheckedChanged += ChkTrashMode_CheckedChanged;

        FlowLayoutPanel flpBtns = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnDelete = UIHelper.CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), DeleteCategoryAsync);
        _btnEdit = UIHelper.CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditCategoryAsync);
        _btnAdd = UIHelper.CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddCategoryAsync);

        flpBtns.Controls.AddRange([_btnDelete, _btnEdit, _btnAdd]);
        pnlTop.Controls.AddRange([lblTitle, flpBtns]);

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
        Controls.Add(pnlTop);
        pnlTop.Controls.Add(_txtSearch);
        pnlTop.Controls.Add(_chkTrashMode);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _categoriesGrid.CapturePosition();

            _allCategories = await _categoryQueryService.GetCategoryGridAsync(_chkTrashMode.Checked);
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
        _dgvCategories.BackgroundColor = _chkTrashMode.Checked ? Color.MistyRose : Color.WhiteSmoke;

        _btnDelete.Text = _chkTrashMode.Checked ? " Khôi phục" : " Xóa";
        _btnDelete.IconChar = _chkTrashMode.Checked ? IconChar.TrashRestore : IconChar.Trash;
        _btnDelete.BackColor = _chkTrashMode.Checked ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

        _btnAdd.Visible = !_chkTrashMode.Checked;
        _btnEdit.Visible = !_chkTrashMode.Checked;

        await LoadDataAsync();
    }

    private async void AddCategoryAsync(object? s, EventArgs e)
    {
        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void EditCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)_dgvCategories.SelectedRows[0].Cells["Id"].Value;
        var cat = await _categoryQueryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        form.LoadCategory(cat);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void DeleteCategoryAsync(object? s, EventArgs e)
    {
        if (_dgvCategories.SelectedRows.Count == 0) return;
        string name = _dgvCategories.SelectedRows[0].Cells["Name"].Value.ToString()!;
        int id = (int)_dgvCategories.SelectedRows[0].Cells["Id"].Value;

        if (_chkTrashMode.Checked)
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
        string keyword = _txtSearch.Text.Trim();

        _filteredCategories = string.IsNullOrEmpty(keyword)
            ? [.. _allCategories]
            : [.. _allCategories.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _categoriesGrid.Bind(_filteredCategories);
    }
}
