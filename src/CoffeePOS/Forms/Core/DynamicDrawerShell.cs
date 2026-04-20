using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms.Core;

public class DynamicDrawerShell<T> : UserControl
{
    private readonly IValidatableComponent<T> _contentComponent;

    public event Action<T>? OnSaved;
    public event Action? OnCancelled;

    public DynamicDrawerShell(string title, Control contentControl, int width = 450)
    {
        if (contentControl is not IValidatableComponent<T> validatable)
        {
            throw new ArgumentException("Content control MUST implement IValidatableComponent<T>");
        }
        _contentComponent = validatable;

        Width = width;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        BuildLayout(title, contentControl);
    }

    private void BuildLayout(string title, Control contentControl)
    {
        SuspendLayout();

        // PANEL HEADER
        AntdUI.Panel pnlHeader = new()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(20, 0, 20, 0),
        };
        AntdUI.Label lblTitle = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        pnlHeader.Controls.Add(lblTitle);

        // PANEL FOOTER
        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(10),
        };
        pnlFooter.SuspendLayout();

        AntdUI.Button btnSave = new()
        {
            Text = "XÁC NHẬN",
            Type = TTypeMini.Primary,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Right,
            Radius = 8,
        };
        btnSave.Click += BtnSave_Click;

        AntdUI.Button btnCancel = new()
        {
            Text = "HỦY",
            Type = TTypeMini.Default,
            Ghost = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Right,
            Radius = 8,
        };
        btnCancel.Click += BtnCancel_Click;

        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);

        // PANEL BODY
        AntdUI.Panel pnlBody = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
        };

        contentControl.Dock = DockStyle.Top;
        pnlBody.Controls.Add(contentControl);

        // DIVIDERS
        Divider dividerTop = new()
        {
            Dock = DockStyle.Top,
            Thickness = 1F,
            Height = 1,
            Margin = new Padding(0),
        };

        Divider dividerBottom = new()
        {
            Dock = DockStyle.Bottom,
            Thickness = 1F,
            Height = 1,
            Margin = new Padding(0),
        };

        Controls.Add(pnlBody);
        Controls.Add(dividerBottom);
        Controls.Add(pnlFooter);
        Controls.Add(dividerTop);
        Controls.Add(pnlHeader);

        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_contentComponent.ValidateInput())
        {
            var payload = _contentComponent.GetPayload();
            OnSaved?.Invoke(payload);

            FindForm()?.Close();
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        OnCancelled?.Invoke();
        FindForm()?.Close();
    }
}
