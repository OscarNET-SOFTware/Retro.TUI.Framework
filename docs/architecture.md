# Retro.TUI.Framework — Technical Architecture

> Detailed technical design document.  
> Complements the [README.md](../README.md) with the contracts, hierarchies and
> behaviours of each layer.

---

## Index

1. [Events layer](#1-events-layer)
2. [Theming layer](#2-theming-layer)
3. [Hosting layer](#3-hosting-layer)
4. [Rendering layer](#4-rendering-layer)
5. [Core layer — Application and MessageLoop](#5-core-layer--application-and-messageloop)
6. [Views layer — View tree](#6-views-layer--view-tree)
7. [Windows layer](#7-windows-layer)
8. [Widgets layer](#8-widgets-layer)
9. [Cross-cutting flows](#9-cross-cutting-flows)

---

## 1. Events layer

**Project**: `Retro.TUI.Events`  
**Root namespace**: `Retro.TUI.Events`  
**No dependencies on other framework layers.**

This layer defines the communication language between all framework components.
No other layer can operate without it.

### 1.1 TuiEvent hierarchy

All events are immutable `record` types that inherit from `TuiEvent`.
This guarantees immutability, structural equality and native pattern matching support.

```
TuiEvent  (abstract record)
├── TuiKeyEvent
├── TuiMouseEvent
├── TuiCommandEvent
├── TuiTimerEvent
└── TuiFocusEvent
```

**Contracts:**

```csharp
namespace Retro.TUI.Events;

/// <summary>Base type for all framework events.</summary>
public abstract record TuiEvent;

/// <summary>Keyboard event: special key or character.</summary>
public sealed record TuiKeyEvent(
    TuiKey       Key,
    char         KeyChar,
    TuiModifiers Modifiers
) : TuiEvent;

/// <summary>Mouse event: movement, button press or scroll wheel.</summary>
public sealed record TuiMouseEvent(
    TuiMouseAction Action,
    int            Col,
    int            Row,
    TuiMouseButton Button
) : TuiEvent;

/// <summary>
/// High-level command event.
/// Enables decoupled communication between components.
/// </summary>
public sealed record TuiCommandEvent(
    TuiCommand Command,
    object?    Parameter = null
) : TuiEvent;

/// <summary>Timer event — emitted periodically by the message loop.</summary>
public sealed record TuiTimerEvent(
    TimeSpan Elapsed
) : TuiEvent;

/// <summary>Focus change event.</summary>
public sealed record TuiFocusEvent(
    TuiFocusAction Action   // Gained | Lost
) : TuiEvent;
```

### 1.2 Supporting enumerations

```csharp
/// <summary>Special keys. Printable characters go in TuiKeyEvent.KeyChar.</summary>
public enum TuiKey
{
    None,
    Enter, Escape, Tab, BackSpace, Delete,
    Insert, Home, End, PageUp, PageDown,
    Left, Right, Up, Down,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12
}

/// <summary>Keyboard modifiers — combinable as flags.</summary>
[Flags]
public enum TuiModifiers
{
    None    = 0,
    Shift   = 1 << 0,
    Control = 1 << 1,
    Alt     = 1 << 2
}

/// <summary>Mouse actions.</summary>
public enum TuiMouseAction { Move, ButtonDown, ButtonUp, Click, DoubleClick, Wheel }

/// <summary>Mouse buttons.</summary>
public enum TuiMouseButton { None, Left, Right, Middle }

/// <summary>Focus change action.</summary>
public enum TuiFocusAction { Gained, Lost }
```

### 1.3 TuiCommand

Commands are high-level semantic identifiers, independent of raw input.
They allow a button, a function key, and a menu item to trigger the same action.

```csharp
/// <summary>
/// Application command. Values 0–99 are reserved by the framework.
/// Applications must use values from 100 onwards.
/// </summary>
public enum TuiCommand
{
    // System commands (reserved)
    None         = 0,
    Quit         = 1,
    Close        = 2,
    Help         = 3,
    Ok           = 4,
    Cancel       = 5,
    Yes          = 6,
    No           = 7,

    // Window commands
    ZoomWindow   = 10,
    ResizeWindow = 11,
    MoveWindow   = 12,
    NextWindow   = 13,
    PrevWindow   = 14,

    // Free range for applications
    UserDefined  = 100
}
```

### 1.4 TuiEventQueue

```csharp
/// <summary>
/// Framework event queue.
/// The host publishes events; the message loop consumes them.
/// Implemented on top of System.Threading.Channels for native async support.
/// </summary>
public sealed class TuiEventQueue : IDisposable
{
    private readonly Channel<TuiEvent> _channel;

    public TuiEventQueue(int capacity = 256);

    /// <summary>Posts an event to the queue (non-blocking).</summary>
    public bool TryPost(TuiEvent evt);

    /// <summary>Reads the next event asynchronously.</summary>
    public ValueTask<TuiEvent> ReadAsync(CancellationToken ct = default);

    /// <summary>Tries to read an event without blocking.</summary>
    public bool TryRead(out TuiEvent? evt);

    public void Dispose();
}
```

---

## 2. Theming layer

**Project**: `Retro.TUI.Theming`  
**Root namespace**: `Retro.TUI.Theming`  
**Dependencies**: none from the framework (only SkiaSharp for `SKColor`).

This layer defines the complete theming system. No control or view has hardcoded
colors or fonts: they always query the active theme.

### 2.1 TuiColorRole

Semantic enum that names each visual role in the framework.
It is independent of any concrete color value.

```csharp
public enum TuiColorRole
{
    // Desktop
    DesktopBackground,
    DesktopPatternDot,

    // Application title bar
    AppTitleBackground,
    AppTitleForeground,

    // Window
    WindowBackground,
    WindowForeground,
    WindowBorder,
    WindowTitleBackground,
    WindowTitleForeground,
    WindowShadow,

    // Dialog
    DialogBackground,
    DialogForeground,
    DialogBorder,
    DialogTitleBackground,
    DialogTitleForeground,

    // Menu
    MenuBackground,
    MenuForeground,
    MenuSelectedBackground,
    MenuSelectedForeground,
    MenuDisabledForeground,
    MenuSeparator,

    // Status bar
    StatusBackground,
    StatusForeground,
    StatusKeyBackground,
    StatusKeyForeground,

    // Form controls
    LabelForeground,
    LabelBackground,
    InputBackground,
    InputForeground,
    InputFocusBackground,
    InputFocusForeground,
    InputSelectionBackground,
    InputSelectionForeground,

    // Button
    ButtonBackground,
    ButtonForeground,
    ButtonFocusBackground,
    ButtonFocusForeground,
    ButtonShadow,

    // CheckBox / RadioButton
    CheckBackground,
    CheckForeground,
    CheckFocusBackground,
    CheckFocusForeground,
    CheckMarkColor,

    // ListBox
    ListBackground,
    ListForeground,
    ListSelectedBackground,
    ListSelectedForeground,

    // ScrollBar
    ScrollBarBackground,
    ScrollBarThumb,
    ScrollBarArrow,
}
```

### 2.2 TuiPalette

```csharp
/// <summary>
/// Maps semantic color roles to concrete color values.
/// Immutable after construction.
/// </summary>
public sealed class TuiPalette
{
    private readonly IReadOnlyDictionary<TuiColorRole, SKColor> _colors;

    public TuiPalette(IReadOnlyDictionary<TuiColorRole, SKColor> colors);

    /// <summary>Returns the color for the given role.</summary>
    public SKColor this[TuiColorRole role] => _colors[role];

    /// <summary>Tries to get the color; returns fallback if the role is not defined.</summary>
    public SKColor GetOrDefault(TuiColorRole role, SKColor fallback = default);

    /// <summary>Creates a copy with some roles overridden.</summary>
    public TuiPalette With(IReadOnlyDictionary<TuiColorRole, SKColor> overrides);
}
```

### 2.3 TuiDesktopPattern

```csharp
/// <summary>Desktop background pattern style.</summary>
public enum TuiDesktopPattern
{
    None,             // Solid background
    DotGrid,          // Dot grid pattern (Norton/PCTools style)
    Checkerboard,     // Checkerboard pattern
    HorizontalLines,
    VerticalLines
}
```

### 2.4 TuiTheme

```csharp
/// <summary>
/// Complete theme: color palette, font and global visual configuration.
/// The active instance is accessed through TuiApplication.CurrentTheme.
/// </summary>
public sealed class TuiTheme
{
    /// <summary>Identifying name of the theme.</summary>
    public required string Name { get; init; }

    /// <summary>Color palette of the theme.</summary>
    public required TuiPalette Palette { get; init; }

    /// <summary>Embedded resource name of the font (monospaced).</summary>
    public required string FontResourceName { get; init; }

    /// <summary>Font size in logical pixels.</summary>
    public required float FontSize { get; init; }

    /// <summary>Desktop background pattern style.</summary>
    public TuiDesktopPattern DesktopPattern { get; init; } = TuiDesktopPattern.DotGrid;

    /// <summary>Line width for single borders.</summary>
    public float BorderWidth { get; init; } = 1f;

    /// <summary>Line width for double borders.</summary>
    public float BorderDoubleWidth { get; init; } = 1f;

    /// <summary>Gap between the two lines of a double border, in pixels.</summary>
    public float BorderDoubleGap { get; init; } = 2f;

    /// <summary>Shadow opacity (0.0 – 1.0).</summary>
    public float ShadowOpacity { get; init; } = 0.55f;
}
```

### 2.5 PcTools9 theme (separate project)

The `Retro.TUI.Theme.PcTools9` project exposes a static class with the theme instance.
It depends only on `Retro.TUI.Theming`.

```csharp
namespace Retro.TUI.Theme.PcTools9;

public static class PcTools9Theme
{
    /// <summary>
    /// PC Tools 9.x theme instance by Central Point Software.
    /// EGA 16-color palette, IBM VGA CP437 font 9×16 px.
    /// </summary>
    public static TuiTheme Instance { get; } = Build();

    private static TuiTheme Build() { /* ... */ }
}
```

---

## 3. Hosting layer

**Project**: `Retro.TUI.Hosting`  
**Root namespace**: `Retro.TUI.Hosting`  
**Dependencies**: `Retro.TUI.Events`

This layer is the only point of contact with SDL2 and the operating system.
The framework knows nothing about SDL2 outside this layer.

### 3.1 ITuiHost

```csharp
/// <summary>
/// Window host contract.
/// Abstracts SDL2 (or any future backend) from the rest of the framework.
/// </summary>
public interface ITuiHost : IDisposable
{
    /// <summary>Window width in physical pixels.</summary>
    int PixelWidth { get; }

    /// <summary>Window height in physical pixels.</summary>
    int PixelHeight { get; }

    /// <summary>DPI scale factor (1.0 on standard displays, 2.0 on HiDPI).</summary>
    float DpiScale { get; }

    /// <summary>Window title.</summary>
    string Title { get; set; }

    /// <summary>
    /// Creates and shows the window.
    /// Must be called before any other operation.
    /// </summary>
    void Initialize(TuiHostOptions options);

    /// <summary>
    /// Processes pending OS events and publishes them
    /// to the framework event queue.
    /// Called on every message loop iteration.
    /// </summary>
    /// <returns>False if the host has requested shutdown.</returns>
    bool PollEvents(TuiEventQueue queue);

    /// <summary>
    /// Presents the rendered frame on screen.
    /// Called at the end of every message loop iteration.
    /// </summary>
    void Present();

    /// <summary>Returns the Skia surface for rendering the current frame.</summary>
    SKSurface AcquireRenderSurface();
}
```

### 3.2 TuiHostOptions

```csharp
public sealed class TuiHostOptions
{
    public required string Title  { get; init; }
    public required int    Width  { get; init; }
    public required int    Height { get; init; }
    public bool Resizable         { get; init; } = false;
    public bool HideSystemCursor  { get; init; } = true;
    public bool CenterOnScreen    { get; init; } = true;

    /// <summary>
    /// When true, the window has no native OS decorations (title bar, borders).
    /// The framework renders its own title bar.
    /// </summary>
    public bool Borderless        { get; init; } = true;
}
```

### 3.3 SdlHost

```csharp
/// <summary>
/// ITuiHost implementation on top of SDL2.
/// This is the only class in the framework that references SDL2 directly.
/// Translates SDL events to TuiEvent and publishes them to TuiEventQueue.
/// </summary>
public sealed class SdlHost : ITuiHost
{
    // Internal SDL2 implementation.
}
```

**SDL event → TuiEvent mapping:**

| SDL event | Generated TuiEvent |
|---|---|
| `SDL_KEYDOWN` | `TuiKeyEvent` |
| `SDL_TEXTINPUT` | `TuiKeyEvent` (KeyChar) |
| `SDL_MOUSEMOTION` | `TuiMouseEvent(Move)` |
| `SDL_MOUSEBUTTONDOWN` | `TuiMouseEvent(ButtonDown)` |
| `SDL_MOUSEBUTTONUP` | `TuiMouseEvent(ButtonUp)` + `Click` |
| `SDL_QUIT` | `TuiCommandEvent(Quit)` |
| `SDL_WINDOWEVENT_CLOSE` | `TuiCommandEvent(Close)` |

---

## 4. Rendering layer

**Project**: `Retro.TUI.Rendering`  
**Root namespace**: `Retro.TUI.Rendering`  
**Dependencies**: `Retro.TUI.Theming`

This layer wraps SkiaSharp and exposes an API oriented to the character grid.
No view or control accesses SkiaSharp directly: they always go through `TuiRenderContext`.

### 4.1 TuiGrid

Character grid metrics. Initialized once with the actual values from the loaded font.

```csharp
/// <summary>
/// Character grid metrics.
/// Immutable after Initialize. Accessible from TuiRenderContext.
/// </summary>
public sealed class TuiGrid
{
    /// <summary>Cell width in pixels.</summary>
    public int CellWidth { get; private set; }

    /// <summary>Cell height in pixels.</summary>
    public int CellHeight { get; private set; }

    /// <summary>Number of visible columns.</summary>
    public int Columns { get; private set; }

    /// <summary>Number of visible rows.</summary>
    public int Rows { get; private set; }

    /// <summary>Total screen width in pixels.</summary>
    public int ScreenWidth { get; private set; }

    /// <summary>Total screen height in pixels.</summary>
    public int ScreenHeight { get; private set; }

    // Cell → pixel conversion
    public float PixelX(int col)    => col * CellWidth;
    public float PixelY(int row)    => row * CellHeight;
    public float BaselineY(int row) => row * CellHeight + _ascent;

    // Pixel → cell conversion
    public int CellCol(float px) => (int)(px / CellWidth);
    public int CellRow(float py) => (int)(py / CellHeight);

    public void Initialize(int screenW, int screenH, SKFont font);
}
```

### 4.2 TuiFont

```csharp
/// <summary>
/// Framework font manager.
/// Loads the font from embedded resources and exposes the configured SKFont.
/// </summary>
public sealed class TuiFont : IDisposable
{
    public SKFont        SkFont  { get; private set; } = null!;
    public SKFontMetrics Metrics { get; private set; }

    public void Load(string resourceName, float size);
    public void Dispose();
}
```

### 4.3 TuiRenderContext

The main rendering interface. Each frame, `TuiApplication` creates a context,
passes it to the view tree for drawing, then finalizes it.

```csharp
/// <summary>
/// Rendering context for a single frame.
/// Operates in grid coordinates (col, row), not in pixels.
/// </summary>
public sealed class TuiRenderContext
{
    public TuiGrid  Grid  { get; }
    public TuiTheme Theme { get; }

    // ── Background ───────────────────────────────────────────────

    public void FillCell(int col, int row, TuiColorRole bg);
    public void FillRow(int row, TuiColorRole bg);
    public void FillRect(int col, int row, int width, int height, TuiColorRole bg);
    public void FillRect(int col, int row, int width, int height, SKColor color);

    // ── Text ─────────────────────────────────────────────────────

    public void DrawText(int col, int row, string text,
        TuiColorRole fg, TuiColorRole bg);

    public void DrawTextCentered(int col, int row, int width, string text,
        TuiColorRole fg, TuiColorRole bg);

    public void DrawTextClipped(int col, int row, int maxWidth, string text,
        TuiColorRole fg, TuiColorRole bg);

    // ── Borders ──────────────────────────────────────────────────

    public void DrawBorder(int col, int row, int width, int height,
        TuiColorRole border, TuiColorRole bg);

    public void DrawBorderDouble(int col, int row, int width, int height,
        TuiColorRole border, TuiColorRole bg);

    // ── Shadow ───────────────────────────────────────────────────

    public void DrawShadow(int col, int row, int width, int height);

    // ── Desktop pattern ──────────────────────────────────────────

    public void DrawDesktopPattern(TuiDesktopPattern pattern);

    // ── Clipping ─────────────────────────────────────────────────

    /// <summary>
    /// Sets a clip rect in grid coordinates.
    /// Stacks: restore with PopClip.
    /// </summary>
    public void PushClip(int col, int row, int width, int height);
    public void PopClip();

    // ── Custom cursor ────────────────────────────────────────────

    public void DrawCursor(float pixelX, float pixelY);
}
```

---

## 5. Core layer — Application and MessageLoop

**Project**: `Retro.TUI.Core`  
**Root namespace**: `Retro.TUI.Core`  
**Dependencies**: `Retro.TUI.Events`, `Retro.TUI.Theming`, `Retro.TUI.Rendering`, `Retro.TUI.Hosting`

The framework nucleus: entry point, main loop and global management.

### 5.1 TuiApplication

```csharp
/// <summary>
/// Root framework class. Entry point for every Retro.TUI application.
/// Manages the lifecycle: initialization, message loop and shutdown.
/// Follows the single-instance pattern (not a static Singleton: passed by injection).
/// </summary>
public abstract class TuiApplication : IDisposable
{
    // ── Global properties ─────────────────────────────────────────

    /// <summary>Active theme. Changing this property changes the complete look.</summary>
    public TuiTheme CurrentTheme { get; set; }

    /// <summary>Root desktop of the view hierarchy.</summary>
    public TuiDesktop Desktop { get; private set; } = null!;

    /// <summary>Event queue shared by the host and the message loop.</summary>
    public TuiEventQueue EventQueue { get; } = new();

    /// <summary>Focus manager.</summary>
    public TuiFocusManager Focus { get; } = new();

    // ── Lifecycle ─────────────────────────────────────────────────

    /// <summary>
    /// Initializes the framework, creates the window and starts the message loop.
    /// Blocks until the application terminates.
    /// </summary>
    public void Run(ITuiHost host, TuiHostOptions options);

    /// <summary>Requests application shutdown.</summary>
    public void Quit();

    // ── Overridable methods ───────────────────────────────────────

    /// <summary>
    /// Application initialization — build the Desktop and add views.
    /// Called after the host and rendering are initialized, before the first frame.
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// High-level command processing.
    /// base.OnCommand handles Quit, Close and system commands.
    /// </summary>
    protected virtual void OnCommand(TuiCommandEvent cmd);

    public void Dispose();
}
```

### 5.2 TuiMessageLoop

```csharp
/// <summary>
/// Framework main loop. Orchestrates the poll → update → render cycle.
/// Used internally by TuiApplication.
/// </summary>
internal sealed class TuiMessageLoop
{
    /// <summary>
    /// Runs the loop until shutdown is requested.
    /// Each iteration:
    ///   1. PollEvents  → publishes TuiEvent to the queue
    ///   2. DispatchEvents → dispatches to the view tree
    ///   3. UpdateTimers → publishes TuiTimerEvent if due
    ///   4. Render      → draws the complete view tree
    ///   5. Present     → presents the frame
    /// </summary>
    public void Run(
        ITuiHost          host,
        TuiEventQueue     queue,
        TuiDesktop        desktop,
        TuiRenderContext  renderCtx,
        CancellationToken ct);
}
```

### 5.3 TuiFocusManager

```csharp
/// <summary>
/// Manages keyboard focus across focusable views.
/// Maintains a tab order and the currently focused view.
/// </summary>
public sealed class TuiFocusManager
{
    public TuiView? Current { get; private set; }

    public void Register(TuiView view);
    public void Unregister(TuiView view);

    /// <summary>Sets focus on a specific view.</summary>
    public void SetFocus(TuiView view);

    /// <summary>Moves focus to the next view in tab order.</summary>
    public void FocusNext();

    /// <summary>Moves focus to the previous view in tab order.</summary>
    public void FocusPrevious();

    public bool IsFocused(TuiView view) => Current == view;

    /// <summary>
    /// Clears all focus state.
    /// Called when a modal dialog is closed.
    /// </summary>
    public void Clear();
}
```

---

## 6. Views layer — View tree

**Project**: `Retro.TUI.Views`  
**Root namespace**: `Retro.TUI.Views`  
**Dependencies**: `Retro.TUI.Core`, `Retro.TUI.Rendering`

### 6.1 TuiView

The base class for the entire visual tree.

```csharp
/// <summary>
/// Base class for all visual elements in the framework.
/// Manages the composition tree, relative coordinates,
/// lifecycle and invalidation.
/// </summary>
public abstract class TuiView
{
    // ── Position and size (in grid cells) ─────────────────────────

    public int Col    { get; set; }
    public int Row    { get; set; }
    public int Width  { get; set; }
    public int Height { get; set; }

    // ── State ─────────────────────────────────────────────────────

    public bool Visible   { get; set; } = true;
    public bool Enabled   { get; set; } = true;
    public bool Focusable { get; set; } = false;

    // ── Tree ──────────────────────────────────────────────────────

    public TuiView?                  Parent   { get; internal set; }
    protected IReadOnlyList<TuiView> Children { get; }

    // ── Absolute coordinates ──────────────────────────────────────

    public int AbsCol => (Parent?.AbsCol ?? 0) + Col;
    public int AbsRow => (Parent?.AbsRow ?? 0) + Row;

    // ── Tree management ───────────────────────────────────────────

    public    void Add(TuiView child);
    public    void Remove(TuiView child);
    protected void Clear();

    // ── Invalidation ─────────────────────────────────────────────

    /// <summary>
    /// Marks this view as needing a redraw.
    /// Propagates invalidation up to the parent if needed.
    /// </summary>
    public void Invalidate();

    protected bool IsDirty { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────

    /// <summary>Called when the view is added to its parent.</summary>
    protected virtual void OnAdded() { }

    /// <summary>Called when the view is removed from its parent.</summary>
    protected virtual void OnRemoved() { }

    // ── Rendering ─────────────────────────────────────────────────

    /// <summary>
    /// Draws this view and its children.
    /// Subclasses override Draw() for their own appearance,
    /// calling base.Draw() at the end to draw children.
    /// </summary>
    public virtual void Draw(TuiRenderContext ctx)
    {
        if (!Visible) return;
        foreach (var child in Children)
            child.Draw(ctx);
        IsDirty = false;
    }

    // ── Event dispatch ────────────────────────────────────────────

    /// <summary>
    /// Dispatches an event to this view.
    /// Returns true if the event was consumed (should not propagate further).
    /// </summary>
    public virtual bool HandleEvent(TuiEvent evt) => false;

    // ── Hit testing ───────────────────────────────────────────────

    public bool HitTest(int col, int row);

    /// <summary>
    /// Finds the deepest view in the tree that contains (col, row).
    /// </summary>
    public TuiView? FindAt(int col, int row);

    // ── Focus lifecycle events ────────────────────────────────────

    public virtual void OnGotFocus()  { }
    public virtual void OnLostFocus() { }
}
```

### 6.2 TuiGroup

```csharp
/// <summary>
/// Container view with event dispatch to children.
/// Equivalent to TGroup in Turbo Vision.
/// </summary>
public class TuiGroup : TuiView
{
    public TuiGroup(int col, int row, int width, int height)
        : base(col, row, width, height) { }

    /// <summary>
    /// Dispatches the event to children in reverse order (z-order)
    /// until one consumes it.
    /// </summary>
    public override bool HandleEvent(TuiEvent evt)
    {
        // Reverse dispatch — highest z-order child has priority
        for (int i = Children.Count - 1; i >= 0; i--)
            if (Children[i].HandleEvent(evt)) return true;
        return base.HandleEvent(evt);
    }
}
```

### 6.3 TuiDesktop

```csharp
/// <summary>
/// Root application container.
/// Manages the background, modal stack and window z-order.
/// </summary>
public sealed class TuiDesktop : TuiGroup
{
    // ── Pre-rendered background ───────────────────────────────────

    /// <summary>
    /// Pre-renders the background as a static image.
    /// Call once after changing the theme.
    /// </summary>
    public void RebuildBackground(TuiRenderContext ctx);

    // ── Modal stack ───────────────────────────────────────────────

    /// <summary>
    /// Opens a view in modal mode.
    /// Keyboard and mouse events only reach the active modal view.
    /// </summary>
    public void PushModal(TuiView modal);

    /// <summary>Closes the active modal view and restores the previous state.</summary>
    public TuiView? PopModal();

    public bool HasModal => _modalStack.Count > 0;

    // ── Z-order ───────────────────────────────────────────────────

    /// <summary>Brings a window to the front of the z-order.</summary>
    public void BringToFront(TuiView view);
}
```

---

## 7. Windows layer

**Project**: `Retro.TUI.Windows`  
**Root namespace**: `Retro.TUI.Windows`  
**Dependencies**: `Retro.TUI.Views`

### 7.1 TuiWindow

```csharp
/// <summary>
/// Window with border, title bar, shadow and drag support.
/// </summary>
public class TuiWindow : TuiGroup
{
    public string Title      { get; set; }
    public bool   Movable    { get; set; } = true;
    public bool   ShowShadow { get; set; } = true;
    public bool   ShowTitle  { get; set; } = true;

    public TuiWindow(string title, int col, int row, int width, int height);

    public override void Draw(TuiRenderContext ctx);
    public override bool HandleEvent(TuiEvent evt);

    // Inner area (excludes border and title)
    public int InnerCol    => AbsCol + 1;
    public int InnerRow    => AbsRow + 2;   // title + border
    public int InnerWidth  => Width  - 2;
    public int InnerHeight => Height - 3;
}
```

### 7.2 TuiDialog

```csharp
/// <summary>
/// Modal dialog. Inherits from TuiWindow and integrates
/// with the Desktop modal stack.
/// </summary>
public class TuiDialog : TuiWindow
{
    public TuiDialog(string title, int col, int row, int width, int height);

    /// <summary>
    /// Opens the dialog as modal over the given Desktop.
    /// Blocks until Close() is called.
    /// </summary>
    public TuiCommand ShowModal(TuiDesktop desktop);

    /// <summary>Closes the dialog returning the given command result.</summary>
    public void Close(TuiCommand result = TuiCommand.Cancel);
}
```

---

## 8. Widgets layer

**Project**: `Retro.TUI.Widgets`  
**Root namespace**: `Retro.TUI.Widgets`  
**Dependencies**: `Retro.TUI.Windows`

Planned controls and their responsibilities:

| Widget | Description |
|---|---|
| `TuiMenuBar` | Top menu bar with items and hover |
| `TuiMenu` | Drop-down menu associated with a MenuBar item |
| `TuiMenuItem` | Individual item in a drop-down menu |
| `TuiStatusBar` | Bottom bar with function key shortcuts |
| `TuiButton` | Pressable button with focus and Enter/Space |
| `TuiLabel` | Static text, no focus |
| `TuiInputLine` | Text field with cursor, selection and editing |
| `TuiCheckBox` | Checkbox control |
| `TuiRadioButton` | Radio button with exclusive groups |
| `TuiListBox` | Item list with selection and scrolling |
| `TuiScrollBar` | Scroll bar (vertical/horizontal) |
| `TuiAppTitleBar` | Application title bar (fixed, not a window title) |

All widgets follow the same contract:

- Inherit from `TuiView` (simple) or `TuiGroup` (composite).
- No hardcoded colors: access the theme via `TuiRenderContext.Theme`.
- Implement `HandleEvent` to respond to keyboard and mouse.
- Call `Invalidate()` when their state changes and a redraw is needed.
- Expose standard .NET events to notify application code:
  `EventHandler`, `EventHandler<T>`.

---

## 9. Cross-cutting flows

### 9.1 Application lifecycle

```
TuiApplication.Run(host, options)
    │
    ├── host.Initialize(options)          // Creates SDL2 window
    ├── TuiFont.Load(...)                 // Loads IBM VGA font
    ├── TuiGrid.Initialize(...)           // Calculates grid metrics
    ├── TuiRenderContext.Create(...)      // Prepares Skia context
    ├── Desktop = new TuiDesktop()
    ├── Desktop.RebuildBackground(ctx)    // Pre-renders background
    ├── OnInitialize()                    // App builds its view tree
    │
    └── TuiMessageLoop.Run(...)           // Main loop
            │
            ├── host.PollEvents(queue)    // SDL → TuiEvent → queue
            ├── DispatchEvents(queue)     // queue → view tree
            ├── UpdateTimers()            // TuiTimerEvent if due
            ├── Desktop.Draw(ctx)         // Draws complete tree
            └── host.Present()            // Presents the frame
```

### 9.2 Mouse event flow

```
SDL_MOUSEBUTTONDOWN
    │
    └── SdlHost.PollEvents()
            │
            └── TuiEventQueue.TryPost(
                    new TuiMouseEvent(ButtonDown, col, row, Left))
                    │
                    └── TuiMessageLoop.DispatchEvents()
                            │
                            ├── Desktop.HasModal?
                            │       └── Yes → dispatch only to active modal view
                            │
                            └── No → Desktop.FindAt(col, row)
                                    │
                                    └── target.HandleEvent(mouseEvent)
                                            │
                                            └── Consumed? → stop
                                            └── No → propagate to parent
```

### 9.3 Keyboard event flow

```
SDL_KEYDOWN
    │
    └── SdlHost → TuiKeyEvent → TuiEventQueue
            │
            └── TuiMessageLoop.DispatchEvents()
                    │
                    ├── Tab / Shift+Tab?
                    │       └── TuiFocusManager.FocusNext() / FocusPrevious()
                    │
                    ├── Escape? → TuiCommandEvent(Cancel) → active modal
                    │
                    └── TuiFocusManager.Current?.HandleEvent(keyEvent)
```

### 9.4 Invalidation and redraw

In the first milestone iteration, **full redraw** is used: every frame redraws
the entire tree from the Desktop.

The `Invalidate()` model is designed from the start to support **dirty regions**
in a future iteration:

1. `view.Invalidate()` marks `IsDirty = true` on the view and propagates up to the parent.
2. In the render pass, only views with `IsDirty = true` are redrawn.
3. Clean regions are copied from the previous frame.

This will be activated as an optimization in a later milestone, with no changes
to the public API.

---

*Living document — updated with each completed milestone.*
