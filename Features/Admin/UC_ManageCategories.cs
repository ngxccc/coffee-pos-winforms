using CoffeePOS.Forms;
using CoffeePOS.Services;
using CoffeePOS.Shared.Dtos;
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
        txtSearch.TextChanged += (s, e) => ApplyFilterAndSort();

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

        btnDelete = CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), async (s, e) => await DeleteCategoryAsync());
        btnEdit = CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditCategory);
        btnAdd = CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddCategory);

        flpBtns.Controls.AddRange([btnDelete, btnEdit, btnAdd]);
        pnlTop.Controls.AddRange([lblTitle, flpBtns]);

        dgvCategories = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.WhiteSmoke,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            RowTemplate = { Height = 40 },
            Font = new Font("Segoe UI", 11),
            EnableHeadersVisualStyles = false,

            AllowUserToResizeColumns = false,
            AllowUserToResizeRows = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
        };
        dgvCategories.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 30, 68);
        dgvCategories.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvCategories.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        dgvCategories.ColumnHeadersHeight = 40;
        dgvCategories.ColumnHeaderMouseClick += DgvCategories_ColumnHeaderMouseClick;
        dgvCategories.CellDoubleClick += EditCategory;

        Controls.Add(dgvCategories);
        Controls.Add(pnlTop);
        pnlTop.Controls.Add(txtSearch);
        pnlTop.Controls.Add(chkTrashMode);
    }

    private static IconButton CreateActionButton(string text, IconChar icon, Color backColor, EventHandler clickEvent)
    {
        IconButton btn = new()
        {
            Text = " " + text,
            IconChar = icon,
            IconSize = 24,
            IconColor = Color.White,
            ForeColor = Color.White,
            BackColor = backColor,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 40),
            FlatStyle = FlatStyle.Flat,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand,
            Margin = new Padding(5, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += clickEvent;
        return btn;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _allCategories = await _categoryQueryService.GetCategoryGridAsync(chkTrashMode.Checked);

            ApplyFilterAndSort();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
        }
    }

    private void RenderGrid(List<CategoryGridDto> data)
    {
        dgvCategories.DataSource = null;
        dgvCategories.DataSource = data;

        if (dgvCategories.Columns[nameof(CategoryGridDto.Id)] != null)
        {
            dgvCategories.Columns[nameof(CategoryGridDto.Id)].HeaderText = "Mã";
            dgvCategories.Columns[nameof(CategoryGridDto.Id)].FillWeight = 20;
        }

        if (dgvCategories.Columns[nameof(CategoryGridDto.Name)] != null)
        {
            dgvCategories.Columns[nameof(CategoryGridDto.Name)].HeaderText = "Tên Danh Mục";
            dgvCategories.Columns[nameof(CategoryGridDto.Name)].FillWeight = 80;
        }

        foreach (DataGridViewColumn col in dgvCategories.Columns)
        {
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            col.HeaderCell.SortGlyphDirection = SortOrder.None;
        }

        if (!string.IsNullOrEmpty(_sortColumnName) && dgvCategories.Columns.Contains(_sortColumnName))
        {
            dgvCategories.Columns[_sortColumnName].HeaderCell.SortGlyphDirection =
                _sortAscending ? SortOrder.Ascending : SortOrder.Descending;
        }
    }

    private void DgvCategories_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvCategories.Columns.Count) return;

        var column = dgvCategories.Columns[e.ColumnIndex];
        string columnName = column.DataPropertyName;
        if (string.IsNullOrWhiteSpace(columnName)) columnName = column.Name;

        if (!IsSortableColumn(columnName)) return;

        if (string.Equals(_sortColumnName, columnName, StringComparison.Ordinal))
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumnName = columnName;
            _sortAscending = true;
        }

        RenderGrid(GetSortedData(_filteredCategories));
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

    private List<CategoryGridDto> GetSortedData(IEnumerable<CategoryGridDto> source)
    {
        if (string.IsNullOrEmpty(_sortColumnName)) return [.. source];

        return (_sortColumnName, _sortAscending) switch
        {
            (nameof(CategoryGridDto.Id), true) => [.. source.OrderBy(c => c.Id)],
            (nameof(CategoryGridDto.Id), false) => [.. source.OrderByDescending(c => c.Id)],
            (nameof(CategoryGridDto.Name), true) => [.. source.OrderBy(c => c.Name)],
            (nameof(CategoryGridDto.Name), false) => [.. source.OrderByDescending(c => c.Name)],
            _ => [.. source]
        };
    }

    private static bool IsSortableColumn(string columnName)
    {
        return columnName == nameof(CategoryGridDto.Id)
            || columnName == nameof(CategoryGridDto.Name);
    }

    private async void AddCategory(object? s, EventArgs e)
    {
        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async void EditCategory(object? s, EventArgs e)
    {
        if (dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)dgvCategories.SelectedRows[0].Cells["Id"].Value;
        var cat = await _categoryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        form.LoadCategory(cat);
        if (form.ShowDialog() == DialogResult.OK) await LoadDataAsync();
    }

    private async Task DeleteCategoryAsync()
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

        if (string.IsNullOrEmpty(keyword))
        {
            _filteredCategories = [.. _allCategories];
        }
        else
        {
            // So sánh chuẩn .NET 8 (Không dùng ToLower)
            _filteredCategories = [.. _allCategories.Where(c =>
                c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];
        }

        // Tự động ăn theo _sortColumnName và _sortAscending hiện tại
        RenderGrid(GetSortedData(_filteredCategories));
    }
}
