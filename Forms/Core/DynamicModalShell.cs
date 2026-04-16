namespace CoffeePOS.Forms.Core;

using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;

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
        BackColor = Color.White;
        _showSaveButton = showSaveButton;
        _innerContent = validatableContent;
        _contentControl = contentModule;

        _contentControl.Dock = DockStyle.Fill;
        Controls.Add(_contentControl);

        var pnlActions = new AntdUI.Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Back = Color.WhiteSmoke
        };

        // WHY: Use StackPanel to mock Web Flexbox behavior. Prevents coordinates from breaking when DPI scale > 100%.
        var stackActions = new StackPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            Margin = new Padding(10),
            Gap = 10
        };

        _btnSave = new AntdUI.Button
        {
            Text = saveButtonText,
            Size = new Size(100, 40),
            Type = TTypeMini.Primary,
            Visible = showSaveButton
        };
        _btnSave.Click += HandleSaveAction;

        _btnCancel = new AntdUI.Button
        {
            Text = cancelButtonText,
            Size = new Size(100, 40),
            Type = TTypeMini.Default,
            DialogResult = DialogResult.Cancel
        };

        if (showSaveButton)
        {
            AcceptButton = _btnSave;
        }

        CancelButton = _btnCancel;

        stackActions.Controls.Add(_btnCancel);
        if (showSaveButton)
        {
            stackActions.Controls.Add(_btnSave);
        }

        pnlActions.Controls.Add(stackActions);
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
