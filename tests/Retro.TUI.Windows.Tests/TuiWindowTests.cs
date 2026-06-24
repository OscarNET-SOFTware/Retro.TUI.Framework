// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiWindowTests.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Events;
using Retro.TUI.Rendering;
using Retro.TUI.Theming;
using Retro.TUI.Views;

using SkiaSharp;

namespace Retro.TUI.Windows;

/// <summary>
/// Tests for <see cref="TuiWindow"/>: construction, layout contract
/// (<see cref="TuiWindow.InnerCol"/>, <see cref="TuiWindow.InnerRow"/>,
/// <see cref="TuiWindow.InnerWidth"/>, <see cref="TuiWindow.InnerHeight"/>),
/// active-state detection and the <see cref="TuiWindow.Draw"/> contract.
/// </summary>
public sealed class TuiWindowTests
{
    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullTitle_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TuiWindow(null!, col: 0, row: 0, width: 20, height: 10));
    }

    [Fact]
    public void Constructor_SetsTitleAndBounds()
    {
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 30, height: 12);

        Assert.Equal("Hello", window.Title);
        Assert.Equal(5, window.Col);
        Assert.Equal(3, window.Row);
        Assert.Equal(30, window.Width);
        Assert.Equal(12, window.Height);
    }

    [Fact]
    public void Constructor_DefaultProperties_AreEnabled()
    {
        var window = new TuiWindow("Hello", 0, 0, 20, 10);

        Assert.True(window.Movable);
        Assert.True(window.ShowShadow);
        Assert.True(window.ShowTitle);
    }

    [Fact]
    public void TuiWindow_IsAssignableFrom_TuiGroup()
    {
        var window = new TuiWindow("Hello", 0, 0, 20, 10);

        Assert.IsType<TuiGroup>(window, exactMatch: false);
    }

    // ── Inner area layout ────────────────────────────────────────────────────

    [Fact]
    public void InnerCol_SkipsLeftBorder()
    {
        var window = new TuiWindow("Hello", col: 10, row: 5, width: 30, height: 12);

        Assert.Equal(window.AbsCol + 1, window.InnerCol);
    }

    [Fact]
    public void InnerRow_SkipsTitleBarAndMarginRow()
    {
        var window = new TuiWindow("Hello", col: 10, row: 5, width: 30, height: 12);

        Assert.Equal(window.AbsRow + 2, window.InnerRow);
    }

    [Fact]
    public void InnerWidth_ExcludesLeftBorderOnly()
    {
        var window = new TuiWindow("Hello", col: 0, row: 0, width: 30, height: 12);

        Assert.Equal(28, window.InnerWidth);
    }

    [Fact]
    public void InnerHeight_ExcludesTitleBarMarginAndBottomBorder()
    {
        var window = new TuiWindow("Hello", col: 0, row: 0, width: 30, height: 12);

        Assert.Equal(9, window.InnerHeight);
    }

    [Fact]
    public void InnerCoordinates_ReflectNestedAbsolutePosition()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 10, row: 5, width: 30, height: 12);
        desktop.Add(window);

        Assert.Equal(11, window.InnerCol);
        Assert.Equal(7, window.InnerRow);
    }

    // ── Active state ──────────────────────────────────────────────────────────

    [Fact]
    public void IsActive_NoParent_IsFalse()
    {
        var window = new TuiWindow("Hello", 0, 0, 20, 10);

        Assert.False(window.IsActive);
    }

    [Fact]
    public void IsActive_OnlyChildOfDesktop_IsTrue()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", 0, 0, 20, 10);
        desktop.Add(window);

        Assert.True(window.IsActive);
    }

    [Fact]
    public void IsActive_NotFrontmostChild_IsFalse()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var back = new TuiWindow("Back", 0, 0, 20, 10);
        var front = new TuiWindow("Front", 5, 5, 20, 10);
        desktop.Add(back);
        desktop.Add(front);

        Assert.False(back.IsActive);
        Assert.True(front.IsActive);
    }

    // ── Draw contract ─────────────────────────────────────────────────────────

    [Fact]
    public void Draw_NullContext_ThrowsArgumentNullException()
    {
        var window = new TuiWindow("Hello", 0, 0, 20, 10);

        Assert.Throws<ArgumentNullException>(() => window.Draw(null!));
    }

    [Fact]
    public void Draw_ActiveWindow_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var window = new TuiWindow("Hello", col: 2, row: 2, width: 20, height: 8);

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => window.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_InactiveWindow_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 40, Height = 20 };
        var back = new TuiWindow("Back", 0, 0, 20, 8);
        var front = new TuiWindow("Front", 5, 5, 20, 8);
        desktop.Add(back);
        desktop.Add(front);

        Assert.False(back.IsActive);

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => desktop.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_WithoutTitleAndShadow_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var window = new TuiWindow("Hello", col: 2, row: 2, width: 20, height: 8)
        {
            ShowTitle = false,
            ShowShadow = false,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => window.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_DrawsChildren()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var window = new TuiWindow("Hello", col: 2, row: 2, width: 20, height: 8);
        var child = new RecordingView { Col = 0, Row = 0, Width = 5, Height = 1 };
        window.Add(child);

        ctx.RenderFrame(surface, c => window.Draw(c));

        Assert.True(child.WasDrawn);
    }

    // ── HasCustomMouseHandling ────────────────────────────────────────────────

    [Fact]
    public void HasCustomMouseHandling_IsTrue()
    {
        var window = new TuiWindow("Hello", 0, 0, 20, 10);

        Assert.True(window.HasCustomMouseHandling);
    }

    // ── HandleEvent — close button ────────────────────────────────────────────

    [Fact]
    public void HandleEvent_ButtonDownOnCloseGlyph_WithTitleShown_ReturnsTrue()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // AbsCol=5, AbsRow=3 is where the [-] glyph is painted.
        var ev = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 5, Row: 3,
                                   TuiMouseButton.Left);

        bool consumed = window.HandleEvent(ev);

        Assert.True(consumed);
    }

    [Fact]
    public void HandleEvent_ButtonDownOnCloseGlyph_WithTitleHidden_ReturnsFalse()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10)
        {
            ShowTitle = false,
        };
        desktop.Add(window);

        // Same cell — but ShowTitle=false so no glyph is rendered; should not consume.
        var ev = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 5, Row: 3,
                                   TuiMouseButton.Left);

        bool consumed = window.HandleEvent(ev);

        Assert.False(consumed);
    }

    [Fact]
    public void HandleEvent_ButtonDownOnCloseGlyph_EmitsCloseCommandToParent()
    {
        var parent = new RecordingGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        parent.Add(window);

        var ev = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 5, Row: 3,
                                   TuiMouseButton.Left);

        window.HandleEvent(ev);

        TuiCommandEvent? closeCmd = parent.ReceivedEvents
            .OfType<TuiCommandEvent>()
            .FirstOrDefault(c => c.Command == TuiCommand.Close);

        Assert.NotNull(closeCmd);
    }

    [Fact]
    public void HandleEvent_ButtonDownOutsideCloseGlyph_ReturnsFalse()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // Click in the middle of the window — not on the [-] glyph.
        var ev = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 10, Row: 6,
                                   TuiMouseButton.Left);

        bool consumed = window.HandleEvent(ev);

        Assert.False(consumed);
    }

    [Fact]
    public void HandleEvent_ButtonUpOnCloseGlyph_ReturnsFalse()
    {
        // Only ButtonDown triggers Close — ButtonUp is ignored.
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        var ev = new TuiMouseEvent(TuiMouseAction.ButtonUp, Col: 5, Row: 3,
                                   TuiMouseButton.Left);

        bool consumed = window.HandleEvent(ev);

        Assert.False(consumed);
    }

    // ── Z-order activation ────────────────────────────────────────────────────

    [Fact]
    public void ButtonDown_OnInactiveWindow_BringsItToFront()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var back = new TuiWindow("Back", col: 0, row: 0, width: 20, height: 10);
        var front = new TuiWindow("Front", col: 5, row: 5, width: 20, height: 10);
        desktop.Add(back);
        desktop.Add(front);

        Assert.False(back.IsActive);

        // Click anywhere on the back window's body (not its title bar).
        back.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 2, Row: 4,
                                            TuiMouseButton.Left));

        Assert.True(back.IsActive);
        Assert.False(front.IsActive);
    }

    [Fact]
    public void ButtonDown_OnAlreadyActiveWindow_RemainsActive()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Only", col: 0, row: 0, width: 20, height: 10);
        desktop.Add(window);

        Assert.True(window.IsActive);

        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 2, Row: 4,
                                              TuiMouseButton.Left));

        Assert.True(window.IsActive);
    }

    // ── Drag ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Drag_ButtonDownOnTitleBar_BeginsDrag_AndReturnsTrue()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // Col=6,Row=3 is on the title bar but not the [-] glyph (col=5).
        var down = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                     TuiMouseButton.Left);

        bool consumed = window.HandleEvent(down);

        Assert.True(consumed);
    }

    [Fact]
    public void Drag_ButtonDownOnTitleBar_MovableTrue_UpdatesPosition()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // Begin drag at title bar (offset 1 from Col).
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        // Move to absolute col=10, row=5 → newCol = 10-1=9, newRow = 5-0=5.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: 10, Row: 5,
                                              TuiMouseButton.None));

        Assert.Equal(9, window.Col);
        Assert.Equal(5, window.Row);
    }

    [Fact]
    public void Drag_Move_ClampedToParentBounds_DoesNotExceedRightEdge()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // col=6 avoids the [-] glyph at col=5; offset = 6-5 = 1.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        // Try to move way off the right edge: newCol = 999-1 = 998.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: 999, Row: 3,
                                              TuiMouseButton.None));

        // maxCol = 80 - 20 = 60.
        Assert.Equal(60, window.Col);
    }

    [Fact]
    public void Drag_Move_ClampedToParentBounds_DoesNotExceedLeftEdge()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        // col=6 avoids the [-] glyph at col=5; offset = 6-5 = 1.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        // Try to move off the left edge: newCol = -99-1 = -100.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: -99, Row: 3,
                                              TuiMouseButton.None));

        Assert.Equal(0, window.Col);
    }

    [Fact]
    public void Drag_ButtonUp_EndsDrag_SubsequentMoveDoesNotMove()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10);
        desktop.Add(window);

        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonUp, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        // Move after ButtonUp — drag is over, position must not change.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: 20, Row: 10,
                                              TuiMouseButton.None));

        Assert.Equal(5, window.Col);
        Assert.Equal(3, window.Row);
    }

    [Fact]
    public void Drag_MovableFalse_ButtonDownOnTitleBar_DoesNotBeginDrag()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10)
        {
            Movable = false,
        };
        desktop.Add(window);

        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        // Subsequent Move must not reposition the window.
        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: 20, Row: 10,
                                              TuiMouseButton.None));

        Assert.Equal(5, window.Col);
        Assert.Equal(3, window.Row);
    }

    [Fact]
    public void Drag_TitleHidden_ButtonDownOnTitleRow_DoesNotBeginDrag()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiWindow("Hello", col: 5, row: 3, width: 20, height: 10)
        {
            ShowTitle = false,
        };
        desktop.Add(window);

        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 6, Row: 3,
                                              TuiMouseButton.Left));

        window.HandleEvent(new TuiMouseEvent(TuiMouseAction.Move, Col: 20, Row: 10,
                                              TuiMouseButton.None));

        Assert.Equal(5, window.Col);
        Assert.Equal(3, window.Row);
    }

    // ── Test doubles ──────────────────────────────────────────────────────────

    /// <summary>A <see cref="TuiGroup"/> that records all events it receives.</summary>
    private sealed class RecordingGroup : TuiGroup
    {
        public List<TuiEvent> ReceivedEvents { get; } = [];

        public override bool HandleEvent(TuiEvent ev)
        {
            ReceivedEvents.Add(ev);
            return base.HandleEvent(ev);
        }
    }

    /// <summary>A minimal <see cref="TuiView"/> that records whether <see cref="Draw"/> was called.</summary>
    private sealed class RecordingView : TuiView
    {
        public bool WasDrawn { get; private set; }

        public override void Draw(TuiRenderContext ctx) => WasDrawn = true;
    }

    // ── Test infrastructure ──────────────────────────────────────────────────

    /// <summary>
    /// Creates an in-memory <see cref="SKSurface"/> for rendering. No GPU or SDL2 required.
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
        var theme = BuildMinimalTheme();

        return new TuiRenderContext(grid, font, theme);
    }

    /// <summary>
    /// Builds a minimal <see cref="TuiTheme"/> that maps every
    /// <see cref="TuiColorRole"/> to a distinct non-default color so that
    /// palette resolution failures surface during tests.
    /// </summary>
    private static TuiTheme BuildMinimalTheme()
    {
        var colors = new Dictionary<TuiColorRole, SKColor>();
        foreach (TuiColorRole role in Enum.GetValues<TuiColorRole>())
            colors[role] = SKColors.Black;

        return new TuiTheme
        {
            Name = "TestTheme",
            Palette = new TuiPalette(colors),
        };
    }
}
