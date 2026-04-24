using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public abstract class BaseAdminHeaderToolbar : UserControl
{
    protected readonly AntdUI.Input _txtSearch;
    protected AntdUI.Checkbox? _chkTrashMode;
    protected readonly AntdUI.Button _btnDelete;
    protected readonly AntdUI.Button _btnEdit;
    protected readonly AntdUI.Button _btnAdd;

    public event EventHandler? AddClicked;
    public event EventHandler? EditClicked;
    public event EventHandler? DeleteClicked;
    public event Action? SearchChanged;
    public event EventHandler? TrashModeChanged;

    public virtual string SearchText => _txtSearch.Text;
    public virtual bool IsTrashMode => _chkTrashMode?.Checked ?? false;

    protected abstract string Title { get; }

    protected abstract string SearchPlaceholder { get; }

    protected abstract bool ShowTrashMode { get; }

    protected virtual (string addLabel, string editLabel, string deleteLabel) GetButtonConfig() =>
        ("Thêm", "Sửa", "Xóa");

    protected BaseAdminHeaderToolbar()
    {
        Dock = DockStyle.Top;
        Height = UiTheme.ToolbarHeight;
        Padding = new Padding(UiTheme.PagePadding, UiTheme.BlockGap, UiTheme.PagePadding, UiTheme.BlockGap);
        BackColor = UiTheme.Surface;

        var lblTitle = new AntdUI.Label
        {
            Text = Title,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            AutoSize = true,
            Location = new Point(0, 20)
        };

        _txtSearch = new AntdUI.Input
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = SearchPlaceholder,
            AllowClear = true
        };
        _txtSearch.OnDebouncedTextChanged(300, () => SearchChanged?.Invoke());

        Controls.Add(lblTitle);
        Controls.Add(_txtSearch);

        if (ShowTrashMode)
        {
            _chkTrashMode = new AntdUI.Checkbox
            {
                Text = "Xem Thùng Rác",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(570, 25),
                Cursor = Cursors.Hand
            };
            _chkTrashMode.CheckedChanged += (_, _) => OnTrashModeChanged(this, EventArgs.Empty);
            Controls.Add(_chkTrashMode);
        }

        var flpButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var (addLabel, editLabel, deleteLabel) = GetButtonConfig();

        _btnDelete = CreateActionButton(deleteLabel, UiTheme.DeleteButtonType, (_, _) => DeleteClicked?.Invoke(this, EventArgs.Empty));
        _btnEdit = CreateActionButton(editLabel, UiTheme.EditButtonType, (_, _) => EditClicked?.Invoke(this, EventArgs.Empty));
        _btnAdd = CreateActionButton(addLabel, UiTheme.AddButtonType, (_, _) => AddClicked?.Invoke(this, EventArgs.Empty));

        flpButtons.Controls.AddRange([_btnDelete, _btnEdit, _btnAdd]);
        Controls.Add(flpButtons);
    }

    private static AntdUI.Button CreateActionButton(string text, AntdUI.TTypeMini type, EventHandler click)
    {
        var btn = new AntdUI.Button
        {
            Text = text,
            Type = type,
            Size = new Size(120, 38),
            Cursor = Cursors.Hand,
            Margin = new Padding(5, 0, 0, 0)
        };
        btn.Click += click;
        return btn;
    }

    protected virtual void OnTrashModeChanged(object? sender, EventArgs e)
    {
        if (_chkTrashMode == null) return;

        _btnDelete.Text = _chkTrashMode.Checked ? "Khôi phục" : "Xóa";
        _btnDelete.Type = _chkTrashMode.Checked ? UiTheme.AddButtonType : UiTheme.DeleteButtonType;

        _btnAdd.Visible = !_chkTrashMode.Checked;
        _btnEdit.Visible = !_chkTrashMode.Checked;

        TrashModeChanged?.Invoke(this, e);
    }
}
