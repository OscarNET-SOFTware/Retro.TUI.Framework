// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiColorRole.cs" company="OscarNET-SOFTware">
// ···
//      Retro.TUI.Framework - A modern C-Sharp .NET multiplatform TUI framework
//      powered by SkiaSharp, inspired by Turbo Vision with a PC Tools 9.x retro aesthetic.
// ···
//      Copyright (c) 2026 Oscar Fernandez Gonzalez a.k.a. Osc@rNET and Contributors
//      Licensed under the MIT License. See the 'LICENSE.md' file for details.
// ···
//      Third-party components are used in this project. For full license texts,
//      see the 'licenses' folder and the 'THIRD-PARTY-NOTICES.md' file.
// ···
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Retro.TUI.Theming;

/// <summary>
/// Identifies every visual color role in the framework.
/// </summary>
/// <remarks>
/// Roles are semantic: they name <em>what</em> a color is used for, not which color it is.
/// The actual color values are supplied by a <see cref="TuiPalette"/> inside a <see cref="TuiTheme"/>.
/// No view or widget may hard-code a color; all rendering must resolve colors through this enum.
/// </remarks>
public enum TuiColorRole
{
    // ── Desktop ───────────────────────────────────────────────────────────

    /// <summary>Background fill color of the desktop area.</summary>
    DesktopBackground,

    // ── Application title bar ─────────────────────────────────────────────

    /// <summary>Background of the application-level title bar.</summary>
    AppTitleBackground,

    /// <summary>Foreground (text) of the application-level title bar.</summary>
    AppTitleForeground,

    /// <summary>
    /// Foreground color of the clock displayed in the application title bar.
    /// Distinct from <see cref="AppTitleForeground"/> to allow independent theming.
    /// </summary>
    AppTitleClockForeground,

    // ── Window ────────────────────────────────────────────────────────────

    /// <summary>Background fill of the window client area.</summary>
    WindowBackground,

    /// <summary>Default foreground (text) color inside the window client area.</summary>
    WindowForeground,

    /// <summary>Color of the window outer border.</summary>
    WindowBorder,

    /// <summary>Background of the active window title bar.</summary>
    WindowTitleBackground,

    /// <summary>Foreground (text) of the active window title bar.</summary>
    WindowTitleForeground,

    /// <summary>Background of the title bar of an inactive (background) window.</summary>
    WindowTitleInactiveBackground,

    /// <summary>Foreground (text) of the title bar of an inactive (background) window.</summary>
    WindowTitleInactiveForeground,

    /// <summary>Background of the window close/restore button glyph area.</summary>
    WindowCloseButtonBackground,

    /// <summary>Foreground (glyph) of the window close/restore button.</summary>
    WindowCloseButtonForeground,

    /// <summary>
    /// Color of the drop shadow cast by the window.
    /// Opacity is controlled separately by <see cref="TuiTheme.ShadowOpacity"/>.
    /// </summary>
    WindowShadow,

    // ── Dialog ────────────────────────────────────────────────────────────

    /// <summary>Background fill of the dialog client area.</summary>
    DialogBackground,

    /// <summary>Default foreground (text) color inside the dialog client area.</summary>
    DialogForeground,

    /// <summary>Color of the dialog outer border.</summary>
    DialogBorder,

    /// <summary>Background of the dialog title bar (normal dialogs).</summary>
    DialogTitleBackground,

    /// <summary>Foreground (text) of the dialog title bar (normal dialogs).</summary>
    DialogTitleForeground,

    /// <summary>
    /// Background of the dialog title bar for warning or error dialogs.
    /// Used instead of <see cref="DialogTitleBackground"/> when the dialog conveys a warning.
    /// </summary>
    DialogTitleWarningBackground,

    /// <summary>
    /// Foreground color for highlighted or instructional text inside a dialog.
    /// Distinct from <see cref="DialogForeground"/> to draw attention to key information.
    /// </summary>
    DialogHighlightForeground,

    // ── Menu bar ──────────────────────────────────────────────────────────

    /// <summary>Background of the horizontal menu bar.</summary>
    MenuBackground,

    /// <summary>Foreground (text) of normal (unselected, enabled) items in the menu bar.</summary>
    MenuForeground,

    /// <summary>
    /// Foreground color of the accelerator (hot-key) letter in a menu bar item.
    /// </summary>
    MenuHotkeyForeground,

    /// <summary>Background of the currently selected (highlighted) item in the menu bar.</summary>
    MenuSelectedBackground,

    /// <summary>Foreground (text) of the currently selected item in the menu bar.</summary>
    MenuSelectedForeground,

    /// <summary>Foreground color of a disabled item in the menu bar.</summary>
    MenuDisabledForeground,

    /// <summary>Color of horizontal separator lines inside a menu popup.</summary>
    MenuSeparator,

    // ── Menu popup ────────────────────────────────────────────────────────

    /// <summary>Background fill of a menu popup (drop-down) panel.</summary>
    MenuPopupBackground,

    /// <summary>Foreground (text) of normal (unselected, enabled) items in a menu popup.</summary>
    MenuPopupForeground,

    /// <summary>
    /// Foreground color of the accelerator (hot-key) letter of an unselected popup item.
    /// </summary>
    MenuPopupHotkeyForeground,

    /// <summary>Background of the currently selected item in a menu popup.</summary>
    MenuPopupSelectedBackground,

    /// <summary>Foreground (text) of the currently selected item in a menu popup.</summary>
    MenuPopupSelectedForeground,

    /// <summary>
    /// Foreground color of the accelerator letter of the currently selected popup item.
    /// </summary>
    MenuPopupSelectedHotkeyForeground,

    /// <summary>
    /// Color of the sub-menu arrow indicator (▶) shown on items that open a nested popup.
    /// </summary>
    MenuPopupSubMenuArrow,

    /// <summary>Color of the border surrounding a menu popup panel.</summary>
    MenuPopupBorder,

    /// <summary>
    /// Color of the drop shadow cast by a menu popup panel.
    /// Opacity is controlled separately by <see cref="TuiTheme.ShadowOpacity"/>.
    /// </summary>
    MenuPopupShadow,

    // ── Status bar ────────────────────────────────────────────────────────

    /// <summary>Background of the status bar.</summary>
    StatusBackground,

    /// <summary>Foreground (text) of normal status bar items.</summary>
    StatusForeground,

    /// <summary>Background of the function-key number area in a status bar item.</summary>
    StatusKeyBackground,

    /// <summary>Foreground (number) of the function-key indicator in a status bar item.</summary>
    StatusKeyForeground,

    // ── Label ─────────────────────────────────────────────────────────────

    /// <summary>Foreground (text) of a label control.</summary>
    LabelForeground,

    /// <summary>
    /// Background of a label control.
    /// Typically transparent (same as the parent dialog or window background).
    /// </summary>
    LabelBackground,

    // ── Input ─────────────────────────────────────────────────────────────

    /// <summary>Background of an unfocused input field.</summary>
    InputBackground,

    /// <summary>Foreground (text) of an unfocused input field.</summary>
    InputForeground,

    /// <summary>Background of a focused input field.</summary>
    InputFocusBackground,

    /// <summary>Foreground (text) of a focused input field.</summary>
    InputFocusForeground,

    /// <summary>Background of the selected text region inside an input field.</summary>
    InputSelectionBackground,

    /// <summary>Foreground of the selected text region inside an input field.</summary>
    InputSelectionForeground,

    /// <summary>
    /// Color of the bottom and right border shadow of an input field.
    /// Provides the characteristic inset visual used in PC Tools-style themes.
    /// </summary>
    InputBorderBottom,

    // ── Button ────────────────────────────────────────────────────────────

    /// <summary>Background of a normal (unfocused) button.</summary>
    ButtonBackground,

    /// <summary>Foreground (label text) of a normal (unfocused) button.</summary>
    ButtonForeground,

    /// <summary>
    /// Foreground color of the accelerator letter of a normal (unfocused) button.
    /// </summary>
    ButtonAcceleratorForeground,

    /// <summary>Background of a focused button.</summary>
    ButtonFocusBackground,

    /// <summary>Foreground (label text) of a focused button.</summary>
    ButtonFocusForeground,

    /// <summary>Foreground color of the accelerator letter of a focused button.</summary>
    ButtonFocusAcceleratorForeground,

    /// <summary>
    /// Color of the drop shadow rendered below and to the right of a button.
    /// </summary>
    ButtonShadow,

    // ── CheckBox / RadioButton ────────────────────────────────────────────

    /// <summary>Background of a check box or radio button control.</summary>
    CheckBackground,

    /// <summary>Foreground (label text) of a check box or radio button control.</summary>
    CheckForeground,

    /// <summary>Background of a focused check box or radio button control.</summary>
    CheckFocusBackground,

    /// <summary>Foreground (label text) of a focused check box or radio button control.</summary>
    CheckFocusForeground,

    /// <summary>Color of the check mark (✓) or selection indicator (●) glyph.</summary>
    CheckMarkColor,

    /// <summary>
    /// Foreground color of the accelerator letter in a check box or radio button label.
    /// </summary>
    CheckAcceleratorForeground,

    // ── ListBox ───────────────────────────────────────────────────────────

    /// <summary>Background of the list item area (unselected items).</summary>
    ListBackground,

    /// <summary>Foreground (text) of unselected list items.</summary>
    ListForeground,

    /// <summary>Background of the currently selected list item.</summary>
    ListSelectedBackground,

    /// <summary>Foreground (text) of the currently selected list item.</summary>
    ListSelectedForeground,

    /// <summary>Background of the list header row.</summary>
    ListHeaderBackground,

    /// <summary>Foreground (text) of the list header row.</summary>
    ListHeaderForeground,

    // ── ScrollBar ─────────────────────────────────────────────────────────

    /// <summary>Background of the scroll bar track (inactive / unscrolled region).</summary>
    ScrollBarBackground,

    /// <summary>Background of the active (scrolled) region of the scroll bar track.</summary>
    ScrollBarActiveBackground,

    /// <summary>Color of the scroll bar thumb (position indicator).</summary>
    ScrollBarThumb,

    /// <summary>Color of the scroll bar arrow glyphs (▲ ▼ ◄ ►).</summary>
    ScrollBarArrow,

    /// <summary>Background of the scroll bar arrow button areas.</summary>
    ScrollBarArrowBackground,

    // ── Mouse cursor ─────────────────────────────────────────────────────

    /// <summary>Fill color of the custom mouse cursor pointer bitmap.</summary>
    MouseCursorFill,

    /// <summary>Outline color of the custom mouse cursor pointer bitmap.</summary>
    MouseCursorOutline,
}
