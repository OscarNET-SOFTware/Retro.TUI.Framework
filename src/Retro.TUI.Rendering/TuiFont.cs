// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiFont.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Rendering;

/// <summary>
/// Wraps a loaded <see cref="SKTypeface"/> and exposes a configured
/// <see cref="SKFont"/> ready for use by <see cref="TuiRenderContext"/>.
/// </summary>
/// <remarks>
/// In SkiaSharp 3.x, <see cref="SKFontManager"/> only manages system fonts;
/// it does not expose a public API to register custom typefaces.
/// The framework loads the IBM VGA 9x16 font from an embedded resource in
/// <see cref="Theming.TuiTheme"/> and exposes it via
/// <see cref="Theming.TuiTheme.Typeface"/>.
/// <see cref="TuiFont"/> receives that typeface directly through
/// <see cref="Load(SKTypeface, float)"/>.
/// <para/>
/// The framework does not provide a fallback font: passing an invalid
/// typeface or size fails immediately.
/// </remarks>
public sealed class TuiFont : IDisposable
{
    // ── Private state ─────────────────────────────────────────────────────────

    private bool _disposed;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets the configured <see cref="SKFont"/> ready for drawing.</summary>
    /// <remarks>Valid after a successful call to <see cref="Load(SKTypeface, float)"/>.</remarks>
    public SKFont SkFont { get; private set; } = null!;

    /// <summary>Gets the metrics of the loaded font.</summary>
    /// <remarks>Valid after a successful call to <see cref="Load(SKTypeface, float)"/>.</remarks>
    public SKFontMetrics Metrics { get; private set; }

    // ── Loading ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Configures the font from the supplied typeface at the given size.
    /// </summary>
    /// <param name="typeface">
    /// The typeface to use. Typically <see cref="Retro.TUI.Theming.TuiTheme.Typeface"/>,
    /// which is loaded from the embedded IBM VGA 9x16 font resource.
    /// </param>
    /// <param name="size">Font size in logical pixels. Must be greater than zero.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="typeface"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="size"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when <see cref="Dispose"/> has already been called.
    /// </exception>
    public void Load(SKTypeface typeface, float size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(typeface);

        if (size <= 0f)
            throw new ArgumentException("Font size must be greater than zero.", nameof(size));

        // Release any previously loaded SKFont before reloading.
        DisposeSkiaObjects();

        // Alias edging produces sharp pixel-art glyphs, consistent with the
        // bitmap-font aesthetic of PC Tools 9.x.
        SkFont = new SKFont(typeface, size) { Edging = SKFontEdging.Alias };
        Metrics = SkFont.Metrics;
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <summary>Releases all SkiaSharp resources held by this instance.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        DisposeSkiaObjects();
        _disposed = true;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void DisposeSkiaObjects()
    {
        // SKFont does NOT own its typeface — the theme retains ownership.
        // Only dispose the SKFont wrapper itself.
        SkFont?.Dispose();
        SkFont = null!;
    }
}
