using CoffeePOS.Forms;
using CoffeePOS.Services;
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
    private DataGridView dgvCategories = null!;
    private TextBox txtSearch = null!;
    private IconButton btnDelete = null!;
    private IconButton btnEdit = null!;
    private IconButton btnAdd = null!;
    private CheckBox chkTrashMode = null!;

    private List<CategoryGridDto> _allCategories = [];
    private List<CategoryGridDto> _filteredCategories = [];
    private string? _sortColumnName;
    private bool _sortAscending = true;

    private int _savedScrollPosition = -1;
    private int _savedSelectedRowId = -1;

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
        Dock = DockStyle.Fill; BackColor = Color.White;

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

        txtSearch = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = "Nhập tên danh mục để tìm..."
        };
        txtSearch.OnDebouncedTextChanged(300, ApplyFilterAndSort);

        chkTrashMode = new CheckBox
        {
            Text = "Xem Thùng Rác",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(570, 25),
            Cursor = Cursors.Hand
        };
        chkTrashMode.CheckedChanged += ChkTrashMode_CheckedChanged;

        FlowLayoutPanel flpBtns = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        btnDelete = UIHelper.CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), DeleteCategoryAsync);
        btnEdit = UIHelper.CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditCategoryAsync);
        btnAdd = UIHelper.CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddCategoryAsync);

        flpBtns.Controls.AddRange([btnDelete, btnEdit, btnAdd]);
        pnlTop.Controls.AddRange([lblTitle, flpBtns]);

        dgvCategories = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        dgvCategories.ApplyStandardAdminStyle();
        dgvCategories.ColumnHeaderMouseClick += DgvCategories_ColumnHeaderMouseClick;
        dgvCategories.CellDoubleClick += EditCategoryAsync;

        Controls.Add(dgvCategories);
        Controls.Add(pnlTop);
        pnlTop.Controls.Add(txtSearch);
        pnlTop.Controls.Add(chkTrashMode);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            dgvCategories.SavePosition(ref _savedScrollPosition, ref _savedSelectedRowId);

            _allCategories = await _categoryQueryService.GetCategoryGridAsync(chkTrashMode.Checked);
            ApplyFilterAndSort();

            dgvCategories.RestorePosition(_savedScrollPosition, _savedSelectedRowId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
        }
    }

    private void DgvCategories_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        dgvCategories.ToggleSortState(e.ColumnIndex, ref _sortColumnName, ref _sortAscending);

        ApplyFilterAndSort();
    }

    private async void ChkTrashMode_CheckedChanged(object? sender, EventArgs e)
    {
        dgvCategories.BackgroundColor = chkTrashMode.Checked ? Color.MistyRose : Color.WhiteSmoke;

        btnDelete.Text = chkTrashMode.Checked ? " Khôi phục" : " Xóa";
        btnDelete.IconChar = chkTrashMode.Checked ? IconChar.TrashRestore : IconChar.Trash;
        btnDelete.BackColor = chkTrashMode.Checked ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

        btnAdd.Visible = !chkTrashMode.Checked;
        btnEdit.Visible = !chkTrashMode.Checked;

        await LoadDataAsync();
    }

    private async void AddCategoryAsync(object? s, EventArgs e)
    {
        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void EditCategoryAsync(object? s, EventArgs e)
    {
        if (dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)dgvCategories.SelectedRows[0].Cells["Id"].Value;
        var cat = await _categoryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        form.LoadCategory(cat);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void DeleteCategoryAsync(object? s, EventArgs e)
    {
        if (dgvCategories.SelectedRows.Count == 0) return;
        string name = dgvCategories.SelectedRows[0].Cells["Name"].Value.ToString()!;
        int id = (int)dgvCategories.SelectedRows[0].Cells["Id"].Value;

        if (chkTrashMode.Checked)
        {
            if (MessageBox.Show($"Khôi phục danh mục '{name}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    await _categoryService.RestoreCategoryAsync(id);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        else
        {
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa danh mục '{name}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    await _categoryService.DeleteCategoryAsync(id);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = txtSearch.Text.Trim();

        _filteredCategories = string.IsNullOrEmpty(keyword)
            ? [.. _allCategories]
            : [.. _allCategories.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        // Sắp xếp dữ liệu bằng hàm Extension Reflection siêu việt (Sort)
        var finalData = _filteredCategories.DynamicSort(_sortColumnName, _sortAscending).ToList();

        // Đổ vào Grid. (DataGrid sẽ tự đọc [DisplayName] và [Browsable] để tự vẽ cột!)
        dgvCategories.DataSource = finalData;

        dgvCategories.UpdateSortGlyphs(_sortColumnName, _sortAscending);
    }
}
