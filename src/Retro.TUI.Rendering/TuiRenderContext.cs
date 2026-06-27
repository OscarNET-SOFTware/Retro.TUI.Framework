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

    /// <summary>
    /// Lazily-built bitmap for <see cref="DrawMouseCursor"/>, rendered once using
    /// the <see cref="TuiColorRole.MouseCursorFill"/> and
    /// <see cref="TuiColorRole.MouseCursorOutline"/> colors resolved at first draw.
    /// </summary>
    /// <remarks>
    /// TODO: <c>_cursorBitmap</c> is cached with colors resolved at first draw.
    /// If <c>CurrentTheme</c> changes at runtime, the cursor will retain stale
    /// colors until this <see cref="TuiRenderContext"/> is recreated. Revisit if
    /// hot theme-switching becomes a supported scenario.
    /// </remarks>
    private SKBitmap? _cursorBitmap;

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

    // ── Frame execution ──────────────────────────────────────────────────────────

    /// <summary>
    /// Acquires the canvas from <paramref name="surface"/>, executes
    /// <paramref name="drawCallback"/> inside a matched
    /// <see cref="BeginFrame"/> / <see cref="EndFrame"/> pair, then flushes.
    /// </summary>
    /// <param name="surface">
    /// The <see cref="SkiaSharp.SKSurface"/> provided by the host for the current frame.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="drawCallback">
    /// The delegate that draws the entire view tree onto this context.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// This method is the <em>only</em> intended caller of the <c>internal</c>
    /// <see cref="BeginFrame"/> and <see cref="EndFrame"/> methods.
    /// <c>Retro.TUI.Core</c> calls it from <c>TuiMessageLoop</c>, which keeps
    /// <c>BeginFrame</c> and <c>EndFrame</c> invisible outside this assembly while
    /// still allowing the message loop to control the render cycle.
    /// <para/>
    /// The <c>try/finally</c> guarantees that <see cref="EndFrame"/> is always
    /// called even when <paramref name="drawCallback"/> throws, preventing the
    /// canvas from being left in an active-frame state on the next iteration.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="surface"/> or <paramref name="drawCallback"/>
    /// is <see langword="null"/>.
    /// </exception>
    public void RenderFrame(SkiaSharp.SKSurface surface, Action<TuiRenderContext> drawCallback)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(drawCallback);

        BeginFrame(surface.Canvas);
        try
        {
            drawCallback(this);
        }
        finally
        {
            EndFrame();
        }
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

    // ── System-menu glyph ─────────────────────────────────────────────────────

    /// <summary>
    /// Draws the system-menu glyph (the <c>[-]</c> icon) at the given grid cell.
    /// </summary>
    /// <param name="col">Grid column of the cell.</param>
    /// <param name="row">Grid row of the cell.</param>
    /// <param name="fg">Color role for the outer border and the dash stroke.</param>
    /// <param name="bg">Color role for the inner fill of the glyph.</param>
    /// <remarks>
    /// The glyph is drawn entirely with geometric primitives — it does not rely on
    /// any font glyph — and scales proportionally to <see cref="TuiGrid.CellWidth"/>
    /// / <see cref="TuiGrid.CellHeight"/> so it remains correct if the font size
    /// changes in a future theme.
    /// <para/>
    /// Pixel layout (reference: IBM VGA 9×16 cell):
    /// <list type="bullet">
    ///   <item><description>
    ///     Outer border: 1 px stroke rectangle covering the full cell.
    ///   </description></item>
    ///   <item><description>
    ///     Inner fill: <paramref name="bg"/>-colored rectangle inset by 1 px on all sides.
    ///   </description></item>
    ///   <item><description>
    ///     Dash: <paramref name="fg"/>-colored filled rectangle,
    ///     width = <c>CellWidth - 4 px</c> (2 px margin left + right),
    ///     height ≈ <c>CellHeight × 3/16</c> (3 px at 16 px cell height),
    ///     positioned 2 px from the left and 5 px from the top of the cell.
    ///   </description></item>
    /// </list>
    /// </remarks>
    public void DrawSystemMenuGlyph(int col, int row, TuiColorRole fg, TuiColorRole bg)
    {
        RequireCanvas();

        float x = Grid.PixelX(col);
        float y = Grid.PixelY(row);
        float w = Grid.CellWidth;
        float h = Grid.CellHeight;

        SKColor fgColor = ResolveColor(fg);
        SKColor bgColor = ResolveColor(bg);

        // ── Outer border — fill full cell with fg color ───────────────────────
        // Using a filled rect instead of a stroked rect: SKPaint strokes are
        // centered on the path, which expands the drawn area by StrokeWidth/2
        // on each side and produces a 10×17 px result on a 9×16 cell.
        _fillPaint.Color = fgColor;
        _canvas!.DrawRect(new SKRect(x, y, x + h, y + h), _fillPaint);

        // ── Inner fill (bg color, inset 1 px on each side) ───────────────────
        _fillPaint.Color = bgColor;
        _canvas.DrawRect(new SKRect(x + 1f, y + 1f, x + h - 1f, y + h - 1f),
                         _fillPaint);

        // ── Dash (fg color, fixed pixel offsets within 16×16 glyph) ──────────────
        // Left=3px, top=6px, width=CellHeight-6px (10px at 16px), height=3px.
        float dashX = x + 3f;
        float dashY = y + 6f;
        float dashWidth = h - 6f;
        float dashHeight = 3f;

        _fillPaint.Color = fgColor;
        _canvas.DrawRect(new SKRect(dashX, dashY,
                                    dashX + dashWidth, dashY + dashHeight),
                         _fillPaint);
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

        const float StrokeWidth = 1f;

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
        // Use y1-1 so the 2px stroke renders fully within the clipped area
        // instead of being cut at the pixel boundary.
        _canvas.DrawLine(x0, y1 - 1f, x1, y1 - 1f, paint);
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

        // Convert element bounds to pixels.
        float px = Grid.PixelX(col);
        float py = Grid.PixelY(row);
        float pw = width * Grid.CellWidth;
        float ph = height * Grid.CellHeight;

        // Shadow offsets are in pixels (not grid cells).
        float ox = Theme.ShadowOffsetX;
        float oy = Theme.ShadowOffsetY;

        _fillPaint.Color = shadowColor;

        // Right strip — full height
        _canvas!.DrawRect(px + pw, py + oy, ox, ph, _fillPaint);

        // Bottom strip — starts after the shadow offset to avoid corner overlap
        _canvas.DrawRect(px + ox, py + ph, pw - ox, oy, _fillPaint);
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

    /// <summary>
    /// Draws the custom mouse pointer at the given screen-pixel position.
    /// </summary>
    /// <param name="pixelX">X position of the pointer hot-spot, in physical pixels.</param>
    /// <param name="pixelY">Y position of the pointer hot-spot, in physical pixels.</param>
    /// <remarks>
    /// Renders a 12×19 pixel-art arrow bitmap, replacing the host's system cursor
    /// (hidden via <c>TuiHostOptions.HideSystemCursor</c>). The bitmap is built once
    /// on first use and cached for subsequent frames — see
    /// <see cref="BuildCursorBitmap"/>.
    /// <para/>
    /// Unlike <see cref="DrawCursor"/> (a one-cell text caret), this method draws in
    /// screen-pixel coordinates and does not snap to the character grid: the pointer
    /// follows the mouse smoothly, matching the PC Tools 9.x visual reference.
    /// <para/>
    /// The caller is responsible for drawing this <em>last</em>, after the entire
    /// view tree, so the pointer always renders on top.
    /// </remarks>
    public void DrawMouseCursor(float pixelX, float pixelY)
    {
        RequireCanvas();

        SKBitmap bitmap = _cursorBitmap ??= BuildCursorBitmap();

        _canvas!.DrawBitmap(bitmap, pixelX, pixelY);
    }

    /// <summary>
    /// Builds the 12×19 pixel-art mouse pointer bitmap using the active theme's
    /// <see cref="TuiColorRole.MouseCursorFill"/> and
    /// <see cref="TuiColorRole.MouseCursorOutline"/> colors.
    /// </summary>
    /// <remarks>
    /// Pixel values: <c>0</c> = transparent, <c>1</c> = fill, <c>2</c> = outline.
    /// Called once and cached in <see cref="_cursorBitmap"/>.
    /// </remarks>
    private SKBitmap BuildCursorBitmap()
    {
        // 0 = transparent, 1 = fill, 2 = outline.
        ReadOnlySpan<byte> pixels =
        [
            2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0,
            2, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0,
            2, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0,
            2, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0,
            2, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0,
            2, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0, 0,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 0,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0,
            2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 2,
            2, 1, 1, 2, 2, 1, 1, 2, 0, 2, 2, 2,
            2, 1, 1, 2, 2, 1, 1, 2, 0, 0, 0, 0,
            2, 1, 2, 0, 2, 1, 1, 1, 2, 0, 0, 0,
            2, 2, 2, 0, 0, 2, 1, 1, 2, 0, 0, 0,
            0, 0, 0, 0, 0, 2, 1, 1, 1, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0,
        ];

        const int width = 12;
        const int height = 19;

        SKColor fill = ResolveColor(TuiColorRole.MouseCursorFill);
        SKColor outline = ResolveColor(TuiColorRole.MouseCursorOutline);
        SKColor transparent = SKColors.Transparent;

        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte value = pixels[y * width + x];
                SKColor color = value switch
                {
                    1 => fill,
                    2 => outline,
                    _ => transparent,
                };

                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
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
        _cursorBitmap?.Dispose();
    }
}
