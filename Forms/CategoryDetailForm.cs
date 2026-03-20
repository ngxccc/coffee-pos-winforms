using CoffeePOS.Models;
using CoffeePOS.Services.Contracts.Commands;

namespace CoffeePOS.Forms;

public partial class CategoryDetailForm : Form
{
    private readonly ICategoryService _categoryService;
    private int _categoryId = 0;
    private TextBox txtName = null!;
    private Button btnSave = null!;

    public CategoryDetailForm(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        InitializeUI();
    }

    public void LoadCategory(Category category)
    {
        _categoryId = category.Id;
        txtName.Text = category.Name;
        Text = "SỬA DANH MỤC";
        btnSave.Text = "CẬP NHẬT";
    }

    private void InitializeUI()
    {
        Text = "THÊM DANH MỤC MỚI";
        Size = new Size(400, 250);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        BackColor = Color.White;

        Label lblName = new()
        {
            Text = "Tên danh mục:",
            Location = new Point(30, 30),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        txtName = new TextBox
        {
            Location = new Point(30, 60),
            Width = 320,
            Font = new Font("Segoe UI", 12)
        };

        btnSave = new Button
        {
            Text = "LƯU",
            Location = new Point(140, 120),
            Size = new Size(100, 40),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSave.Click += async (s, e) => await SaveAsync();

        Button btnCancel = new()
        {
            Text = "HỦY",
            Location = new Point(250, 120),
            Size = new Size(100, 40),
            BackColor = Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancel.Click += (s, e) => Close();

        Controls.AddRange([lblName, txtName, btnSave, btnCancel]);
    }

    private async Task SaveAsync()
    {
        btnSave.Enabled = false;
        try
        {
            var cat = new Category
            {
                Id = _categoryId,
                Name = txtName.Text.Trim()
            };

            if (_categoryId == 0) await _categoryService.AddCategoryAsync(cat);
            else await _categoryService.UpdateCategoryAsync(cat);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
