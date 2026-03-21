using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;

namespace CoffeePOS.Features.Admin.Controls;

public abstract class BaseAdminHeaderToolbar : UserControl
{
    protected readonly TextBox _txtSearch;
    protected CheckBox? _chkTrashMode;
    protected readonly IconButton _btnDelete;
    protected readonly IconButton _btnEdit;
    protected readonly IconButton _btnAdd;

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

    protected virtual (string addLabel, IconChar addIcon, string editLabel, IconChar editIcon, string deleteLabel, IconChar deleteIcon) GetButtonConfig() =>
        ("Thêm Mới", IconChar.Plus, "Sửa", IconChar.Pen, "Xóa", IconChar.Trash);

    protected BaseAdminHeaderToolbar()
    {
        Dock = DockStyle.Top;
        Height = 80;
        Padding = new Padding(0, 10, 0, 10);
        BackColor = Color.White;

        // Title
        Label lblTitle = new()
        {
            Text = Title,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Location = new Point(0, 20)
        };

        // Search Box
        _txtSearch = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 12),
            Location = new Point(250, 22),
            PlaceholderText = SearchPlaceholder
        };
        _txtSearch.OnDebouncedTextChanged(300, () => SearchChanged?.Invoke());

        Controls.Add(lblTitle);
        Controls.Add(_txtSearch);

        // Trash Mode Checkbox (optional)
        if (ShowTrashMode)
        {
            _chkTrashMode = new CheckBox
            {
                Text = "Xem Thùng Rác",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(570, 25),
                Cursor = Cursors.Hand
            };
            _chkTrashMode.CheckedChanged += OnTrashModeChanged;
            Controls.Add(_chkTrashMode);
        }

        // Action Buttons
        FlowLayoutPanel flpButtons = new()
        {
            Dock = DockStyle.Right,
            Width = 400,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        var (addLabel, addIcon, editLabel, editIcon, deleteLabel, deleteIcon) = GetButtonConfig();

        _btnDelete = UIHelper.CreateActionButton(deleteLabel, deleteIcon, Color.FromArgb(231, 76, 60), (_, _) => DeleteClicked?.Invoke(this, EventArgs.Empty));
        _btnEdit = UIHelper.CreateActionButton(editLabel, editIcon, Color.FromArgb(243, 156, 18), (_, _) => EditClicked?.Invoke(this, EventArgs.Empty));
        _btnAdd = UIHelper.CreateActionButton(addLabel, addIcon, Color.FromArgb(46, 204, 113), (_, _) => AddClicked?.Invoke(this, EventArgs.Empty));

        flpButtons.Controls.AddRange([_btnDelete, _btnEdit, _btnAdd]);
        Controls.Add(flpButtons);
    }

    protected virtual void OnTrashModeChanged(object? sender, EventArgs e)
    {
        if (_chkTrashMode == null) return;

        _btnDelete.Text = _chkTrashMode.Checked ? " Khôi phục" : " Xóa";
        _btnDelete.IconChar = _chkTrashMode.Checked ? IconChar.TrashRestore : IconChar.Trash;
        _btnDelete.BackColor = _chkTrashMode.Checked ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

        _btnAdd.Visible = !_chkTrashMode.Checked;
        _btnEdit.Visible = !_chkTrashMode.Checked;

        TrashModeChanged?.Invoke(this, e);
    }
}
