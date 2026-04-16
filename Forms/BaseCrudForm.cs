using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public abstract class BaseCrudForm : AntdUI.Window
{
    protected BaseCrudForm(string title, Size size)
    {
        Text = title;
        Size = size;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
    }

    protected static Label CreateLabel(string text, Point location)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = location
        };

    protected static TextBox CreateTextBox(Point location, int width, float fontSize = 11)
        => new()
        {
            Location = location,
            Width = width,
            Font = new Font("Segoe UI", fontSize)
        };

    protected static TextBox CreatePasswordBox(Point location, int width, float fontSize = 11)
        => new()
        {
            Location = location,
            Width = width,
            Font = new Font("Segoe UI", fontSize),
            PasswordChar = '\u25cf'
        };

    protected static Button CreatePrimaryButton(string text, Point location, Size size, Color backColor)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = size,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    protected static Button CreateCancelButton(Point location)
    {
        var button = new Button
        {
            Text = "HỦY",
            Location = location,
            Size = new Size(70, 32),
            BackColor = Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    protected async Task ExecuteSaveAsync(Button saveButton, Func<Task> saveAction, string? successMessage = null)
    {
        saveButton.Enabled = false;
        try
        {
            await saveAction();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                MessageBoxHelper.Info(successMessage, owner: this);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi nhập liệu", this);
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
            saveButton.Enabled = true;
        }
    }
}
