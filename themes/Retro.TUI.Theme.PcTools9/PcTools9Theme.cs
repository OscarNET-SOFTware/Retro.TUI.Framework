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

using System.Reflection;

using Retro.TUI.Theming;

using SkiaSharp;

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
/// Accessing <see cref="Instance"/> triggers the static initializer, which loads
/// the IBM VGA 9x16 typeface from the embedded resource exactly once.
/// The font file (<c>PxPlus_IBM_VGA_9x16.ttf</c> by VileR, CC BY-SA 4.0) is
/// embedded as a resource in this assembly.
/// <para/>
/// Access the pre-built theme via <see cref="Instance"/>.
/// </remarks>
public static class PcTools9Theme
{
    // ── Raw palette colors ────────────────────────────────────────────────────
    // Named constants make Build() self-documenting and allow other code to
    // reference raw color values without going through the palette.

    /// <summary>Pure black. EGA color #0.</summary>
    public static readonly SKColor Black = new(0x00, 0x00, 0x00);

    /// <summary>
    /// EGA bright blue. The signature color of the PC Tools interface.
    /// Used for window backgrounds, dialog backgrounds and the application title bar.
    /// EGA color #9.
    /// </summary>
    public static readonly SKColor EgaBrightBlue = new(0x55, 0x55, 0xFF);

    /// <summary>
    /// EGA Bright Cyan — standard EGA color #11 (0x55, 0xFF, 0xFF).
    /// Used for dialog backgrounds.
    /// </summary>
    public static readonly SKColor EgaBrightCyan = new(0x55, 0xFF, 0xFF);

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

    // ── Font constants ────────────────────────────────────────────────────────

    /// <summary>
    /// Family name of the IBM VGA 9x16 font as reported by the typeface metadata.
    /// Must match the value set in <see cref="Retro.TUI.Theming.TuiTheme.FontFamily"/>.
    /// </summary>
    public const string FontFamilyName = "PxPlus IBM VGA 9x16";

    /// <summary>
    /// Manifest resource name of the embedded font file in this assembly.
    /// Set via <c>&lt;LogicalName&gt;</c> in the project file.
    /// </summary>
    private const string FontResourceName =
        "Retro.TUI.Theme.PcTools9.Fonts.PxPlus_IBM_VGA_9x16.ttf";

    // ── Loaded typeface ───────────────────────────────────────────────────────

    /// <summary>
    /// The IBM VGA 9x16 typeface loaded from the embedded resource.
    /// </summary>
    /// <remarks>
    /// In SkiaSharp 3.x, <see cref="SKFontManager"/> only manages system fonts;
    /// it does not expose a public API to register custom typefaces.
    /// The correct pattern for embedded fonts is to load the <see cref="SKTypeface"/>
    /// once, retain it for the application lifetime, and pass it directly to
    /// <see cref="SKFont"/> or to <c>TuiFont.Load</c> in <c>Retro.TUI.Rendering</c>.
    /// <para/>
    /// This property is initialized by the static field initializer, which the CLR
    /// guarantees runs exactly once and is thread-safe.
    /// </remarks>
    public static SKTypeface Typeface { get; } = LoadTypeface();

    // ── Theme singleton ───────────────────────────────────────────────────────

    /// <summary>
    /// The pre-built, ready-to-use PC Tools 9.x theme instance.
    /// </summary>
    /// <remarks>
    /// Created lazily on first access; thread-safe via CLR type initialization.
    /// Accessing this property also ensures <see cref="Typeface"/> is initialized.
    /// </remarks>
    public static TuiTheme Instance { get; } = Build();

    // ── Private: typeface loader ──────────────────────────────────────────────

    private static SKTypeface LoadTypeface()
    {
        Assembly assembly = typeof(PcTools9Theme).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(FontResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded font resource '{FontResourceName}' was not found in assembly " +
                $"'{assembly.GetName().Name}'. " +
                $"Ensure 'Fonts/PxPlus_IBM_VGA_9x16.ttf' exists in the theme project " +
                $"and is declared as EmbeddedResource with the correct LogicalName.");

        SKTypeface typeface = SKTypeface.FromStream(stream)
            ?? throw new InvalidOperationException(
                $"SkiaSharp could not load a typeface from resource '{FontResourceName}'. " +
                $"Verify that the file is a valid TrueType font.");

        // Note: SKFontManager.Default.RegisterTypeface was removed in SkiaSharp 3.x.
        // The typeface is stored in the static Typeface property and passed directly
        // to TuiTheme.Typeface so TuiApplication.ResolveTypeface can use it without
        // going through MatchFamily.

        return typeface;
    }

    // ── Private: theme builder ────────────────────────────────────────────────

    private static TuiTheme Build()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            // ── Desktop ───────────────────────────────────────────────────
            [TuiColorRole.DesktopBackground] = PcToolsGray,
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
            [TuiColorRole.DialogBackground] = EgaBrightCyan,
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

            // ── Mouse cursor ──────────────────────────────────────────────
            [TuiColorRole.MouseCursorFill] = White,
            [TuiColorRole.MouseCursorOutline] = Black,
        });

        return new TuiTheme
        {
            Name = "PC Tools 9.x",
            Palette = palette,
            DesktopPattern = TuiDesktopPattern.None,
            FontFamily = FontFamilyName,
            FontSize = 16f,
            Typeface = Typeface,
            ShadowOpacity = 0.65f,
            ShadowOffsetX = 8,
            ShadowOffsetY = 7,
        };
    }
}
