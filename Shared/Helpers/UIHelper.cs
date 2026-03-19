using FontAwesome.Sharp;

namespace CoffeePOS.Shared.Helpers;

public static class UIHelper
{
    public static IconButton CreateActionButton(string text, IconChar icon, Color backColor, EventHandler clickEvent)
    {
        var btn = new IconButton
        {
            Text = " " + text,
            IconChar = icon,
            IconSize = 24,
            IconColor = Color.White,
            ForeColor = Color.White,
            BackColor = backColor,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 40),
            FlatStyle = FlatStyle.Flat,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Cursor = Cursors.Hand,
            Margin = new Padding(5, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += clickEvent;
        return btn;
    }

    public static void ApplyStandardAdminStyle(this DataGridView dgv)
    {
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.BackgroundColor = Color.WhiteSmoke;
        dgv.BorderStyle = BorderStyle.None;
        dgv.RowHeadersVisible = false;
        dgv.RowTemplate.Height = 40;
        dgv.Font = new Font("Segoe UI", 11);
        dgv.EnableHeadersVisualStyles = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.AllowUserToResizeRows = false;

        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 30, 68);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 40;
    }
}
