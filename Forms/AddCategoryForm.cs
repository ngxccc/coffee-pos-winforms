using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class AddCategoryForm : Form
{
    private readonly ICategoryService _categoryService;
    private TextBox txtName = null!;
    private Button btnSave = null!;

    public AddCategoryForm(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "THÊM DANH MỤC MỚI";
        Size = new Size(400, 210);
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
            Location = new Point(200, 120),
            Size = new Size(70, 32),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += async (s, e) => await SaveAsync();

        Button btnCancel = new()
        {
            Text = "HỦY",
            Location = new Point(280, 120),
            Size = new Size(70, 32),
            BackColor = Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => Close();

        Controls.AddRange([lblName, txtName, btnSave, btnCancel]);
    }

    private async Task SaveAsync()
    {
        btnSave.Enabled = false;
        try
        {
            var command = new UpsertCategoryDto(
                0,
                txtName.Text.Trim());

            await _categoryService.AddCategoryAsync(command);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi", this);
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error(ex.Message, owner: this);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }
}
