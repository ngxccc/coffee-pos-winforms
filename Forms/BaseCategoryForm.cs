using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public abstract class BaseCategoryForm : BaseCrudForm
{
    private int _categoryId;
    private readonly Button _btnSave;

    protected readonly ICategoryService CategoryService;
    protected readonly TextBox TxtName;

    protected BaseCategoryForm(ICategoryService categoryService, string title, string saveButtonText)
        : base(title, new Size(400, 210))
    {
        CategoryService = categoryService;

        TxtName = CreateTextBox(new Point(30, 60), 320, 12);
        _btnSave = CreatePrimaryButton(saveButtonText, new Point(170, 120), new Size(100, 32), Color.FromArgb(46, 204, 113));
        _btnSave.Click += async (_, _) => await SaveAsync();

        var btnCancel = CreateCancelButton(new Point(280, 120));
        btnCancel.Click += (_, _) => Close();

        Controls.AddRange(
        [
            CreateLabel("Tên danh mục:", new Point(30, 30)),
            TxtName,
            _btnSave,
            btnCancel
        ]);

        AcceptButton = _btnSave;
        CancelButton = btnCancel;
    }

    public void LoadCategory(CategoryDetailDto category)
    {
        _categoryId = category.Id;
        TxtName.Text = category.Name;
        Text = $"SỬA DANH MỤC: {category.Name}";
    }

    private async Task SaveAsync()
    {
        var command = new UpsertCategoryDto(_categoryId, TxtName.Text.Trim());
        await ExecuteSaveAsync(_btnSave, () => PersistAsync(command));
    }

    protected abstract Task PersistAsync(UpsertCategoryDto command);
}
