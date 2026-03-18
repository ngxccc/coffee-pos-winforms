using CoffeePOS.Forms;
using CoffeePOS.Models;
using CoffeePOS.Services;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Features.Admin;

public class UC_ManageCategories : UserControl
{
    private readonly ICategoryService _categoryService;
    private readonly IServiceProvider _serviceProvider;
    private DataGridView dgvCategories = null!;
    private TextBox txtSearch = null!;

    private List<Category> _allCategories = [];
    private List<Category> _filteredCategories = [];
    private string? _sortColumnName;
    private bool _sortAscending = true;

    public UC_ManageCategories(ICategoryService categoryService, IServiceProvider serviceProvider)
    {
        _categoryService = categoryService;
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
            Padding = new Padding(20)
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
        txtSearch.TextChanged += TxtSearch_TextChanged;

        FlowLayoutPanel flpBtns = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
        };

        var btnDel = CreateActionButton("Xóa", IconChar.Trash, Color.FromArgb(231, 76, 60), async (s, e) => await DeleteCategoryAsync());
        var btnEdit = CreateActionButton("Sửa", IconChar.Pen, Color.FromArgb(243, 156, 18), EditCategory);
        var btnAdd = CreateActionButton("Thêm Mới", IconChar.Plus, Color.FromArgb(46, 204, 113), AddCategory);

        flpBtns.Controls.AddRange([btnDel, btnEdit, btnAdd]);
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

        Controls.Add(dgvCategories);
        Controls.Add(pnlTop);
        pnlTop.Controls.Add(txtSearch);
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
            _allCategories = await _categoryService.GetAllCategoriesAsync();
            _filteredCategories = [.. _allCategories];

            RenderGrid(GetSortedData(_filteredCategories));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
        }
    }

    private void RenderGrid(List<Category> data)
    {
        dgvCategories.DataSource = null;
        dgvCategories.DataSource = data;

        if (dgvCategories.Columns["Id"] != null)
        {
            dgvCategories.Columns["Id"].HeaderText = "Mã";
            dgvCategories.Columns["Id"].FillWeight = 20;
        }

        if (dgvCategories.Columns["Name"] != null)
        {
            dgvCategories.Columns["Name"].HeaderText = "Tên Danh Mục";
            dgvCategories.Columns["Name"].FillWeight = 80;
        }

        if (dgvCategories.Columns["IsDeleted"] != null) dgvCategories.Columns["IsDeleted"].Visible = false;

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

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        string keyword = txtSearch.Text.ToLower().Trim();
        if (string.IsNullOrEmpty(keyword))
        {
            _filteredCategories = [.. _allCategories];
            RenderGrid(GetSortedData(_filteredCategories));
            return;
        }

        _filteredCategories = [.. _allCategories.Where(c => c.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))];
        RenderGrid(GetSortedData(_filteredCategories));
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

    private List<Category> GetSortedData(IEnumerable<Category> source)
    {
        if (string.IsNullOrEmpty(_sortColumnName)) return [.. source];

        return (_sortColumnName, _sortAscending) switch
        {
            (nameof(Category.Id), true) => [.. source.OrderBy(c => c.Id)],
            (nameof(Category.Id), false) => [.. source.OrderByDescending(c => c.Id)],
            (nameof(Category.Name), true) => [.. source.OrderBy(c => c.Name)],
            (nameof(Category.Name), false) => [.. source.OrderByDescending(c => c.Name)],
            _ => [.. source]
        };
    }

    private static bool IsSortableColumn(string columnName)
    {
        return columnName == nameof(Category.Id)
            || columnName == nameof(Category.Name);
    }

    private void AddCategory(object? s, EventArgs e)
    {
        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        if (form.ShowDialog() == DialogResult.OK) _ = LoadDataAsync();
    }

    private async void EditCategory(object? s, EventArgs e)
    {
        if (dgvCategories.SelectedRows.Count == 0) return;
        int id = (int)dgvCategories.SelectedRows[0].Cells["Id"].Value;
        var cat = await _categoryService.GetCategoryByIdAsync(id);
        if (cat == null) return;

        var form = _serviceProvider.GetRequiredService<CategoryDetailForm>();
        form.LoadCategory(cat);
        if (form.ShowDialog() == DialogResult.OK) _ = LoadDataAsync();
    }

    private async Task DeleteCategoryAsync()
    {
        if (dgvCategories.SelectedRows.Count == 0) return;
        string name = dgvCategories.SelectedRows[0].Cells["Name"].Value.ToString()!;
        int id = (int)dgvCategories.SelectedRows[0].Cells["Id"].Value;

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
