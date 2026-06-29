// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiColorMap.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

namespace Retro.TUI.Theming;

/// <summary>
/// Maps every <see cref="TuiColorRole"/> to its canonical <see cref="SKColor"/>
/// using the standard EGA 16-color palette defined in <see cref="TuiEgaPalette"/>.
/// </summary>
/// <remarks>
/// This class is the single source of truth for the framework's fixed color scheme.
/// All rendering code resolves colors through <see cref="TuiPalette.Resolve"/>,
/// which delegates to <see cref="Resolve"/> here.
/// <para/>
/// Two non-EGA grays are used internally for roles that require intermediate
/// values not present in the standard 16-color EGA specification:
/// <list type="bullet">
///   <item><description>
///     <c>#696969</c> — mid gray, used for desktop background and status bar.
///   </description></item>
///   <item><description>
///     <c>#CACACA</c> — light gray, used for inactive title bars and scroll bar tracks.
///   </description></item>
/// </list>
/// </remarks>
internal static class TuiColorMap
{
    // ── Non-EGA grays ─────────────────────────────────────────────────────────
    // These two values do not appear in the standard 16-color EGA palette but
    // are required for roles that need intermediate gray values.

    /// <summary>Mid gray <c>#696969</c> — desktop background and status bar.</summary>
    private static readonly SKColor s_gray = new(0x69, 0x69, 0x69);

    /// <summary>Light gray <c>#CACACA</c> — inactive title bars and scroll bar tracks.</summary>
    private static readonly SKColor s_lightGray = new(0xCA, 0xCA, 0xCA);

    // ── Role map ──────────────────────────────────────────────────────────────

    private static readonly Dictionary<TuiColorRole, SKColor> s_map = new()
    {
        // ── Desktop ───────────────────────────────────────────────────────────
        [TuiColorRole.DesktopBackground] = TuiEgaPalette.EgaDarkGray,

        // ── Application title bar ─────────────────────────────────────────────
        [TuiColorRole.AppTitleBackground] = TuiEgaPalette.EgaBrightBlue,
        [TuiColorRole.AppTitleForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.AppTitleClockForeground] = TuiEgaPalette.EgaYellow,

        // ── Window ────────────────────────────────────────────────────────────
        [TuiColorRole.WindowBackground] = TuiEgaPalette.EgaBrightBlue,
        [TuiColorRole.WindowForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.WindowBorder] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.WindowTitleBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.WindowTitleForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.WindowTitleInactiveBackground] = s_lightGray,
        [TuiColorRole.WindowTitleInactiveForeground] = s_gray,
        [TuiColorRole.WindowCloseButtonBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.WindowCloseButtonForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.WindowShadow] = TuiEgaPalette.EgaBlack,

        // ── Dialog ────────────────────────────────────────────────────────────
        [TuiColorRole.DialogBackground] = TuiEgaPalette.EgaBrightCyan,
        [TuiColorRole.DialogForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.DialogBorder] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.DialogTitleBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.DialogTitleForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.DialogTitleWarningBackground] = TuiEgaPalette.EgaBrightRed,
        [TuiColorRole.DialogHighlightForeground] = TuiEgaPalette.EgaYellow,

        // ── Menu bar ──────────────────────────────────────────────────────────
        [TuiColorRole.MenuBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MenuForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.MenuHotkeyForeground] = TuiEgaPalette.EgaRed,
        [TuiColorRole.MenuSelectedBackground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.MenuSelectedForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MenuDisabledForeground] = s_gray,
        [TuiColorRole.MenuSeparator] = TuiEgaPalette.EgaBlack,

        // ── Menu popup ────────────────────────────────────────────────────────
        [TuiColorRole.MenuPopupBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MenuPopupForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.MenuPopupHotkeyForeground] = TuiEgaPalette.EgaRed,
        [TuiColorRole.MenuPopupSelectedBackground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.MenuPopupSelectedForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MenuPopupSelectedHotkeyForeground] = TuiEgaPalette.EgaRed,
        [TuiColorRole.MenuPopupSubMenuArrow] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MenuPopupBorder] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.MenuPopupShadow] = TuiEgaPalette.EgaBlack,

        // ── Status bar ────────────────────────────────────────────────────────
        [TuiColorRole.StatusBackground] = s_gray,
        [TuiColorRole.StatusForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.StatusKeyBackground] = s_gray,
        [TuiColorRole.StatusKeyForeground] = TuiEgaPalette.EgaYellow,

        // ── Label ─────────────────────────────────────────────────────────────
        [TuiColorRole.LabelForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.LabelBackground] = TuiEgaPalette.EgaBrightBlue,

        // ── Input ─────────────────────────────────────────────────────────────
        [TuiColorRole.InputBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.InputForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.InputFocusBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.InputFocusForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.InputSelectionBackground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.InputSelectionForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.InputBorderBottom] = TuiEgaPalette.EgaBlack,

        // ── Button ────────────────────────────────────────────────────────────
        [TuiColorRole.ButtonBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.ButtonForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.ButtonAcceleratorForeground] = TuiEgaPalette.EgaRed,
        [TuiColorRole.ButtonFocusBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.ButtonFocusForeground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.ButtonFocusAcceleratorForeground] = TuiEgaPalette.EgaRed,
        [TuiColorRole.ButtonShadow] = TuiEgaPalette.EgaBlack,

        // ── CheckBox / RadioButton ────────────────────────────────────────────
        [TuiColorRole.CheckBackground] = TuiEgaPalette.EgaBrightBlue,
        [TuiColorRole.CheckForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.CheckFocusBackground] = TuiEgaPalette.EgaBrightBlue,
        [TuiColorRole.CheckFocusForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.CheckMarkColor] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.CheckAcceleratorForeground] = TuiEgaPalette.EgaYellow,

        // ── ListBox ───────────────────────────────────────────────────────────
        [TuiColorRole.ListBackground] = TuiEgaPalette.EgaBrightBlue,
        [TuiColorRole.ListForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.ListSelectedBackground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.ListSelectedForeground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.ListHeaderBackground] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.ListHeaderForeground] = TuiEgaPalette.EgaWhite,

        // ── ScrollBar ─────────────────────────────────────────────────────────
        [TuiColorRole.ScrollBarBackground] = s_lightGray,
        [TuiColorRole.ScrollBarActiveBackground] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.ScrollBarThumb] = s_gray,
        [TuiColorRole.ScrollBarArrow] = TuiEgaPalette.EgaBlack,
        [TuiColorRole.ScrollBarArrowBackground] = s_lightGray,

        // ── Mouse cursor ──────────────────────────────────────────────────────
        [TuiColorRole.MouseCursorFill] = TuiEgaPalette.EgaWhite,
        [TuiColorRole.MouseCursorOutline] = TuiEgaPalette.EgaBlack,
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the <see cref="SKColor"/> assigned to <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The semantic color role to resolve.</param>
    /// <returns>The <see cref="SKColor"/> mapped to <paramref name="role"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when <paramref name="role"/> has no entry in the color map.
    /// This indicates a missing mapping that must be added to <see cref="s_map"/>.
    /// </exception>
    internal static SKColor Resolve(TuiColorRole role) => s_map[role];
}
