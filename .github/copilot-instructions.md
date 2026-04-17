# CoffeePOS AntdUI Migration - Knowledge Base

## Project Overview
- **Project Name**: CoffeePOS (Coffee Point of Sale System)
- **Technology Stack**: .NET 8.0 WinForms + AntdUI v2.3.9
- **Build Command**: `dotnet build CoffeePOS.sln -v minimal`

---

## Key Control Properties & Requirements

### AntdUI.Input - REQUIRED Height Property
- **MUST have**: `Height` (32-40px typical) during initialization
- Properties: `Text`, `Placeholder`, `ReadOnly`, `ForeColor`, `BackColor`, `Width`, `Location`
- Use helper: `CreateInput(Point location, int width, float fontSize = 11)` includes Height automatically
- Event: `TextChanged` (subscribe in OnLoad, not constructor)

### AntdUI.Label - NO Auto-Size for Width
- **MUST set**: `AutoSize = true` for text width, OR explicit `Width` for fixed-width labels
- Properties: `Text`, `Font`, `ForeColor`, `AutoSize`, `Dock`, `Location`
- Font sizing: Use `new Font("Segoe UI", 10)` for standard labels, `FontStyle.Bold` for section headers
- Do NOT set both `AutoSize = true` and explicit `Width` (width will be ignored)

### AntdUI.Checkbox
- Properties: `Checked`, `CheckedChanged` event, `Enabled` for disable state
- Manual grouping required for RadioButtons (no auto-group like WinForms)

### Theme Application
- Set `BackColor = UiTheme.Surface` on panels/containers for consistency
- Use `UiTheme.TextSecondary` for helper/muted text
- Never hardcode colors; always use UiTheme constants

---

## Common Errors & Solutions

### Error #1: AntdUI.Button vs WinForms Button Confusion
**Symptom**: Compiler error "The name 'Button' is ambiguous between..."

**Root Cause**: Both `System.Windows.Forms.Button` and `AntdUI.Button` exist in scope

**Solution**:
```csharp
// ✅ CORRECT: Explicit namespace
AntdUI.Button btn = new AntdUI.Button();

// ❌ WRONG: May resolve to WinForms Button
Button btn = new Button();
```

**Lesson**: Always use fully-qualified `AntdUI.Button` in new code, or add `using AntdUI;` at top with explicit `new AntdUI.Button()`

---

### Error #1b: AntdUI.Input Missing Height - Control Not Visible
**Symptom**: AntdUI.Input appears invisible or extremely small on form

**Root Cause**:
- `Height` property not set during initialization
- AntdUI.Input requires explicit `Height` value (unlike WinForms TextBox which auto-sizes)

**Solution**:
```csharp
// ❌ WRONG: No Height specified
AntdUI.Input input = new AntdUI.Input()
{
    Text = "Hello",
    Width = 200,
    Location = new Point(10, 10)
};

// ✅ CORRECT: Height is mandatory
AntdUI.Input input = new AntdUI.Input()
{
    Text = "Hello",
    Width = 200,
    Height = 32,  // REQUIRED! Default should be 32-40px
    Location = new Point(10, 10)
};

// Or in helper method:
protected static AntdUI.Input CreateInput(Point location, int width, float fontSize = 11)
    => new()
    {
        Location = location,
        Width = width,
        Height = 32,  // Always include Height
        Font = new Font("Segoe UI", fontSize)
    };
```

**Lesson**: AntdUI.Input MUST have explicit Height; always include it in initialization or helper methods

---

### Error #2: AntdUI.Input TextChanged Event Not Firing
**Symptom**: TextChanged event handler not called when user types

**Root Cause**:
- Event subscription happens before control is added to form
- Or control is disabled/read-only unintentionally

**Solution**:
```csharp
// ✅ CORRECT: Subscribe after form layout complete
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    searchInput.TextChanged += SearchInput_TextChanged;
}

// Alternative: Use TextBoxExtensions debounce helper
searchInput.AddDebounceTextChanged(300, (s, e) =>
{
    PerformSearch(searchInput.Text);
});
```

**Lesson**: For AntdUI.Input, subscribe to TextChanged in OnLoad or after form shown, not in constructor

---

### Error #3: RadioButton/Checkbox Manual Grouping
**Symptom**: Multiple RadioButtons can be checked simultaneously (breaks radio button semantics)

**Root Cause**: RadioButtons created in `FlowLayoutPanel` don't auto-group

**Solution**:
```csharp
// ✅ CORRECT: Manually uncheck siblings
private void SizeRadio_CheckedChanged(object sender, EventArgs e)
{
    if (((AntdUI.Radio)sender).Checked)
    {
        foreach (var radio in sizeRadios)
        {
            if (radio != sender)
                radio.Checked = false;
        }
    }
}

// Or: Create explicit RadioGroup-like logic
private AntdUI.Radio _selectedRadio = null;
private void Radio_CheckedChanged(object sender, EventArgs e)
{
    if (((AntdUI.Radio)sender).Checked)
    {
        if (_selectedRadio != null && _selectedRadio != sender)
            _selectedRadio.Checked = false;
        _selectedRadio = (AntdUI.Radio)sender;
    }
}
```

**Lesson**: AntdUI.Radio doesn't auto-group like WinForms RadioButton; implement radio group logic manually

---

### Error #4: Button Style Not Applied / Buttons Look Wrong
**Symptom**: Button appears gray/default even after setting Type property

**Root Cause**:
- ButtonStyle applied before button sized/rendered
- Or UiTheme ButtonStyle not properly structured

**Solution**:
```csharp
// ✅ CORRECT: Set properties in order
AntdUI.Button btn = new AntdUI.Button()
{
    Text = "Delete",
    Size = new Size(100, 40),
    Type = AntdUI.ButtonType.Primary,  // Apply type after size/text
};

// Then apply theme
btn.BackColor = UiTheme.BrandError;
btn.ForeColor = Color.White;
btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

// ✅ Or use UiTheme ButtonStyle dictionary
var style = UiTheme.ButtonStyles["Error"];
btn.BackColor = style.BackColor;
btn.ForeColor = style.ForeColor;
btn.Font = style.Font;
```

**Lesson**: Apply styling after control is fully initialized; verify UiTheme constants exist and are not null

---

### Error #5: NumericUpDown vs InputNumber Value Type Mismatch
**Symptom**: Compiler error or runtime value truncation

**Root Cause**:
- WinForms NumericUpDown uses int/int; AntdUI.InputNumber uses decimal
- Calculation results lose precision

**Solution**:
```csharp
// ❌ WRONG: Treating InputNumber.Value as int
int quantity = (int)quantityInput.Value;  // May lose decimal places

// ✅ CORRECT: Keep as decimal until final calculation
decimal quantity = quantityInput.Value;
decimal totalPrice = price * quantity;  // Decimal arithmetic

// Or convert explicitly when needed
int quantityInt = Convert.ToInt32(quantityInput.Value);  // Explicit conversion
```

**Lesson**: AntdUI.InputNumber is decimal-based for financial accuracy; only convert to int when absolutely necessary

---

### Error #6: Panel Host BackColor Not Applied to Children
**Symptom**: DataGridView shows with white background inside gray panel

**Root Cause**: Child controls don't inherit panel background color automatically

**Solution**:
```csharp
// ✅ CORRECT: Set child control BackColor explicitly
AntdUI.Panel hostPanel = new AntdUI.Panel()
{
    Dock = DockStyle.Fill,
    BackColor = UiTheme.Surface,  // Gray surface
};

DataGridView dgv = new DataGridView()
{
    Dock = DockStyle.Fill,
    BackColor = UiTheme.SurfaceAlt,  // Explicitly set to white
};

hostPanel.Controls.Add(dgv);
```

**Lesson**: AntdUI.Panel doesn't cascade BackColor to children; set child BackColor independently for intended visual hierarchy

---

### Error #7: ImageHelper Async Loading Causes Cross-Thread Exception
**Symptom**: "Cross-thread operation not valid" exception when setting PictureBox.Image from async task

**Root Cause**: Updating UI control from background thread directly

**Solution**:
```csharp
// ❌ WRONG: Direct update in Task
Task.Run(() =>
{
    pictureBox.Image = LoadedBitmap;  // Cross-thread exception!
});

// ✅ CORRECT: Use Invoke to marshal to UI thread
Task.Run(() =>
{
    Bitmap loadedImage = GetImageFromDatabase(imageId);
    pictureBox.Invoke((Action)(() =>
    {
        pictureBox.Image = loadedImage;
    }));
});

// Or: Use async/await for cleaner syntax
private async Task LoadImageAsync(PictureBox pb, string id)
{
    var image = await Task.Run(() => GetImageFromDatabase(id));
    pb.Image = image;  // Automatically on UI thread
}
```

**Lesson**: Always use `Invoke()` or async/await pattern for cross-thread UI updates; never update controls directly from background thread

---

## Best Practices Established

### 1. Theme Consistency
- **Always use UiTheme constants**: Never hardcode colors (Color.White, Color.Red, etc.)
- **Single token source**: Update color in UiTheme.cs; all forms automatically reflect change
- **Token naming**: Use semantic names (BrandError, TextSecondary) not hex values

### 2. Control Migration Pattern
When replacing WinForms with AntdUI:
1. Identify all properties used (Text, Checked, Value, events, etc.)
2. Find AntdUI equivalent control
3. Map properties using AntdUI documentation
4. Migrate event handlers (usually identical names)
5. Update styling to use UiTheme constants
6. Test on-screen rendering

### 3. Layout & Spacing
- Use `FlowLayoutPanel` for dynamic control groups (toolbars, card collections)
- Use `AntdUI.Panel` for fixed-layout sections with gray backgrounds
- Apply `Padding = new Padding(UiTheme.PagePadding)` to top-level panels
- Use `BlockGap = 10` between logical sections

### 4. Button Styling Strategy
- **Primary actions**: `Type = AntdUI.ButtonType.Primary` + `UiTheme.BrandPrimary`
- **Destructive actions**: `UiTheme.BrandError` for delete/remove buttons
- **Success confirmations**: `UiTheme.BrandSuccess` for save/add buttons
- **Default/Cancel**: `Type = AntdUI.ButtonType.Default` + `UiTheme.TextSecondary`

### 5. Image Handling Without Icons
- Use standard `PictureBox` with `SizeMode = PictureBoxSizeMode.Zoom`
- Implement async loading via `ImageHelper` with bitmap fallback
- Letter-based avatars (first letter + random color) for missing images
- Cache generated bitmaps to reduce GC pressure

### 6. Form Control Events
- Subscribe to events in `OnLoad()` or `OnShown()`, not constructor
- Unsubscribe in `Dispose()` for long-lived forms to prevent memory leaks
- Use null-conditional operator for event handlers: `TextChanged?.Invoke(...)`

### 7. Debouncing User Input
- TextInput fields (search, filters): Use `TextBoxExtensions.AddDebounceTextChanged(ms)`
- Prevents excessive database queries during rapid typing
- Default debounce: 300ms (configurable per field)

---

*Last Updated: April 17, 2026*
