// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RenderingContractTests.cs" company="OscarNET-SOFTware">
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

// ════════════════════════════════════════════════════════════════════════════
// TuiGrid — pixel / cell conversion contracts
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Verifies <see cref="TuiGrid"/> initialization, guard clauses and
/// coordinate-conversion contracts.
/// These tests do not require SDL2, SkiaSharp GPU context, or any real window.
/// </summary>
public sealed class TuiGridTests
{
    // ── Initialize guard clauses ──────────────────────────────────────────

    [Fact]
    public void Initialize_NullFont_ThrowsArgumentNullException()
    {
        var grid = new TuiGrid();
        Assert.Throws<ArgumentNullException>(() =>
            grid.Initialize(800, 600, null!));
    }

    [Fact]
    public void Initialize_ZeroWidth_ThrowsArgumentOutOfRangeException()
    {
        var grid = new TuiGrid();
        using var font = new SKFont();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            grid.Initialize(0, 600, font));
    }

    [Fact]
    public void Initialize_NegativeHeight_ThrowsArgumentOutOfRangeException()
    {
        var grid = new TuiGrid();
        using var font = new SKFont();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            grid.Initialize(800, -1, font));
    }

    // ── Initialization results ────────────────────────────────────────────

    [Fact]
    public void Initialize_ValidArgs_SetsPositiveCellDimensions()
    {
        var grid = CreateInitializedGrid(800, 480);

        Assert.True(grid.CellWidth > 0, "CellWidth must be positive after Initialize.");
        Assert.True(grid.CellHeight > 0, "CellHeight must be positive after Initialize.");
    }

    [Fact]
    public void Initialize_ValidArgs_SetsExpectedScreenDimensions()
    {
        var grid = CreateInitializedGrid(1024, 768);

        Assert.Equal(1024, grid.ScreenWidth);
        Assert.Equal(768, grid.ScreenHeight);
    }

    [Fact]
    public void Initialize_ValidArgs_SetsNonZeroColumnsAndRows()
    {
        var grid = CreateInitializedGrid(800, 600);

        Assert.True(grid.Columns > 0, "Columns must be positive after Initialize.");
        Assert.True(grid.Rows > 0, "Rows must be positive after Initialize.");
    }

    [Fact]
    public void Initialize_CalledTwice_IsIdempotent()
    {
        var grid = CreateInitializedGrid(800, 600);

        int expectedCols = grid.Columns;
        int expectedRows = grid.Rows;

        // Second call must be silently ignored.
        using var font2 = new SKFont();
        grid.Initialize(1920, 1080, font2);

        Assert.Equal(expectedCols, grid.Columns);
        Assert.Equal(expectedRows, grid.Rows);
    }

    // ── Cell → pixel conversion ───────────────────────────────────────────

    [Fact]
    public void PixelX_Col0_ReturnsZero()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.Equal(0f, grid.PixelX(0));
    }

    [Fact]
    public void PixelX_ColN_ReturnsN_Times_CellWidth()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.Equal(grid.CellWidth * 3f, grid.PixelX(3));
    }

    [Fact]
    public void PixelY_Row0_ReturnsZero()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.Equal(0f, grid.PixelY(0));
    }

    [Fact]
    public void PixelY_RowN_ReturnsN_Times_CellHeight()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.Equal(grid.CellHeight * 5f, grid.PixelY(5));
    }

    [Fact]
    public void BaselineY_Row0_IsGreaterThanOrEqualToZero()
    {
        var grid = CreateInitializedGrid(800, 480);
        // Baseline is ascent pixels below the top of row 0.
        Assert.True(grid.BaselineY(0) >= 0f);
    }

    [Fact]
    public void BaselineY_Increases_WithRow()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.True(grid.BaselineY(1) > grid.BaselineY(0));
    }

    // ── Pixel → cell conversion ───────────────────────────────────────────

    [Fact]
    public void CellCol_PixelZero_ReturnsColumnZero()
    {
        var grid = CreateInitializedGrid(800, 480);
        Assert.Equal(0, grid.CellCol(0f));
    }

    [Fact]
    public void CellCol_RoundTrip_IsConsistentWithPixelX()
    {
        var grid = CreateInitializedGrid(800, 480);

        for (int c = 0; c < grid.Columns; c++)
            Assert.Equal(c, grid.CellCol(grid.PixelX(c)));
    }

    [Fact]
    public void CellRow_RoundTrip_IsConsistentWithPixelY()
    {
        var grid = CreateInitializedGrid(800, 480);

        for (int r = 0; r < grid.Rows; r++)
            Assert.Equal(r, grid.CellRow(grid.PixelY(r)));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static TuiGrid CreateInitializedGrid(int screenW, int screenH)
    {
        // Use a default SKFont (system fallback). Cell dimensions will be
        // non-zero for any valid system typeface, which is all we need here.
        using var font = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(screenW, screenH, font);
        return grid;
    }
}

// ════════════════════════════════════════════════════════════════════════════
// TuiFont — guard clauses and load-failure contracts
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Verifies <see cref="TuiFont"/> guard clauses and fail-fast behavior.
/// These tests do not require a real theme font to be registered.
/// </summary>
public sealed class TuiFontTests
{
    // ── Load guard clauses ────────────────────────────────────────────────

    [Fact]
    public void Load_NullTypeface_ThrowsArgumentNullException()
    {
        using var font = new TuiFont();
        Assert.Throws<ArgumentNullException>(() => font.Load(null!, 16f));
    }

    [Fact]
    public void Load_ZeroSize_ThrowsArgumentException()
    {
        using var font = new TuiFont();
        Assert.Throws<ArgumentException>(() => font.Load(SKTypeface.Default, 0f));
    }

    [Fact]
    public void Load_NegativeSize_ThrowsArgumentException()
    {
        using var font = new TuiFont();
        Assert.Throws<ArgumentException>(() => font.Load(SKTypeface.Default, -1f));
    }

    [Fact]
    public void Load_ValidArgs_DoesNotThrow()
    {
        using var font = new TuiFont();
        var ex = Record.Exception(() => font.Load(SKTypeface.Default, 16f));
        Assert.Null(ex);
    }

    [Fact]
    public void Load_ValidArgs_ExposesNonNullSkFont()
    {
        using var font = new TuiFont();
        font.Load(SKTypeface.Default, 16f);
        Assert.NotNull(font.SkFont);
    }

    // ── Disposal contract ─────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var font = new TuiFont();
        font.Dispose();

        var ex = Record.Exception(() => font.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Load_AfterDispose_ThrowsObjectDisposedException()
    {
        var font = new TuiFont();
        font.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
            font.Load(SKTypeface.Default, 16f));
    }
}

// ════════════════════════════════════════════════════════════════════════════
// TuiRenderContext — construction guard clauses and frame-lifecycle contract
// ════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Verifies <see cref="TuiRenderContext"/> guard clauses, frame-lifecycle
/// enforcement and that drawing methods accept valid inputs without throwing.
/// These tests use an in-memory <see cref="SKSurface"/> — no SDL2 or GPU required.
/// </summary>
public sealed class TuiRenderContextTests
{
    // ── Construction guard clauses ────────────────────────────────────────

    [Fact]
    public void Constructor_NullGrid_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TuiRenderContext(null!, new TuiFont()));
    }

    [Fact]
    public void Constructor_NullFont_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TuiRenderContext(new TuiGrid(), null!));
    }

    // ── Frame lifecycle ───────────────────────────────────────────────────

    [Fact]
    public void DrawMethod_BeforeBeginFrame_ThrowsInvalidOperationException()
    {
        var ctx = BuildContext(80, 25);

        // Any draw call before BeginFrame must throw.
        Assert.Throws<InvalidOperationException>(() =>
            ctx.FillRow(0, TuiColorRole.DesktopBackground));
    }

    [Fact]
    public void BeginFrame_NullCanvas_ThrowsArgumentNullException()
    {
        var ctx = BuildContext(80, 25);
        Assert.Throws<ArgumentNullException>(() => ctx.BeginFrame(null!));
    }

    // ── Drawing methods — accept valid inputs without throwing ────────────

    [Fact]
    public void FillCell_InsideGrid_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.FillCell(0, 0, TuiColorRole.DesktopBackground));

    [Fact]
    public void FillRow_ValidRow_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.FillRow(0, TuiColorRole.DesktopBackground));

    [Fact]
    public void FillRect_ValidRegion_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.FillRect(0, 0, 4, 2, TuiColorRole.DesktopBackground));

    [Fact]
    public void FillRect_ExplicitColor_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.FillRect(0, 0, 2, 1, SKColors.Red));

    [Fact]
    public void DrawText_ValidArgs_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx =>
            ctx.DrawText(0, 0, "Hello", TuiColorRole.AppTitleForeground, TuiColorRole.AppTitleBackground));

    [Fact]
    public void DrawText_NullText_ThrowsArgumentNullException()
        => AssertThrowsDuringDraw<ArgumentNullException>(ctx =>
            ctx.DrawText(0, 0, null!, TuiColorRole.AppTitleForeground, TuiColorRole.AppTitleBackground));

    [Fact]
    public void DrawTextCentered_ValidArgs_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx =>
            ctx.DrawTextCentered(0, 0, 10, "Hi", TuiColorRole.AppTitleForeground, TuiColorRole.AppTitleBackground));

    [Fact]
    public void DrawTextClipped_ShortText_DoesNotTruncate()
    {
        // Text shorter than maxWidth must be drawn unchanged (no ellipsis appended).
        // We can only verify no exception is thrown here (no pixel inspection in CI).
        AssertDrawDoesNotThrow(ctx =>
            ctx.DrawTextClipped(0, 0, 20, "Short", TuiColorRole.WindowForeground, TuiColorRole.WindowBackground));
    }

    [Fact]
    public void DrawBorder_ValidArgs_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx =>
            ctx.DrawBorder(0, 0, 20, 10, TuiColorRole.WindowBorder));

    [Fact]
    public void DrawBorder_MinimumSize_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx =>
            ctx.DrawBorder(0, 0, 1, 1, TuiColorRole.WindowBorder));

    [Fact]
    public void DrawShadow_ValidArgs_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.DrawShadow(1, 1, 10, 5));

    [Fact]
    public void PushClip_And_PopClip_DoNotThrow()
        => AssertDrawDoesNotThrow(ctx =>
        {
            ctx.PushClip(0, 0, 10, 5);
            ctx.PopClip();
        });

    [Fact]
    public void DrawCursor_ValidPosition_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.DrawCursor(0f, 0f));

    // ── Mouse cursor ─────────────────────────────────────────────────────

    [Fact]
    public void DrawMouseCursor_ValidPosition_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.DrawMouseCursor(0f, 0f));

    [Fact]
    public void DrawMouseCursor_NonZeroPosition_DoesNotThrow()
        => AssertDrawDoesNotThrow(ctx => ctx.DrawMouseCursor(123.5f, 47.0f));

    [Fact]
    public void DrawMouseCursor_CalledTwice_ReusesCachedBitmap()
    {
        // Calling twice exercises the lazy-init branch (first call) and the
        // cached-bitmap branch (second call) without throwing either time.
        using var surface = CreateSurface(800, 480);
        var ctx = BuildContext(80, 25);

        ctx.BeginFrame(surface.Canvas);
        var ex1 = Record.Exception(() => ctx.DrawMouseCursor(0f, 0f));
        var ex2 = Record.Exception(() => ctx.DrawMouseCursor(10f, 10f));
        ctx.EndFrame();

        Assert.Null(ex1);
        Assert.Null(ex2);
    }

    [Fact]
    public void DrawMouseCursor_BeforeBeginFrame_ThrowsInvalidOperationException()
    {
        var ctx = BuildContext(80, 25);

        Assert.Throws<InvalidOperationException>(() => ctx.DrawMouseCursor(0f, 0f));
    }

    [Fact]
    public void Dispose_AfterDrawMouseCursor_DoesNotThrow()
    {
        // Ensures the cached _cursorBitmap is disposed cleanly.
        using var surface = CreateSurface(800, 480);
        var ctx = BuildContext(80, 25);

        ctx.BeginFrame(surface.Canvas);
        ctx.DrawMouseCursor(0f, 0f);
        ctx.EndFrame();

        var ex = Record.Exception(ctx.Dispose);

        Assert.Null(ex);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="drawAction"/> inside a valid BeginFrame / EndFrame
    /// pair and asserts no exception is thrown.
    /// </summary>
    private static void AssertDrawDoesNotThrow(Action<TuiRenderContext> drawAction)
    {
        using var surface = CreateSurface(800, 480);
        var ctx = BuildContext(80, 25);

        ctx.BeginFrame(surface.Canvas);
        var ex = Record.Exception(() => drawAction(ctx));
        ctx.EndFrame();

        Assert.Null(ex);
    }

    /// <summary>
    /// Executes <paramref name="drawAction"/> inside a valid BeginFrame / EndFrame
    /// pair and asserts that an exception of type <typeparamref name="TException"/>
    /// is thrown.
    /// </summary>
    private static void AssertThrowsDuringDraw<TException>(Action<TuiRenderContext> drawAction)
        where TException : Exception
    {
        using var surface = CreateSurface(800, 480);
        var ctx = BuildContext(80, 25);

        ctx.BeginFrame(surface.Canvas);
        Assert.Throws<TException>(() => drawAction(ctx));
        ctx.EndFrame();
    }

    /// <summary>
    /// Creates an in-memory CPU-rasterized <see cref="SKSurface"/>.
    /// No GPU or SDL2 required.
    /// </summary>
    private static SKSurface CreateSurface(int w, int h)
        => SKSurface.Create(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));

    /// <summary>
    /// Builds a <see cref="TuiRenderContext"/> with an initialized <see cref="TuiGrid"/>
    /// and a minimal theme that covers all <see cref="TuiColorRole"/> values used in tests.
    /// </summary>
    private static TuiRenderContext BuildContext(int cols, int rows)
    {
        int screenW = cols * 9;
        int screenH = rows * 16;

        using var skFont = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(screenW, screenH, skFont);

        var font = new TuiFont();
        font.Load(SKTypeface.Default, 16f);

        return new TuiRenderContext(grid, font);
    }
}
