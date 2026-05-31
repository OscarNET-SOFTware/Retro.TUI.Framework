// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiRenderContext.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Theming;

using SkiaSharp;

namespace Retro.TUI.Rendering;

/// <summary>
/// Grid-oriented rendering context for a single frame.
/// </summary>
/// <remarks>
/// <see cref="TuiRenderContext"/> is the <em>only</em> rendering API that views and
/// controls may use. No framework layer above <c>Retro.TUI.Rendering</c> references
/// SkiaSharp directly.
/// <para/>
/// All drawing methods operate in <em>grid coordinates</em> — <c>(col, row)</c> pairs
/// that identify character cells. Pixel conversion is handled internally via
/// <see cref="TuiGrid"/>.
/// <para/>
/// The context is a reusable object. <c>TuiApplication</c> calls
/// <see cref="BeginFrame"/> at the start of every render pass and
/// <see cref="EndFrame"/> after the view tree has finished drawing; both methods
/// are <c>internal</c> and invisible outside the framework.
/// </remarks>
public sealed class TuiRenderContext : IDisposable
{
    // ── Private state ─────────────────────────────────────────────────────────

    private SKCanvas? _canvas;
    private readonly SKPaint _fillPaint = new() { IsAntialias = false };
    private readonly SKPaint _textPaint = new() { IsAntialias = false };

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new <see cref="TuiRenderContext"/> with the specified
    /// grid metrics, font and theme.
    /// </summary>
    /// <param name="grid">The character grid metrics for this session.</param>
    /// <param name="font">The loaded framework font.</param>
    /// <param name="theme">The active visual theme.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any argument is <see langword="null"/>.
    /// </exception>
    public TuiRenderContext(TuiGrid grid, TuiFont font, TuiTheme theme)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(theme);

        Grid = grid;
        Font = font;
        Theme = theme;
    }

    // ── Public properties ─────────────────────────────────────────────────────

    /// <summary>Gets the character grid metrics for the current session.</summary>
    public TuiGrid Grid { get; }

    /// <summary>Gets the loaded framework font.</summary>
    public TuiFont Font { get; }

    /// <summary>Gets the active visual theme.</summary>
    public TuiTheme Theme { get; }

    // ── Internal frame lifecycle (NOT part of the public API) ─────────────────

    /// <summary>
    /// Prepares the context for a new render pass against the supplied canvas.
    /// </summary>
    /// <remarks>
    /// Called by <c>TuiApplication</c> at the start of every frame.
    /// Not visible outside the <c>Retro.TUI.Rendering</c> assembly.
    /// </remarks>
    internal void BeginFrame(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        _canvas = canvas;
    }

    /// <summary>
    /// Flushes all pending drawing operations to the canvas.
    /// </summary>
    /// <remarks>
    /// Called by <c>TuiApplication</c> after the view tree has finished drawing.
    /// Not visible outside the <c>Retro.TUI.Rendering</c> assembly.
    /// </remarks>
    internal void EndFrame()
    {
        RequireCanvas();
        _canvas!.Flush();
        _canvas = null;
    }

    // ── Background ────────────────────────────────────────────────────────────

    /// <summary>
    /// Fills a single cell with the specified background color role.
    /// </summary>
    /// <param name="col">Zero-based column index of the cell.</param>
    /// <param name="row">Zero-based row index of the cell.</param>
    /// <param name="bg">The background color role to apply.</param>
    public void FillCell(int col, int row, TuiColorRole bg)
    {
        RequireCanvas();
        _fillPaint.Color = ResolveColor(bg);
        _canvas!.DrawRect(Grid.PixelX(col), Grid.PixelY(row),
                          Grid.CellWidth, Grid.CellHeight, _fillPaint);
    }

    /// <summary>
    /// Fills an entire row with the specified background color role.
    /// </summary>
    /// <param name="row">Zero-based row index.</param>
    /// <param name="bg">The background color role to apply.</param>
    public void FillRow(int row, TuiColorRole bg)
    {
        RequireCanvas();
        _fillPaint.Color = ResolveColor(bg);
        _canvas!.DrawRect(0f, Grid.PixelY(row),
                          Grid.ScreenWidth, Grid.CellHeight, _fillPaint);
    }

    /// <summary>
    /// Fills a rectangular region with the specified background color role.
    /// </summary>
    /// <param name="col">Left edge in grid columns.</param>
    /// <param name="row">Top edge in grid rows.</param>
    /// <param name="width">Width in grid columns.</param>
    /// <param name="height">Height in grid rows.</param>
    /// <param name="bg">The background color role to apply.</param>
    public void FillRect(int col, int row, int width, int height, TuiColorRole bg)
    {
        RequireCanvas();
        _fillPaint.Color = ResolveColor(bg);
        DrawPixelRect(col, row, width, height);
    }

    /// <summary>
    /// Fills a rectangular region with an explicit <see cref="SKColor"/>.
    /// </summary>
    /// <param name="col">Left edge in grid columns.</param>
    /// <param name="row">Top edge in grid rows.</param>
    /// <param name="width">Width in grid columns.</param>
    /// <param name="height">Height in grid rows.</param>
    /// <param name="color">The explicit color to fill with.</param>
    /// <remarks>
    /// Use this overload sparingly — prefer role-based colors for theme compatibility.
    /// </remarks>
    public void FillRect(int col, int row, int width, int height, SKColor color)
    {
        RequireCanvas();
        _fillPaint.Color = color;
        DrawPixelRect(col, row, width, height);
    }

    // ── Text ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a string starting at the given cell, with explicit foreground and
    /// background color roles.
    /// </summary>
    /// <param name="col">Left edge in grid columns.</param>
    /// <param name="row">Row in grid rows.</param>
    /// <param name="text">The string to draw. Must not be <see langword="null"/>.</param>
    /// <param name="fg">Foreground (glyph) color role.</param>
    /// <param name="bg">Background (cell fill) color role.</param>
    public void DrawText(int col, int row, string text, TuiColorRole fg, TuiColorRole bg)
    {
        RequireCanvas();
        ArgumentNullException.ThrowIfNull(text);

        // Fill background for the exact character run width.
        int runWidth = Math.Min(text.Length, Grid.Columns - col);
        _fillPaint.Color = ResolveColor(bg);
        DrawPixelRect(col, row, runWidth, 1);

        // Draw glyphs.
        _textPaint.Color = ResolveColor(fg);
        _canvas!.DrawText(text, Grid.PixelX(col), Grid.BaselineY(row),
                          Font.SkFont, _textPaint);
    }

    /// <summary>
    /// Draws a string centered within a fixed-width region.
    /// </summary>
    /// <param name="col">Left edge of the region in grid columns.</param>
    /// <param name="row">Row in grid rows.</param>
    /// <param name="width">Region width in grid columns.</param>
    /// <param name="text">The string to center. Must not be <see langword="null"/>.</param>
    /// <param name="fg">Foreground color role.</param>
    /// <param name="bg">Background color role.</param>
    public void DrawTextCentered(int col, int row, int width, string text,
                                 TuiColorRole fg, TuiColorRole bg)
    {
        ArgumentNullException.ThrowIfNull(text);

        int textLen = Math.Min(text.Length, width);
        int padding = (width - textLen) / 2;
        string padded = text.PadLeft(padding + textLen).PadRight(width);
        DrawText(col, row, padded[..width], fg, bg);
    }

    /// <summary>
    /// Draws a string clipped to a maximum width, truncating with an ellipsis
    /// character if the text is longer than <paramref name="maxWidth"/>.
    /// </summary>
    /// <param name="col">Left edge in grid columns.</param>
    /// <param name="row">Row in grid rows.</param>
    /// <param name="maxWidth">Maximum number of character cells to use.</param>
    /// <param name="text">The string to draw. Must not be <see langword="null"/>.</param>
    /// <param name="fg">Foreground color role.</param>
    /// <param name="bg">Background color role.</param>
    public void DrawTextClipped(int col, int row, int maxWidth, string text,
                                TuiColorRole fg, TuiColorRole bg)
    {
        ArgumentNullException.ThrowIfNull(text);

        string clipped = text.Length <= maxWidth
            ? text
            : string.Concat(text.AsSpan(0, maxWidth - 1), "\u2026"); // horizontal ellipsis

        DrawText(col, row, clipped, fg, bg);
    }

    // ── Borders ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the PC Tools 9.x window border: a 2 px geometric line on the
    /// left edge and a 2 px geometric line on the bottom edge.
    /// </summary>
    /// <param name="col">Left edge of the window in grid columns.</param>
    /// <param name="row">Top edge of the window in grid rows.</param>
    /// <param name="width">Window width in grid columns.</param>
    /// <param name="height">Window height in grid rows.</param>
    /// <param name="border">Border color role.</param>
    /// <remarks>
    /// The top edge is not drawn here — it is rendered by <c>TuiWindow</c>
    /// as the title bar. The right edge is not drawn per the PC Tools 9.x
    /// visual reference. The system-menu icon in the top-left corner is also
    /// the responsibility of <c>TuiWindow.Draw()</c>.
    /// </remarks>
    public void DrawBorder(int col, int row, int width, int height,
                           TuiColorRole border)
    {
        RequireCanvas();

        const float StrokeWidth = 2f;

        using var paint = new SKPaint
        {
            Color = ResolveColor(border),
            StrokeWidth = StrokeWidth,
            IsStroke = true,
            IsAntialias = false,
        };

        float x0 = Grid.PixelX(col);
        float y0 = Grid.PixelY(row);
        float x1 = Grid.PixelX(col + width);
        float y1 = Grid.PixelY(row + height);

        // Left edge — vertical line, full window height.
        _canvas!.DrawLine(x0, y0, x0, y1, paint);

        // Bottom edge — horizontal line, full window width.
        _canvas.DrawLine(x0, y1, x1, y1, paint);
    }

    // ── Shadow ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a drop shadow offset by <see cref="TuiTheme.ShadowOffsetX"/> and
    /// <see cref="TuiTheme.ShadowOffsetY"/> cells from the given rectangle,
    /// using the <see cref="TuiColorRole.WindowShadow"/> palette color at
    /// <see cref="TuiTheme.ShadowOpacity"/>.
    /// </summary>
    /// <param name="col">Left edge of the casting element in grid columns.</param>
    /// <param name="row">Top edge of the casting element in grid rows.</param>
    /// <param name="width">Width of the casting element in grid columns.</param>
    /// <param name="height">Height of the casting element in grid rows.</param>
    public void DrawShadow(int col, int row, int width, int height)
    {
        RequireCanvas();

        SKColor baseColor = ResolveColor(TuiColorRole.WindowShadow);
        byte alpha = (byte)(255 * Math.Clamp(Theme.ShadowOpacity, 0f, 1f));
        SKColor shadowColor = baseColor.WithAlpha(alpha);

        int sx = col + Theme.ShadowOffsetX;
        int sy = row + Theme.ShadowOffsetY;

        // Bottom strip
        _fillPaint.Color = shadowColor;
        _canvas!.DrawRect(
            Grid.PixelX(sx),
            Grid.PixelY(sy + height - Theme.ShadowOffsetY),
            width * Grid.CellWidth,
            Theme.ShadowOffsetY * Grid.CellHeight,
            _fillPaint);

        // Right strip
        _canvas.DrawRect(
            Grid.PixelX(sx + width - Theme.ShadowOffsetX),
            Grid.PixelY(sy),
            Theme.ShadowOffsetX * Grid.CellWidth,
            height * Grid.CellHeight,
            _fillPaint);
    }

    // ── Desktop pattern ───────────────────────────────────────────────────────

    /// <summary>
    /// Draws the full-screen desktop background pattern as defined by
    /// <paramref name="pattern"/>.
    /// </summary>
    /// <param name="pattern">
    /// The pattern style. <see cref="TuiDesktopPattern.None"/> fills with
    /// a solid <see cref="TuiColorRole.DesktopBackground"/> color only.
    /// </param>
    public void DrawDesktopPattern(TuiDesktopPattern pattern)
    {
        RequireCanvas();

        // Always fill the solid background first.
        _fillPaint.Color = ResolveColor(TuiColorRole.DesktopBackground);
        _canvas!.DrawRect(0, 0, Grid.ScreenWidth, Grid.ScreenHeight, _fillPaint);

        if (pattern == TuiDesktopPattern.None)
            return;

        SKColor dotColor = ResolveColor(TuiColorRole.DesktopPatternDot);

        switch (pattern)
        {
            case TuiDesktopPattern.DotGrid:
                DrawPatternDotGrid(dotColor);
                break;

            case TuiDesktopPattern.Checkerboard:
                DrawPatternCheckerboard(dotColor);
                break;

            case TuiDesktopPattern.HorizontalLines:
                DrawPatternHorizontalLines(dotColor);
                break;

            case TuiDesktopPattern.VerticalLines:
                DrawPatternVerticalLines(dotColor);
                break;
        }
    }

    // ── Clipping ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Pushes a clip rectangle in grid coordinates onto the canvas clip stack.
    /// Restore the previous clip region by calling <see cref="PopClip"/>.
    /// </summary>
    /// <param name="col">Left edge in grid columns.</param>
    /// <param name="row">Top edge in grid rows.</param>
    /// <param name="width">Width in grid columns.</param>
    /// <param name="height">Height in grid rows.</param>
    /// <remarks>
    /// Calls to <see cref="PushClip"/> and <see cref="PopClip"/> must be balanced.
    /// Each <see cref="PushClip"/> saves the current canvas state; each
    /// <see cref="PopClip"/> restores it.
    /// </remarks>
    public void PushClip(int col, int row, int width, int height)
    {
        RequireCanvas();
        _canvas!.Save();
        _canvas.ClipRect(new SKRect(
            Grid.PixelX(col),
            Grid.PixelY(row),
            Grid.PixelX(col + width),
            Grid.PixelY(row + height)));
    }

    /// <summary>
    /// Pops the clip rectangle previously pushed by <see cref="PushClip"/>,
    /// restoring the canvas state to what it was before that call.
    /// </summary>
    public void PopClip()
    {
        RequireCanvas();
        _canvas!.Restore();
    }

    // ── Custom cursor ─────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a blinking text cursor at the given pixel position.
    /// </summary>
    /// <param name="pixelX">X position in physical pixels.</param>
    /// <param name="pixelY">Y position in physical pixels (top of the cell).</param>
    /// <remarks>
    /// The cursor is rendered as a solid one-cell rectangle using the
    /// <see cref="TuiColorRole.InputForeground"/> color. Blink logic is handled
    /// by the caller (typically <c>TuiInputLine</c>), which simply omits this call
    /// during the "off" phase of the blink cycle.
    /// </remarks>
    public void DrawCursor(float pixelX, float pixelY)
    {
        RequireCanvas();
        _fillPaint.Color = ResolveColor(TuiColorRole.InputForeground);
        _canvas!.DrawRect(pixelX, pixelY, Grid.CellWidth, Grid.CellHeight, _fillPaint);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Throws <see cref="InvalidOperationException"/> if no frame is active.</summary>
    private void RequireCanvas()
    {
        if (_canvas is null)
            throw new InvalidOperationException(
                "No active frame. TuiRenderContext.BeginFrame must be called before any drawing operation.");
    }

    /// <summary>
    /// Resolves a <see cref="TuiColorRole"/> to its concrete <see cref="SKColor"/>
    /// using the active theme palette.
    /// </summary>
    private SKColor ResolveColor(TuiColorRole role)
        => Theme.Palette.GetOrDefault(role);

    /// <summary>
    /// Fills a rectangle specified in grid coordinates using <see cref="_fillPaint"/>.
    /// The caller is responsible for setting <c>_fillPaint.Color</c> beforehand.
    /// </summary>
    private void DrawPixelRect(int col, int row, int width, int height)
        => _canvas!.DrawRect(
            Grid.PixelX(col),
            Grid.PixelY(row),
            width * Grid.CellWidth,
            height * Grid.CellHeight,
            _fillPaint);

    // ── Desktop pattern helpers ───────────────────────────────────────────────

    private void DrawPatternDotGrid(SKColor color)
    {
        // Place one dot per cell at the center of each grid cell.
        // Matches the classic Norton Commander / PC Tools dot-grid desktop.
        _fillPaint.Color = color;
        float dotSize = Math.Max(1f, Grid.CellWidth * 0.15f);
        float dotOffX = (Grid.CellWidth - dotSize) * 0.5f;
        float dotOffY = (Grid.CellHeight - dotSize) * 0.5f;

        for (int r = 0; r < Grid.Rows; r++)
            for (int c = 0; c < Grid.Columns; c++)
                _canvas!.DrawRect(
                    Grid.PixelX(c) + dotOffX,
                    Grid.PixelY(r) + dotOffY,
                    dotSize, dotSize,
                    _fillPaint);
    }

    private void DrawPatternCheckerboard(SKColor color)
    {
        _fillPaint.Color = color;
        for (int r = 0; r < Grid.Rows; r++)
            for (int c = 0; c < Grid.Columns; c++)
                if ((r + c) % 2 == 0)
                    DrawPixelRect(c, r, 1, 1);
    }

    private void DrawPatternHorizontalLines(SKColor color)
    {
        _fillPaint.Color = color;
        for (int r = 0; r < Grid.Rows; r += 2)
        {
            _canvas!.DrawRect(0f, Grid.PixelY(r) + Grid.CellHeight - 1f,
                              Grid.ScreenWidth, 1f, _fillPaint);
        }
    }

    private void DrawPatternVerticalLines(SKColor color)
    {
        _fillPaint.Color = color;
        for (int c = 0; c < Grid.Columns; c += 2)
        {
            _canvas!.DrawRect(Grid.PixelX(c) + Grid.CellWidth - 1f, 0f,
                              1f, Grid.ScreenHeight, _fillPaint);
        }
    }
    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <summary>
    /// Releases the <see cref="SKPaint"/> instances owned by this context.
    /// </summary>
    /// <remarks>
    /// The paints are reused across every frame to avoid per-frame allocations.
    /// Dispose this context when the application shuts down.
    /// </remarks>
    public void Dispose()
    {
        _fillPaint.Dispose();
        _textPaint.Dispose();
    }
}
