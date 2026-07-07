// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiTheme.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

namespace Retro.TUI.Theming;

/// <summary>
/// Provides the fixed typography and shadow parameters used by the framework renderer.
/// </summary>
/// <remarks>
/// The color scheme is fixed and defined in <see cref="TuiPalette"/> and
/// <see cref="TuiEgaPalette"/>. <see cref="TuiTheme"/> only holds parameters
/// that may vary by environment: font family, font size, typeface and shadow settings.
/// <para/>
/// All properties have sensible defaults matching the IBM VGA 9×16 reference font.
/// Override individual properties via the static setters when the host environment
/// requires different values (e.g. a different screen resolution or font size).
/// </remarks>
public static class TuiTheme
{
    // ── Font constants ────────────────────────────────────────────────────────

    /// <summary>
    /// Manifest resource name of the embedded font file in this assembly.
    /// Set via <c>&lt;LogicalName&gt;</c> in the project file.
    /// </summary>
    private const string FontResourceName =
        "Retro.TUI.Theming.Fonts.PxPlus_IBM_VGA_9x16.ttf";

    // ── Typography ────────────────────────────────────────────────────────────

    /// <summary>Family name of the IBM VGA 9x16 font.</summary>
    public static string FontFamily { get; } = "PxPlus IBM VGA 9x16";

    /// <summary>
    /// Gets the IBM VGA 9x16 typeface loaded from the embedded resource.
    /// </summary>
    /// <remarks>
    /// Initialized by the static constructor. In SkiaSharp 3.x,
    /// <c>SKFontManager.RegisterTypeface</c> was removed; the typeface is
    /// loaded once and passed directly to the rendering layer.
    /// </remarks>
    public static SKTypeface Typeface { get; }

    /// <summary>
    /// Size of the primary font in logical pixels.
    /// </summary>
    /// <value><c>16f</c> — matches the 9×16 px VGA glyph height.</value>
    public static float FontSize { get; } = 16f;

    // ── Shadow ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Opacity of drop shadows cast by windows, dialogs and menu popups.
    /// </summary>
    /// <remarks>
    /// The shadow color is defined by <see cref="TuiColorRole.WindowShadow"/> and
    /// related roles in <see cref="TuiPalette"/>. This value controls transparency.
    /// </remarks>
    /// <value><c>0.65f</c> — range [0.0, 1.0].</value>
    public static float ShadowOpacity { get; } = 0.65f;

    /// <summary>Horizontal offset in cells of drop shadows.</summary>
    /// <value><c>1</c> cell = 9 px.</value>
    public static int ShadowOffsetX { get; } = 1;

    /// <summary>Vertical offset in cells of drop shadows.</summary>
    /// <value><c>1</c> cell = 16 px.</value>
    public static int ShadowOffsetY { get; } = 1;

    /// <summary>
    /// Initializes static members — loads the IBM VGA 9x16 typeface from
    /// the embedded resource exactly once.
    /// </summary>
    static TuiTheme()
    {
        Assembly assembly = typeof(TuiTheme).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(FontResourceName);

        Typeface = (stream is not null
            ? SKTypeface.FromStream(stream)
            : null)
            ?? SKTypeface.Default;
    }
}
