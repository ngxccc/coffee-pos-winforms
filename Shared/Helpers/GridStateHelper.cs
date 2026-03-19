namespace CoffeePOS.Shared.Helpers;

public static class GridStateHelper
{
    public static void SavePosition(this DataGridView dgv, ref int scrollPos, ref int selectedId, string idColName = "Id")
    {
        if (dgv.Rows.Count == 0) return;
        scrollPos = dgv.FirstDisplayedScrollingRowIndex;
        selectedId = dgv.SelectedRows.Count > 0 ? (int)dgv.SelectedRows[0].Cells[idColName].Value : -1;
    }

    public static void RestorePosition(this DataGridView dgv, int scrollPos, int selectedId, string idColName = "Id")
    {
        if (dgv.Rows.Count == 0) return;
        try
        {
            dgv.ClearSelection();
            if (selectedId > 0)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if ((int)row.Cells[idColName].Value == selectedId)
                    {
                        row.Selected = true; break;
                    }
                }
            }
            if (scrollPos >= 0)
            {
                dgv.FirstDisplayedScrollingRowIndex = Math.Min(scrollPos, dgv.Rows.Count - 1);
            }
        }
        catch { }
    }

    public static void ToggleSortState(this DataGridView dgv, int columnIndex, ref string? sortColumnName, ref bool sortAscending)
    {
        if (columnIndex < 0 || columnIndex >= dgv.Columns.Count) return;

        string columnName = dgv.Columns[columnIndex].DataPropertyName;
        if (string.IsNullOrWhiteSpace(columnName)) return;

        if (string.Equals(sortColumnName, columnName, StringComparison.OrdinalIgnoreCase))
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortColumnName = columnName;
            sortAscending = true;
        }
    }

    public static void UpdateSortGlyphs(this DataGridView dgv, string? sortColumnName, bool sortAscending)
    {
        foreach (DataGridViewColumn col in dgv.Columns)
        {
            col.HeaderCell.SortGlyphDirection = SortOrder.None;
        }

        if (!string.IsNullOrEmpty(sortColumnName) && dgv.Columns.Contains(sortColumnName))
        {
            dgv.Columns[sortColumnName].HeaderCell.SortGlyphDirection =
                sortAscending ? SortOrder.Ascending : SortOrder.Descending;
        }
    }
}
