// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PcTools9Theme.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Theming;

namespace Retro.TUI.Theme.PcTools9;

/// <summary>
/// Provides the canonical PC Tools 9.x visual theme by Central Point Software (~1991).
/// </summary>
/// <remarks>
/// Color values were derived by direct pixel measurement (Paint.NET) on original
/// PC Tools 9.x screenshots at full resolution. Reference documentation is in
/// <c>docs/theming-reference.md</c>.
/// <para/>
/// The palette uses 8 distinct colors. Two of them — <see cref="PcToolsGray"/> and
/// <see cref="PcToolsLightGray"/> — are custom values that do not appear in the
/// standard 16-color EGA/VGA palette.
/// <para/>
/// Access the pre-built theme via <see cref="Instance"/>.
/// </remarks>
public static class PcTools9Theme
{
    // ── Raw palette colors ────────────────────────────────────────────────
    // Named constants make the Build() method self-documenting and allow
    // other code to reference the raw color values without accessing the palette.

    /// <summary>Pure black. EGA color #0.</summary>
    public static readonly SKColor Black = new(0x00, 0x00, 0x00);

    /// <summary>
    /// EGA bright blue. The signature color of the PC Tools interface.
    /// Used for window backgrounds, dialog backgrounds and the application title bar.
    /// EGA color #9.
    /// </summary>
    public static readonly SKColor EgaBrightBlue = new(0x55, 0x55, 0xFF);

    /// <summary>
    /// Medium gray. Custom PC Tools value — not a standard EGA color.
    /// Used for the desktop background, status bar and scroll bar thumb.
    /// </summary>
    public static readonly SKColor PcToolsGray = new(0x69, 0x69, 0x69);

    /// <summary>EGA dark red. Used for all accelerator / hot-key letters. EGA color #4.</summary>
    public static readonly SKColor EgaDarkRed = new(0xAA, 0x00, 0x00);

    /// <summary>
    /// Light gray. Custom PC Tools value — not a standard EGA color.
    /// Used for scroll bar tracks, arrow backgrounds and inactive window title bars.
    /// </summary>
    public static readonly SKColor PcToolsLightGray = new(0xCA, 0xCA, 0xCA);

    /// <summary>EGA bright red. Used for warning dialog title bars. EGA color #12.</summary>
    public static readonly SKColor EgaBrightRed = new(0xFF, 0x55, 0x55);

    /// <summary>
    /// EGA bright yellow. Used for function-key numbers, check/radio accelerators
    /// and the application title bar clock. EGA color #14.
    /// </summary>
    public static readonly SKColor EgaBrightYellow = new(0xFF, 0xFF, 0x55);

    /// <summary>Pure white. EGA color #15.</summary>
    public static readonly SKColor White = new(0xFF, 0xFF, 0xFF);

    // ── Theme singleton ───────────────────────────────────────────────────

    /// <summary>
    /// The pre-built, ready-to-use PC Tools 9.x theme instance.
    /// </summary>
    /// <remarks>
    /// The instance is created lazily on first access and is thread-safe.
    /// </remarks>
    public static TuiTheme Instance { get; } = Build();

    // ── Builder ───────────────────────────────────────────────────────────

    private static TuiTheme Build()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            // ── Desktop ───────────────────────────────────────────────────
            [TuiColorRole.DesktopBackground] = PcToolsGray,
            // DesktopPatternDot: no dot pattern in PC Tools — same as background.
            [TuiColorRole.DesktopPatternDot] = PcToolsGray,

            // ── Application title bar ─────────────────────────────────────
            [TuiColorRole.AppTitleBackground] = EgaBrightBlue,
            [TuiColorRole.AppTitleForeground] = White,
            [TuiColorRole.AppTitleClockForeground] = EgaBrightYellow,

            // ── Window ────────────────────────────────────────────────────
            [TuiColorRole.WindowBackground] = EgaBrightBlue,
            [TuiColorRole.WindowForeground] = White,
            [TuiColorRole.WindowBorder] = Black,
            [TuiColorRole.WindowTitleBackground] = White,
            [TuiColorRole.WindowTitleForeground] = Black,
            [TuiColorRole.WindowTitleInactiveBackground] = PcToolsLightGray,
            [TuiColorRole.WindowTitleInactiveForeground] = PcToolsGray,
            [TuiColorRole.WindowCloseButtonBackground] = White,
            [TuiColorRole.WindowCloseButtonForeground] = Black,
            [TuiColorRole.WindowShadow] = Black,

            // ── Dialog ────────────────────────────────────────────────────
            [TuiColorRole.DialogBackground] = EgaBrightBlue,
            [TuiColorRole.DialogForeground] = White,
            [TuiColorRole.DialogBorder] = Black,
            [TuiColorRole.DialogTitleBackground] = White,
            [TuiColorRole.DialogTitleForeground] = Black,
            [TuiColorRole.DialogTitleWarningBackground] = EgaBrightRed,
            [TuiColorRole.DialogHighlightForeground] = EgaBrightYellow,

            // ── Menu bar ──────────────────────────────────────────────────
            [TuiColorRole.MenuBackground] = White,
            [TuiColorRole.MenuForeground] = Black,
            [TuiColorRole.MenuHotkeyForeground] = EgaDarkRed,
            [TuiColorRole.MenuSelectedBackground] = Black,
            [TuiColorRole.MenuSelectedForeground] = White,
            [TuiColorRole.MenuDisabledForeground] = PcToolsGray,
            [TuiColorRole.MenuSeparator] = Black,

            // ── Menu popup ────────────────────────────────────────────────
            [TuiColorRole.MenuPopupBackground] = White,
            [TuiColorRole.MenuPopupForeground] = Black,
            [TuiColorRole.MenuPopupHotkeyForeground] = EgaDarkRed,
            [TuiColorRole.MenuPopupSelectedBackground] = Black,
            [TuiColorRole.MenuPopupSelectedForeground] = White,
            [TuiColorRole.MenuPopupSelectedHotkeyForeground] = EgaDarkRed,
            [TuiColorRole.MenuPopupSubMenuArrow] = White,
            [TuiColorRole.MenuPopupBorder] = Black,
            [TuiColorRole.MenuPopupShadow] = Black,

            // ── Status bar ────────────────────────────────────────────────
            [TuiColorRole.StatusBackground] = PcToolsGray,
            [TuiColorRole.StatusForeground] = White,
            [TuiColorRole.StatusKeyBackground] = PcToolsGray,
            [TuiColorRole.StatusKeyForeground] = EgaBrightYellow,

            // ── Label ─────────────────────────────────────────────────────
            [TuiColorRole.LabelForeground] = White,
            [TuiColorRole.LabelBackground] = EgaBrightBlue,

            // ── Input ─────────────────────────────────────────────────────
            [TuiColorRole.InputBackground] = White,
            [TuiColorRole.InputForeground] = Black,
            [TuiColorRole.InputFocusBackground] = White,
            [TuiColorRole.InputFocusForeground] = Black,
            [TuiColorRole.InputSelectionBackground] = Black,
            [TuiColorRole.InputSelectionForeground] = White,
            [TuiColorRole.InputBorderBottom] = Black,

            // ── Button ────────────────────────────────────────────────────
            [TuiColorRole.ButtonBackground] = White,
            [TuiColorRole.ButtonForeground] = Black,
            [TuiColorRole.ButtonAcceleratorForeground] = EgaDarkRed,
            [TuiColorRole.ButtonFocusBackground] = White,
            [TuiColorRole.ButtonFocusForeground] = Black,
            [TuiColorRole.ButtonFocusAcceleratorForeground] = EgaDarkRed,
            [TuiColorRole.ButtonShadow] = Black,

            // ── CheckBox / RadioButton ────────────────────────────────────
            [TuiColorRole.CheckBackground] = EgaBrightBlue,
            [TuiColorRole.CheckForeground] = White,
            [TuiColorRole.CheckFocusBackground] = EgaBrightBlue,
            [TuiColorRole.CheckFocusForeground] = White,
            [TuiColorRole.CheckMarkColor] = White,
            [TuiColorRole.CheckAcceleratorForeground] = EgaBrightYellow,

            // ── ListBox ───────────────────────────────────────────────────
            [TuiColorRole.ListBackground] = EgaBrightBlue,
            [TuiColorRole.ListForeground] = White,
            [TuiColorRole.ListSelectedBackground] = Black,
            [TuiColorRole.ListSelectedForeground] = White,
            [TuiColorRole.ListHeaderBackground] = Black,
            [TuiColorRole.ListHeaderForeground] = White,

            // ── ScrollBar ─────────────────────────────────────────────────
            [TuiColorRole.ScrollBarBackground] = PcToolsLightGray,
            [TuiColorRole.ScrollBarActiveBackground] = White,
            [TuiColorRole.ScrollBarThumb] = PcToolsGray,
            [TuiColorRole.ScrollBarArrow] = Black,
            [TuiColorRole.ScrollBarArrowBackground] = PcToolsLightGray,
        });

        return new TuiTheme
        {
            Name = "PC Tools 9.x",
            Palette = palette,
            DesktopPattern = TuiDesktopPattern.None,
            FontFamily = "IBM VGA 9x16",
            FontSize = 16f,
            ShadowOpacity = 1.0f,
            ShadowOffsetX = 2,
            ShadowOffsetY = 1,
        };
    }
}
