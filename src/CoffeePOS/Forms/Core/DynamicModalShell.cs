namespace CoffeePOS.Forms.Core;

using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

public sealed class DynamicModalShell<T> : Window
{
    private readonly IValidatableComponent<T> _innerContent;
    private readonly Control _contentControl;
    private readonly AntdUI.Button _btnSave;
    private readonly AntdUI.Button _btnCancel;
    private readonly bool _showSaveButton;

    public DynamicModalShell(
        string title,
        Control contentModule,
        Size modalSize,
        bool showSaveButton = true,
        string saveButtonText = "LƯU",
        string cancelButtonText = "HUỶ")
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Tiêu đề không được để trống.", nameof(title));
        }

        ArgumentNullException.ThrowIfNull(contentModule);

        if (contentModule is not IValidatableComponent<T> validatableContent)
        {
            throw new ArgumentException($"Nội dung modal phải implement IValidatableComponent<{typeof(T).Name}>.", nameof(contentModule));
        }

        Text = title;
        Size = modalSize;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        Resizable = false;
        ShowInTaskbar = false;
        BackColor = UiTheme.Surface;
        _showSaveButton = showSaveButton;
        _innerContent = validatableContent;
        _contentControl = contentModule;

        AntdUI.Panel pnlActions = new()
        {
            Dock = DockStyle.Bottom,
            Height = 50,
        };

        Divider divider1 = new()
        {
            Dock = DockStyle.Bottom,
            Thickness = 1F,
            Size = new Size(modalSize.Width, 1),
            Margin = new Padding(0)
        };

        // WHY: Use StackPanel to mock Web Flexbox behavior. Prevents coordinates from breaking when DPI scale > 100%.
        StackPanel stackActions = new()
        {
            Dock = DockStyle.Right,
            Width = modalSize.Width,
            Margin = new Padding(10),
            Gap = 10,
            RightToLeft = RightToLeft.Yes
        };

        _btnSave = new()
        {
            Text = saveButtonText,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Type = TTypeMini.Primary,
            Visible = showSaveButton,
            Shape = TShape.Round,
            Cursor = Cursors.Hand,
        };
        _btnSave.Click += HandleSaveAction;

        _btnCancel = new()
        {
            Text = cancelButtonText,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Type = TTypeMini.Default,
            DialogResult = DialogResult.Cancel,
            Shape = TShape.Round,
            Cursor = Cursors.Hand,
        };

        CancelButton = _btnCancel;
        stackActions.Controls.Add(_btnCancel);
        if (showSaveButton)
        {
            AcceptButton = _btnSave;
            stackActions.Controls.Add(_btnSave);
        }

        pnlActions.Controls.Add(stackActions);

        _contentControl.Dock = DockStyle.Fill;
        Controls.Add(_contentControl);
        Controls.Add(divider1);
        Controls.Add(pnlActions);
    }

    private void HandleSaveAction(object? sender, EventArgs e)
    {
        if (!_showSaveButton) return;

        if (!_innerContent.ValidateInput()) return;

        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _btnSave.Click -= HandleSaveAction;
        }

        base.Dispose(disposing);
    }

    public T ExtractData() => _innerContent.GetPayload();
}
