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

namespace Retro.TUI.Core;

/// <summary>
/// Tests for the event-dispatch logic inside <see cref="TuiMessageLoop"/>:
/// Tab/Shift+Tab focus navigation, Escape → Cancel command, keyboard routing
/// to the focused view, and mouse hit-test dispatch.
/// </summary>
/// <remarks>
/// <see cref="TuiMessageLoop"/> is <c>internal sealed</c> and its <c>Run</c> method
/// requires a live <see cref="Retro.TUI.Hosting.ITuiHost"/> for the render loop.
/// These tests exercise only the dispatch logic by accessing the private static
/// helper methods through <c>InternalsVisibleTo</c>. The full render loop is covered
/// by integration tests in a later milestone.
/// <para/>
/// Each test uses a pre-loaded <see cref="TuiEventQueue"/>, calls the internal
/// <c>DispatchEvents</c> method reflectively, and asserts the side-effects on
/// views and the focus manager.
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

    /// <summary>
    /// Thin wrapper so test methods read as first-class calls rather than
    /// direct static references. <c>DispatchEvents</c> is <c>internal static</c>
    /// on <see cref="TuiMessageLoop"/>, visible here via <c>InternalsVisibleTo</c>.
    /// </summary>
    private static void DispatchEvents(
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager)
        => TuiMessageLoop.DispatchEvents(queue, desktop, focusManager);

    private static TuiEventQueue QueueWith(params TuiEvent[] events)
    {
        var queue = new TuiEventQueue();
        foreach (TuiEvent ev in events)
            queue.TryPost(ev);
        return queue;
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
        using var queue = QueueWith(new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.None));

        DispatchEvents(queue, desktop, fm);

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
        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.Shift));

        DispatchEvents(queue, desktop, fm);

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
        using var queue = QueueWith(new TuiKeyEvent(TuiKey.Tab, '\0', TuiModifiers.None));

        DispatchEvents(queue, desktop, fm);

        // The Tab key must be consumed by the focus manager, never by the view.
        Assert.Empty(view.ReceivedEvents);
    }

    // ── Escape → Cancel command ───────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_Escape_PostsCancelCommandToDesktop()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop();
        var handler = new RecordingView { Width = 80, Height = 25 };
        desktop.Add(handler);

        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Escape, '\0', TuiModifiers.None));

        DispatchEvents(queue, desktop, fm);

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
        var keyEvent = new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None);
        using var queue = QueueWith(keyEvent);

        DispatchEvents(queue, desktop, fm);

        Assert.Single(view.ReceivedEvents);
        Assert.Same(keyEvent, view.ReceivedEvents[0]);
    }

    [Fact]
    public void DispatchEvents_RegularKey_NoFocusedView_IsDropped()
    {
        var fm = new TuiFocusManager();  // no view focused
        var desktop = new TuiDesktop();
        using var queue = QueueWith(
            new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None));

        // Must not throw.
        var ex = Record.Exception(() => DispatchEvents(queue, desktop, fm));

        Assert.Null(ex);
    }

    // ── Mouse hit-test dispatch ───────────────────────────────────────────────

    [Fact]
    public void DispatchEvents_MouseClick_RoutesToViewUnderCursor()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var target = new RecordingView { Col = 10, Row = 5, Width = 20, Height = 10 };
        desktop.Add(target);

        var mouseEvent = new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 15, Row: 8,
                                           TuiMouseButton.Left);
        using var queue = QueueWith(mouseEvent);

        DispatchEvents(queue, desktop, fm);

        Assert.Single(target.ReceivedEvents);
        Assert.Same(mouseEvent, target.ReceivedEvents[0]);
    }

    [Fact]
    public void DispatchEvents_MouseClick_OutsideAllViews_IsDropped()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        // No children — click hits nothing.

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 5, Row: 5,
                              TuiMouseButton.Left));

        var ex = Record.Exception(() => DispatchEvents(queue, desktop, fm));

        Assert.Null(ex);
    }

    [Fact]
    public void DispatchEvents_MouseClick_OnInvisibleView_IsDropped()
    {
        var fm = new TuiFocusManager();
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var hidden = new RecordingView
        {
            Col = 0,
            Row = 0,
            Width = 80,
            Height = 25,
            Visible = false
        };
        desktop.Add(hidden);

        using var queue = QueueWith(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, Col: 5, Row: 5,
                              TuiMouseButton.Left));

        DispatchEvents(queue, desktop, fm);

        Assert.Empty(hidden.ReceivedEvents);
    }
}
