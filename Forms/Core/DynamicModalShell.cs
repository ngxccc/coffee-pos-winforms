namespace CoffeePOS.Forms.Core;

// PERF: Generic constraint <T> eliminates the need for expensive runtime Boxing/Unboxing and reflection casting.
public sealed class DynamicModalShell<T> : Form
{
    private readonly IValidatableComponent<T> _innerContent;
    private readonly Button _btnSave;
    private readonly Button _btnCancel;
    private readonly bool _showSaveButton;

    public DynamicModalShell(
        string title,
        Control contentModule,
        Size modalSize,
        bool showSaveButton = true,
        string saveButtonText = "LƯU",
        string cancelButtonText = "HUỶ")
    {
        Text = title;
        Size = modalSize;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        _showSaveButton = showSaveButton;

        // WHY: Hard constraint. If contentModule doesn't implement the exact T, it crashes here instantly rather than failing silently later.
        _innerContent = (contentModule as IValidatableComponent<T>)
            ?? throw new ArgumentException($"Ruột Form bị lỗi: Phải implement IValidatableComponent<{typeof(T).Name}>!");

        contentModule.Dock = DockStyle.Fill;
        Controls.Add(contentModule);

        var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };

        _btnSave = new Button { Text = saveButtonText, Width = 100, Height = 40, Location = new Point(modalSize.Width - 240, 10), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Visible = showSaveButton };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += HandleSaveAction;

        _btnCancel = new Button { Text = cancelButtonText, Width = 100, Height = 40, Location = new Point(modalSize.Width - 130, 10), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
        _btnCancel.FlatAppearance.BorderSize = 0;

        if (!showSaveButton)
        {
            _btnCancel.Location = new Point(modalSize.Width - 130, 10);
        }

        pnlActions.Controls.AddRange([_btnSave, _btnCancel]);
        Controls.Add(pnlActions);

        if (showSaveButton)
        {
            AcceptButton = _btnSave;
        }
        CancelButton = _btnCancel;
    }

    private void HandleSaveAction(object? sender, EventArgs e)
    {
        if (!_showSaveButton)
        {
            return;
        }

        if (_innerContent.ValidateInput())
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // WHY: Returns strongly-typed T. Compiler guarantees type safety for the caller. No more runtime (T) casting.
    public T ExtractData() => _innerContent.GetPayload();
}
