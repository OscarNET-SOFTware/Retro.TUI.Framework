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

namespace Retro.TUI.Theming;

/// <summary>
/// The complete, immutable definition of a visual theme.
/// </summary>
/// <remarks>
/// A <see cref="TuiTheme"/> aggregates all visual parameters used by the framework renderer:
/// the color palette, the desktop pattern, typography settings and shadow parameters.
/// <para/>
/// Instances are immutable after construction via the <c>init</c>-only properties.
/// The recommended construction pattern is object-initializer syntax:
/// <code>
/// var theme = new TuiTheme
/// {
///     Name    = "My Theme",
///     Palette = myPalette,
/// };
/// </code>
/// Theme packages (e.g. <c>Retro.TUI.Theme.PcTools9</c>) expose a pre-built singleton
/// via a static <c>Instance</c> property.
/// </remarks>
public sealed class TuiTheme
{
    // ── Identity ──────────────────────────────────────────────────────────

    /// <summary>
    /// Human-readable display name of the theme.
    /// </summary>
    public required string Name { get; init; }

    // ── Color palette ─────────────────────────────────────────────────────

    /// <summary>
    /// The color palette that maps every <see cref="TuiColorRole"/> to a concrete color.
    /// </summary>
    public required TuiPalette Palette { get; init; }

    // ── Typography ────────────────────────────────────────────────────────

    /// <summary>
    /// Name of the primary font family used to render all UI text.
    /// </summary>
    /// <remarks>
    /// The font must be available as an embedded resource in the theme assembly
    /// or registered with the SkiaSharp font manager before the theme is applied.
    /// </remarks>
    /// <value>Defaults to <c>"IBM VGA 9x16"</c> (the canonical CP437 VGA bitmap font).</value>
    public string FontFamily { get; init; } = "IBM VGA 9x16";

    /// <summary>
    /// Gets the pre-loaded <see cref="SkiaSharp.SKTypeface"/> for this theme, or
    /// <see langword="null"/> to resolve by <see cref="FontFamily"/> name via
    /// <c>SKFontManager.Default.MatchFamily</c>.
    /// </summary>
    /// <remarks>
    /// Theme packages that load fonts from embedded resources should set this
    /// property directly. <c>SKFontManager.RegisterTypeface</c> was removed in
    /// SkiaSharp 3.x, so family-name lookup via <c>MatchFamily</c> does not work
    /// for embedded fonts. When this property is non-null, <c>TuiApplication</c>
    /// uses it directly and skips the <c>MatchFamily</c> fallback.
    /// </remarks>
    public SkiaSharp.SKTypeface? Typeface { get; init; }

    /// <summary>
    /// Size of the primary font in logical pixels.
    /// </summary>
    /// <value>Defaults to <c>16f</c> (matches the 9×16 px VGA glyph height).</value>
    public float FontSize { get; init; } = 16f;

    // ── Shadow ────────────────────────────────────────────────────────────

    /// <summary>
    /// Opacity of drop shadows cast by windows, dialogs and menu popups.
    /// </summary>
    /// <remarks>
    /// The shadow color itself is defined by <see cref="TuiColorRole.WindowShadow"/>,
    /// <see cref="TuiColorRole.MenuPopupShadow"/> etc. in the palette.
    /// This value controls how transparent the shadow appears over whatever is behind it.
    /// </remarks>
    /// <value>
    /// A value in the range [0.0, 1.0].
    /// Defaults to <c>0.65f</c>.
    /// </value>
    public float ShadowOpacity { get; init; } = 0.65f;

    /// <summary>
    /// Horizontal offset in pixels of drop shadows.
    /// </summary>
    /// <value>Defaults to <c>8</c>. Unit is pixels, not grid cells.</value>
    public int ShadowOffsetX { get; init; } = 8;

    /// <summary>
    /// Vertical offset in pixels of drop shadows.
    /// </summary>
    /// <value>Defaults to <c>7</c>. Unit is pixels, not grid cells.</value>
    public int ShadowOffsetY { get; init; } = 7;
}
