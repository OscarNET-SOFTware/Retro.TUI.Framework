// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiGrid.cs" company="OscarNET-SOFTware">
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
/// Character grid metrics derived from the active font and the window dimensions.
/// </summary>
/// <remarks>
/// <see cref="TuiGrid"/> is initialized once per session by <see cref="TuiRenderContext"/>
/// after the font is loaded. All values are immutable from that point on.
/// <para/>
/// The grid bridges the gap between pixel space (used by SkiaSharp) and cell space
/// (used by every view and control in the framework). All rendering operations are
/// expressed in <c>(col, row)</c> coordinates; <see cref="TuiGrid"/> provides the
/// conversion utilities to translate them into pixel coordinates before drawing.
/// </remarks>
public sealed class TuiGrid
{
    // ── Private state ─────────────────────────────────────────────────────────

    private float _ascent;
    private bool _initialized;

    // ── Public properties ─────────────────────────────────────────────────────

    /// <summary>Gets the cell width in physical pixels.</summary>
    public int CellWidth { get; private set; }

    /// <summary>Gets the cell height in physical pixels.</summary>
    public int CellHeight { get; private set; }

    /// <summary>Gets the number of fully visible columns on screen.</summary>
    public int Columns { get; private set; }

    /// <summary>Gets the number of fully visible rows on screen.</summary>
    public int Rows { get; private set; }

    /// <summary>Gets the total screen (window) width in physical pixels.</summary>
    public int ScreenWidth { get; private set; }

    /// <summary>Gets the total screen (window) height in physical pixels.</summary>
    public int ScreenHeight { get; private set; }

    // ── Initialization ────────────────────────────────────────────────────────

    /// <summary>
    /// Computes all grid metrics from the window dimensions and the loaded font.
    /// </summary>
    /// <param name="screenWidth">Window width in physical pixels.</param>
    /// <param name="screenHeight">Window height in physical pixels.</param>
    /// <param name="font">
    /// The <see cref="SKFont"/> whose metrics determine cell size.
    /// Must be a monospaced font.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="font"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="screenWidth"/> or <paramref name="screenHeight"/>
    /// is less than or equal to zero.
    /// </exception>
    /// <remarks>
    /// Must be called exactly once before any grid-coordinate conversion.
    /// Subsequent calls are silently ignored to prevent accidental re-initialization
    /// during the application lifetime.
    /// </remarks>
    public void Initialize(int screenWidth, int screenHeight, SKFont font)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(screenWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(screenHeight);

        if (_initialized)
            return;

        // Measure a representative character to determine cell dimensions.
        // We use 'W' — the widest glyph in most monospaced fonts — to ensure
        // cells are never narrower than any printable character.
        font.MeasureText("W", out SKRect bounds);
        SKFontMetrics metrics = font.Metrics;

        // Cell width: round the measured glyph advance to the nearest integer.
        CellWidth = (int)MathF.Round(font.MeasureText("W"));
        // Cell height: cap + descent (both reported as positive values by SkiaSharp).
        CellHeight = (int)MathF.Round(-metrics.Ascent + metrics.Descent);

        // Guard against degenerate fonts that report zero metrics.
        if (CellWidth <= 0)
            CellWidth = 9;
        if (CellHeight <= 0)
            CellHeight = 16;

        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Columns = screenWidth / CellWidth;
        Rows = screenHeight / CellHeight;

        // Ascent is negative in SkiaSharp convention (distance above baseline).
        // Store as positive offset so BaselineY() can add it directly.
        _ascent = -metrics.Ascent;
        _initialized = true;
    }

    // ── Cell → pixel conversions ──────────────────────────────────────────────

    /// <summary>
    /// Returns the left edge of the cell at the given column, in physical pixels.
    /// </summary>
    /// <param name="col">Zero-based column index.</param>
    public float PixelX(int col) => col * CellWidth;

    /// <summary>
    /// Returns the top edge of the cell at the given row, in physical pixels.
    /// </summary>
    /// <param name="row">Zero-based row index.</param>
    public float PixelY(int row) => row * CellHeight;

    /// <summary>
    /// Returns the text baseline Y coordinate for the given row, in physical pixels.
    /// </summary>
    /// <param name="row">Zero-based row index.</param>
    /// <remarks>
    /// SkiaSharp's <c>DrawText</c> places glyphs relative to the baseline, not
    /// the top of the cell. Use this method as the Y argument whenever drawing
    /// text with a raw <see cref="SKCanvas"/>.
    /// </remarks>
    public float BaselineY(int row) => row * CellHeight + _ascent;

    // ── Pixel → cell conversions ──────────────────────────────────────────────

    /// <summary>
    /// Returns the column index of the cell that contains the given X pixel coordinate.
    /// </summary>
    /// <param name="px">X coordinate in physical pixels.</param>
    public int CellCol(float px) => (int)(px / CellWidth);

    /// <summary>
    /// Returns the row index of the cell that contains the given Y pixel coordinate.
    /// </summary>
    /// <param name="py">Y coordinate in physical pixels.</param>
    public int CellRow(float py) => (int)(py / CellHeight);
}
