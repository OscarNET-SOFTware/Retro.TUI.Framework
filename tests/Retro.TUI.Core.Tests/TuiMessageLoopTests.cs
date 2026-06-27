// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMessageLoopTests.cs" company="OscarNET-SOFTware">
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
using Retro.TUI.Views;

using SkiaSharp;

namespace Retro.TUI.Core;

/// <summary>
/// Tests for the event-dispatch logic inside <see cref="TuiMessageLoop"/>:
/// Tab/Shift+Tab focus navigation, Escape → Cancel command, keyboard routing
/// to the focused view, mouse hit-test dispatch, pixel→cell coordinate
/// recomputation, and mouse-cursor pixel tracking.
/// </summary>
/// <remarks>
/// <see cref="TuiMessageLoop"/> is <c>internal sealed</c> and an instance class:
/// it tracks the current mouse-pointer pixel position across iterations for the
/// custom mouse cursor. Its <c>Run</c> method requires a live
/// <see cref="Retro.TUI.Hosting.ITuiHost"/> for the render loop and is not exercised
/// directly here. These tests instantiate <see cref="TuiMessageLoop"/> and call its
/// <c>internal</c> <c>DispatchEvents</c> method, visible via
/// <c>InternalsVisibleTo</c>. The full render loop is covered by integration tests
/// in a later milestone.
/// </remarks>
public sealed class TuiMessageLoopTests
{
    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class RecordingView : TuiView
    {
        public RecordingView() => Focusable = true;
        public List<TuiEvent> ReceivedEvents { get; } = [];
        public bool ConsumeEvents { get; set; }

        public override void Draw(TuiRenderContext ctx) { }

        public override bool HandleEvent(TuiEvent ev)
        {
            ReceivedEvents.Add(ev);
            return ConsumeEvents;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TuiEventQueue QueueWith(params TuiEvent[] events)
    {
        var queue = new TuiEventQueue();
        foreach (TuiEvent ev in events)
            queue.TryPost(ev);
        return queue;
    }

    /// <summary>
    /// Builds an initialized <see cref="TuiGrid"/> for a screen of
    /// <paramref name="cols"/>×<paramref name="rows"/> nominal cells.
    /// </summary>
    /// <remarks>
    /// Tests derive expected pixel positions from <see cref="TuiGrid.PixelX"/> /
    /// <see cref="TuiGrid.PixelY"/> and expected cell positions from
    /// <see cref="TuiGrid.CellCol"/> / <see cref="TuiGrid.CellRow"/> on this same
    /// grid instance, so the actual <c>CellWidth</c>/<c>CellHeight</c> measured
    /// from <see cref="SKTypeface.Default"/> do not need to match any specific
    /// nominal size for the assertions to hold.
    /// </remarks>
    private static TuiGrid BuildGrid(int cols, int rows)
    {
        using var font = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(cols * 9, rows * 16, font);
        return grid;
    }

    // ── Tab / Shift+Tab ───────────────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_Tab_CallsFocusNext()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        var viewB = new RecordingView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewA);

        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        using var queue = QueueWith(new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.None));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Same(viewB, fm.Current);
    }

    [Fact]
    public void DispatchEvents_ShiftTab_CallsFocusPrevious()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        var viewB = new RecordingView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewB);

        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.Shift));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Same(viewA, fm.Current);
    }

    [Fact]
    public void DispatchEvents_Tab_IsNotForwardedToFocusedView()
    {
        var fm = new TuiFocusManager();
        var view = new RecordingView();
        fm.Register(view);
        fm.SetFocus(view);

        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        using var queue = QueueWith(new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.None));

        loop.DispatchEvents(queue, desktop, fm, grid);

        // The Tab key must be consumed by the focus manager, never by the view.
        Assert.Empty(view.ReceivedEvents);
    }

    // ── Escape → Cancel command ───────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_Escape_PostsCancelCommandToDesktop()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var handler = new RecordingView { Width = 80, Height = 25 };
        desktop.Add(handler);

        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Escape, '\0', TuiModifiers.None));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Single(handler.ReceivedEvents);
        var cmdEvent = Assert.IsType<TuiCommandEvent>(handler.ReceivedEvents[0]);
        Assert.Equal(TuiCommand.Cancel, cmdEvent.Command);
    }

    // ── Keyboard → focused view ───────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_RegularKey_RoutesToFocusedView()
    {
        var fm = new TuiFocusManager();
        var view = new RecordingView();
        fm.Register(view);
        fm.SetFocus(view);

        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var keyEvent = new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None);
        using var queue = QueueWith(keyEvent);

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Single(view.ReceivedEvents);
        Assert.Same(keyEvent, view.ReceivedEvents[0]);
    }

    [Fact]
    public void DispatchEvents_RegularKey_NoFocusedView_IsDropped()
    {
        var fm = new TuiFocusManager();  // no view focused
        var desktop = new TuiDesktop();
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None));

        // Must not throw.
        var ex = Record.Exception(() => loop.DispatchEvents(queue, desktop, fm, grid));

        Assert.Null(ex);
    }

    // ── Mouse hit-test dispatch ───────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_MouseClick_RoutesToViewUnderCursor()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var target = new RecordingView { Col = 10, Row = 5, Width = 20, Height = 10 };
        desktop.Add(target);

        // Pixel position inside the target's cell range (col 15, row 8).
        float pixelX = grid.PixelX(15) + 1f;
        float pixelY = grid.PixelY(8) + 1f;

        var mouseEvent = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                                           TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY);
        using var queue = QueueWith(mouseEvent);

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Single(target.ReceivedEvents);
        var received = Assert.IsType<TuiMouseEvent>(target.ReceivedEvents[0]);
        Assert.Equal(15, received.Col);
        Assert.Equal(8, received.Row);
        Assert.Equal(pixelX, received.PixelX);
        Assert.Equal(pixelY, received.PixelY);
    }

    [Fact]
    public void DispatchEvents_MouseClick_OutsideAllViews_IsDropped()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        // No children — click hits nothing.

        float pixelX = grid.PixelX(5);
        float pixelY = grid.PixelY(5);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                              TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY));

        var ex = Record.Exception(() => loop.DispatchEvents(queue, desktop, fm, grid));

        Assert.Null(ex);
    }

    [Fact]
    public void DispatchEvents_MouseClick_OnInvisibleView_IsDropped()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var hidden = new RecordingView
        {
            Col = 0,
            Row = 0,
            Width = 80,
            Height = 25,
            Visible = false
        };
        desktop.Add(hidden);

        float pixelX = grid.PixelX(5);
        float pixelY = grid.PixelY(5);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                              TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Empty(hidden.ReceivedEvents);
    }

    // ── Pixel → cell coordinate recomputation ───────────────────────────────────

    [Fact]
    public void DispatchEvents_MouseEvent_RecomputesColRowFromPixels()
    {
        // Even though the raw event carries Col=0, Row=0 (host placeholders),
        // the dispatched event must carry the recomputed grid coordinates.
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var target = new RecordingView { Col = 0, Row = 0, Width = 80, Height = 25 };
        desktop.Add(target);

        float pixelX = grid.PixelX(7) + 2f;
        float pixelY = grid.PixelY(3) + 2f;

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.Move, Col: 0, Row: 0,
                              TuiMouseButton.None, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        var received = Assert.IsType<TuiMouseEvent>(Assert.Single(target.ReceivedEvents));
        Assert.Equal(grid.CellCol(pixelX), received.Col);
        Assert.Equal(grid.CellRow(pixelY), received.Row);
    }

    [Fact]
    public void DispatchEvents_MouseEvent_PreservesPixelCoordinates()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        var target = new RecordingView { Col = 0, Row = 0, Width = 80, Height = 25 };
        desktop.Add(target);

        const float pixelX = 42.5f;
        const float pixelY = 17.25f;

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.Move, Col: 0, Row: 0,
                              TuiMouseButton.None, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        var received = Assert.IsType<TuiMouseEvent>(Assert.Single(target.ReceivedEvents));
        Assert.Equal(pixelX, received.PixelX);
        Assert.Equal(pixelY, received.PixelY);
    }

    // ── TuiGroup skipped during bubble-up ───────────────────────────────────────

    [Fact]
    public void DispatchEvents_MouseClick_SkipsPureTuiGroupDuringBubbleUp()
    {
        // A plain TuiGroup wrapping a RecordingView: the event must reach the
        // RecordingView exactly once (not twice via the group's HandleEvent).
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();

        var group = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var target = new RecordingView { Col = 0, Row = 0, Width = 80, Height = 25 };
        group.Add(target);
        desktop.Add(group);

        float pixelX = grid.PixelX(1);
        float pixelY = grid.PixelY(1);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                              TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Single(target.ReceivedEvents);
    }

    [Fact]
    public void DispatchEvents_MouseClick_DeliversToGroupWithCustomMouseHandling()
    {
        // A TuiGroup subclass that opts in via HasCustomMouseHandling must
        // receive the event during bubble-up — exactly as TuiWindow will.
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();

        var customGroup = new CustomMouseGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var child = new RecordingView { Col = 0, Row = 0, Width = 80, Height = 25 };
        customGroup.Add(child);
        desktop.Add(customGroup);

        float pixelX = grid.PixelX(1);
        float pixelY = grid.PixelY(1);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                              TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        // Child receives it first (hit-test), then the custom group during bubble.
        Assert.Single(child.ReceivedEvents);
        Assert.Single(customGroup.ReceivedEvents);
    }

    [Fact]
    public void DispatchEvents_MouseClick_CustomGroupConsumes_ChildNotReachedAgain()
    {
        // When the custom group consumes the event, bubble-up stops.
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();

        var customGroup = new CustomMouseGroup
        {
            Col = 0,
            Row = 0,
            Width = 80,
            Height = 25,
            ConsumeEvents = true,
        };
        var child = new RecordingView { Col = 0, Row = 0, Width = 80, Height = 25 };
        customGroup.Add(child);
        desktop.Add(customGroup);

        float pixelX = grid.PixelX(1);
        float pixelY = grid.PixelY(1);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 0, Row: 0,
                              TuiMouseButton.Left, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        // Child was hit first; custom group consumed on bubble — desktop untouched.
        Assert.Single(child.ReceivedEvents);
        Assert.Single(customGroup.ReceivedEvents);
    }

    /// <summary>
    /// A <see cref="TuiGroup"/> subclass that opts into mouse bubble-up by
    /// overriding <see cref="TuiView.HasCustomMouseHandling"/>.
    /// </summary>
    private sealed class CustomMouseGroup : TuiGroup
    {
        public List<TuiEvent> ReceivedEvents { get; } = [];
        public bool ConsumeEvents { get; set; }

        public override bool HasCustomMouseHandling => true;

        public override bool HandleEvent(TuiEvent ev)
        {
            ReceivedEvents.Add(ev);
            // Do NOT call base.HandleEvent — that would re-dispatch downward
            // to children, delivering the event a second time.
            return ConsumeEvents;
        }
    }

    // ── Mouse cursor pixel tracking ──────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_MouseMove_UpdatesCursorPixelPosition()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();

        const float pixelX = 100f;
        const float pixelY = 50f;

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.Move, Col: 0, Row: 0,
                              TuiMouseButton.None, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Equal(pixelX, loop.CursorPixelX);
        Assert.Equal(pixelY, loop.CursorPixelY);
    }

    [Fact]
    public void DispatchEvents_MouseMove_OutsideAllViews_StillUpdatesCursorPosition()
    {
        // The cursor must track the pointer even when no view is hit —
        // the cursor is drawn regardless of hit-testing outcome.
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var grid = BuildGrid(80, 25);
        var loop = new TuiMessageLoop();
        // No children — move hits nothing.

        const float pixelX = 200f;
        const float pixelY = 75f;

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.Move, Col: 0, Row: 0,
                              TuiMouseButton.None, PixelX: pixelX, PixelY: pixelY));

        loop.DispatchEvents(queue, desktop, fm, grid);

        Assert.Equal(pixelX, loop.CursorPixelX);
        Assert.Equal(pixelY, loop.CursorPixelY);
    }

    // ── Mouse capture tests ───────────────────────────────────────────────────

    [Fact]
    public void DispatchMouseEvent_ButtonDown_OnCustomMouseHandlingView_SetsCapturedView()
    {
        // Arrange
        var grid = new TuiGrid();
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var window = new CapturingView { Col = 0, Row = 0, Width = 30, Height = 10 };
        desktop.Add(window);
        var queue = new TuiEventQueue();
        var loop = new TuiMessageLoop();

        var buttonDown = new TuiMouseEvent(
            TuiMouseAction.ButtonDown, Col: 1, Row: 1,
            Button: TuiMouseButton.Left);

        // Act
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);
        queue.TryPost(buttonDown);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);

        // Assert
        Assert.Equal(window, loop.CapturedView);
    }

    [Fact]
    public void DispatchMouseEvent_Move_WhileCaptured_DeliveredToCapturedView_NotHitTarget()
    {
        // Arrange
        var grid = new TuiGrid();
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var window = new CapturingView { Col = 0, Row = 0, Width = 30, Height = 10 };
        var otherView = new CapturingView { Col = 40, Row = 0, Width = 20, Height = 10 };
        desktop.Add(window);
        desktop.Add(otherView);
        var queue = new TuiEventQueue();
        var loop = new TuiMessageLoop();

        // Establish capture on window via ButtonDown inside its bounds.
        var buttonDown = new TuiMouseEvent(
            TuiMouseAction.ButtonDown, Col: 1, Row: 1,
            Button: TuiMouseButton.Left);
        queue.TryPost(buttonDown);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);

        // Move cursor into otherView's territory (col 41).
        var move = new TuiMouseEvent(
            TuiMouseAction.Move, Col: 41, Row: 0,
            Button: TuiMouseButton.None);
        queue.TryPost(move);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);

        // Assert: window received the Move, otherView did not.
        Assert.Contains(TuiMouseAction.Move, window.ReceivedActions);
        Assert.DoesNotContain(TuiMouseAction.Move, otherView.ReceivedActions);
    }

    [Fact]
    public void DispatchMouseEvent_ButtonUp_ReleasesCapturedView()
    {
        // Arrange
        var grid = new TuiGrid();
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var window = new CapturingView { Col = 0, Row = 0, Width = 30, Height = 10 };
        desktop.Add(window);
        var queue = new TuiEventQueue();
        var loop = new TuiMessageLoop();

        var buttonDown = new TuiMouseEvent(
            TuiMouseAction.ButtonDown, Col: 1, Row: 1,
            Button: TuiMouseButton.Left);
        var buttonUp = new TuiMouseEvent(
            TuiMouseAction.ButtonUp, Col: 1, Row: 1,
            Button: TuiMouseButton.Left);

        queue.TryPost(buttonDown);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);
        Assert.Equal(window, loop.CapturedView); // sanity check

        // Act
        queue.TryPost(buttonUp);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);

        // Assert
        Assert.Null(loop.CapturedView);
    }

    [Fact]
    public void DispatchMouseEvent_ButtonDown_NormalView_DoesNotSetCapturedView()
    {
        // Arrange — a plain view that consumes ButtonDown but has no custom mouse handling.
        var grid = new TuiGrid();
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var plain = new ConsumingView { Col = 0, Row = 0, Width = 30, Height = 10 };
        desktop.Add(plain);
        var queue = new TuiEventQueue();
        var loop = new TuiMessageLoop();

        var buttonDown = new TuiMouseEvent(
            TuiMouseAction.ButtonDown, Col: 1, Row: 1,
            Button: TuiMouseButton.Left);

        queue.TryPost(buttonDown);
        loop.DispatchEvents(queue, desktop, new TuiFocusManager(), grid);

        // Assert: consuming ButtonDown without HasCustomMouseHandling does NOT capture.
        Assert.Null(loop.CapturedView);
    }

    // ── Test-only view helpers ────────────────────────────────────────────────

    /// <summary>
    /// A leaf view that declares custom mouse handling, records every received
    /// action, and always consumes mouse events.
    /// </summary>
    private sealed class CapturingView : TuiView
    {
        public List<TuiMouseAction> ReceivedActions { get; } = [];
        public override bool HasCustomMouseHandling => true;

        public override void Draw(TuiRenderContext ctx) { }

        public override bool HandleEvent(TuiEvent ev)
        {
            if (ev is TuiMouseEvent mouse)
            {
                ReceivedActions.Add(mouse.Action);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A plain leaf view without custom mouse handling that always consumes
    /// mouse events. Used to verify capture is NOT established for normal views.
    /// </summary>
    private sealed class ConsumingView : TuiView
    {
        public override bool HasCustomMouseHandling => false;
        public override void Draw(TuiRenderContext ctx) { }
        public override bool HandleEvent(TuiEvent ev) => ev is TuiMouseEvent;
    }
}
