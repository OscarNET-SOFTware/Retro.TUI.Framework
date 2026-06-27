// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMessageLoop.cs" company="OscarNET-SOFTware">
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
using Retro.TUI.Hosting;
using Retro.TUI.Rendering;
using Retro.TUI.Views;

namespace Retro.TUI.Core;

/// <summary>
/// Orchestrates the poll → dispatch → render cycle for a single application session.
/// </summary>
/// <remarks>
/// <see cref="TuiMessageLoop"/> is <c>internal sealed</c>: it is an implementation
/// detail of <see cref="TuiApplication"/> and is never exposed to application code.
/// Application authors interact exclusively with <see cref="TuiApplication.Run"/>.
/// <para/>
/// <see cref="TuiMessageLoop"/> is an instance class because it tracks the current
/// mouse-pointer position (<see cref="_cursorPixelX"/> / <see cref="_cursorPixelY"/>)
/// across iterations, used to draw the custom mouse cursor each frame.
/// <para/>
/// Each iteration of the loop performs six steps, in order:
/// <list type="number">
///   <item><description>
///     <b>PollEvents</b> — asks the host to drain the OS event queue and post
///     translated <see cref="TuiEvent"/> instances to the queue.
///   </description></item>
///   <item><description>
///     <b>DispatchEvents</b> — routes each pending event to the appropriate
///     view: Tab / Shift+Tab update focus; Escape posts a Cancel command to the
///     desktop tree; all other keyboard events go to the focused view; mouse
///     events have <see cref="TuiMouseEvent.Col"/> / <see cref="TuiMouseEvent.Row"/>
///     recomputed from <see cref="TuiMouseEvent.PixelX"/> /
///     <see cref="TuiMouseEvent.PixelY"/> via <see cref="TuiGrid.CellCol"/> /
///     <see cref="TuiGrid.CellRow"/>, then are hit-tested against the desktop tree.
///   </description></item>
///   <item><description>
///     <b>UpdateTimers</b> — reserved for future timer support (no-op in M2).
///   </description></item>
///   <item><description>
///     <b>Render</b> — acquires the host surface, calls
///     <see cref="TuiRenderContext.RenderFrame"/> which draws the full
///     desktop tree inside a matched begin/end pair.
///   </description></item>
///   <item><description>
///     <b>Draw mouse cursor</b> — draws the custom mouse pointer at the last
///     known pixel position via <see cref="TuiRenderContext.DrawMouseCursor"/>,
///     always on top of the entire view tree.
///   </description></item>
///   <item><description>
///     <b>Present</b> — asks the host to flip the rendered frame to the screen.
///   </description></item>
/// </list>
/// </remarks>
internal sealed class TuiMessageLoop
{
    // ── State ─────────────────────────────────────────────────────────────────

    // _cursorPixelX/_cursorPixelY are plain fields updated on every mouse event by
    // DispatchMouseEvent and read each frame by Run(). IDE0032 suppressed to avoid
    // a spurious auto-property suggestion on fields with non-trivial write paths.
#pragma warning disable IDE0032
    /// <summary>
    /// Last known horizontal position of the mouse pointer, in screen pixels.
    /// </summary>
    /// <remarks>
    /// Updated by <see cref="DispatchMouseEvent"/> from
    /// <see cref="TuiMouseEvent.PixelX"/> on every mouse event, and used by
    /// <see cref="Run"/> to draw the custom mouse cursor each frame.
    /// Exposed via <see cref="CursorPixelX"/> for testing.
    /// </remarks>
    private float _cursorPixelX;

    /// <summary>
    /// Last known vertical position of the mouse pointer, in screen pixels.
    /// </summary>
    /// <remarks>
    /// Updated by <see cref="DispatchMouseEvent"/> from
    /// <see cref="TuiMouseEvent.PixelY"/> on every mouse event, and used by
    /// <see cref="Run"/> to draw the custom mouse cursor each frame.
    /// Exposed via <see cref="CursorPixelY"/> for testing.
    /// </remarks>
    private float _cursorPixelY;
#pragma warning restore IDE0032

    /// <summary>
    /// Gets the last known horizontal pixel position of the mouse pointer.
    /// Visible for testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal float CursorPixelX => _cursorPixelX;

    /// <summary>
    /// Gets the last known vertical pixel position of the mouse pointer.
    /// Visible for testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal float CursorPixelY => _cursorPixelY;

    // ── Run loop ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the message loop until the host signals shutdown or the cancellation
    /// token is cancelled.
    /// </summary>
    /// <param name="host">The native window host.</param>
    /// <param name="queue">The event queue shared with the host.</param>
    /// <param name="desktop">The root view of the visual tree.</param>
    /// <param name="focusManager">The keyboard focus manager.</param>
    /// <param name="renderCtx">The render context for the current session.</param>
    /// <param name="ct">Token used to request an early exit.</param>
    /// <param name="onCommand">
    /// Optional callback invoked for each <see cref="TuiCommandEvent"/> before it
    /// is forwarded to the desktop tree. Pass <see langword="null"/> to skip.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any argument is <see langword="null"/>.
    /// </exception>
    public void Run(
        ITuiHost host,
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        TuiRenderContext renderCtx,
        CancellationToken ct,
        Action<TuiCommandEvent>? onCommand = null)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(focusManager);
        ArgumentNullException.ThrowIfNull(renderCtx);

        while (!ct.IsCancellationRequested)
        {
            // ── Step 1: poll OS events ────────────────────────────────────
            bool keepRunning = host.PollEvents(queue);
            if (!keepRunning)
                break;

            // ── Step 2: dispatch framework events ─────────────────────────
            DispatchEvents(queue, desktop, focusManager, renderCtx.Grid, onCommand);

            // ── Step 3: update timers (reserved for future milestone) ──────
            // UpdateTimers();

            // ── Step 4: render ─────────────────────────────────────────────
            renderCtx.RenderFrame(host.AcquireRenderSurface(), ctx =>
            {
                desktop.Draw(ctx);

                // ── Step 4b: draw the custom mouse cursor on top of everything ──
                ctx.DrawMouseCursor(_cursorPixelX, _cursorPixelY);
            });

            // ── Step 5: present ───────────────────────────────────────────
            host.Present();
        }
    }

    // ── Event dispatch ────────────────────────────────────────────────────────

    /// <summary>
    /// Drains the event queue and routes each event according to its type.
    /// </summary>
    /// <param name="queue">The event queue to drain.</param>
    /// <param name="desktop">The root view of the visual tree.</param>
    /// <param name="focusManager">The keyboard focus manager.</param>
    /// <param name="grid">
    /// <param name="onCommand">
    /// Optional callback invoked for each <see cref="TuiCommandEvent"/> before it
    /// is forwarded to the desktop tree. Pass <see langword="null"/> to skip.
    /// </param>
    /// The active character grid, used to recompute <see cref="TuiMouseEvent.Col"/>
    /// and <see cref="TuiMouseEvent.Row"/> from pixel coordinates.
    /// </param>
    internal void DispatchEvents(
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        TuiGrid grid,
        Action<TuiCommandEvent>? onCommand = null)
    {
        while (queue.TryRead(out TuiEvent? ev))
        {
            // TryRead guarantees a non-null value when it returns true.
            if (ev is null)
                continue;

            switch (ev)
            {
                case TuiKeyEvent keyEvent:
                    DispatchKeyEvent(keyEvent, desktop, focusManager);
                    break;

                case TuiMouseEvent mouseEvent:
                    DispatchMouseEvent(mouseEvent, desktop, grid);
                    break;

                case TuiCommandEvent commandEvent:
                    // Notify the application first; then broadcast to the desktop
                    // so the view tree can also react (e.g. close the active dialog).
                    onCommand?.Invoke(commandEvent);
                    desktop.HandleEvent(commandEvent);
                    break;

                default:
                    desktop.HandleEvent(ev);
                    break;
            }
        }
    }

    /// <summary>
    /// Routes a keyboard event.
    /// <list type="bullet">
    ///   <item><description>Tab → <see cref="TuiFocusManager.FocusNext"/></description></item>
    ///   <item><description>Shift+Tab → <see cref="TuiFocusManager.FocusPrevious"/></description></item>
    ///   <item><description>Escape → posts <see cref="TuiCommand.Cancel"/> to the desktop</description></item>
    ///   <item><description>All other keys → focused view via <see cref="TuiFocusManager.Current"/></description></item>
    /// </list>
    /// </summary>
    private static void DispatchKeyEvent(
        TuiKeyEvent keyEvent,
        TuiDesktop desktop,
        TuiFocusManager focusManager)
    {
        // Tab / Shift+Tab: focus navigation — consumed here, not forwarded.
        if (keyEvent.Key == TuiKey.Tab)
        {
            if ((keyEvent.Modifiers & TuiModifiers.Shift) != 0)
                focusManager.FocusPrevious();
            else
                focusManager.FocusNext();

            return;
        }

        // Escape: synthesize a Cancel command and broadcast via the desktop.
        if (keyEvent.Key == TuiKey.Escape)
        {
            desktop.HandleEvent(new TuiCommandEvent(TuiCommand.Cancel));
            return;
        }

        // All other keyboard events go to the currently focused view.
        focusManager.Current?.HandleEvent(keyEvent);
    }

    /// <summary>
    /// Recomputes grid coordinates from pixel coordinates, updates the tracked
    /// mouse-cursor pixel position, then routes the event to the deepest visible
    /// view under the cursor and bubbles up the ancestor chain until consumed.
    /// </summary>
    /// <param name="mouseEvent">
    /// The raw mouse event as posted by the host. <see cref="TuiMouseEvent.Col"/>
    /// and <see cref="TuiMouseEvent.Row"/> are placeholders (<c>0</c>);
    /// <see cref="TuiMouseEvent.PixelX"/> / <see cref="TuiMouseEvent.PixelY"/>
    /// carry the real screen-pixel position.
    /// </param>
    /// <param name="desktop">The root view of the visual tree.</param>
    /// <param name="grid">
    /// The active character grid, used to convert pixel coordinates to grid
    /// coordinates via <see cref="TuiGrid.CellCol"/> / <see cref="TuiGrid.CellRow"/>.
    /// </param>
    /// <remarks>
    /// <see cref="TuiGroup"/> instances are skipped during the bubble phase
    /// when <see cref="TuiView.HasCustomMouseHandling"/> is <see langword="false"/>
    /// (the default), because their <see cref="TuiGroup.HandleEvent"/> implementation
    /// re-dispatches downward to children — which would deliver the event a second
    /// time to the same target that was already tried via hit-testing.
    /// <see cref="TuiGroup"/> subclasses that override
    /// <see cref="TuiView.HasCustomMouseHandling"/> to return <see langword="true"/>
    /// (e.g. <c>TuiWindow</c>) are included in the bubble so they can handle
    /// title-bar clicks, drag operations, and similar container-level gestures.
    /// </remarks>
    private void DispatchMouseEvent(TuiMouseEvent mouseEvent, TuiDesktop desktop, TuiGrid grid)
    {
        // Track the pointer position for the custom mouse cursor, drawn each
        // frame in Run() regardless of whether any view consumes this event.
        _cursorPixelX = mouseEvent.PixelX;
        _cursorPixelY = mouseEvent.PixelY;

        // Recompute Col/Row from the real pixel position. The host posts Col/Row
        // as placeholders (0); this is the single point of truth for the mapping.
        int col = grid.CellCol(mouseEvent.PixelX);
        int row = grid.CellRow(mouseEvent.PixelY);
        TuiMouseEvent resolvedEvent = mouseEvent with { Col = col, Row = row };

        // Hit-test the desktop tree to find the frontmost view at the cursor position.
        TuiView? target = desktop.FindAt(resolvedEvent.Col, resolvedEvent.Row);

        if (target is null)
            return;

        // Bubble up the ancestor chain until the event is consumed.
        // Pure TuiGroup instances that declare no custom mouse handling are
        // skipped: their HandleEvent only re-dispatches to children and would
        // deliver the event twice. TuiGroup subclasses that override
        // HasCustomMouseHandling (e.g. TuiWindow) are included so they can
        // intercept title-bar clicks, drag operations, etc.
        TuiView? current = target;
        while (current is not null)
        {
            if (!current.Enabled || (current is TuiGroup && !current.HasCustomMouseHandling))
            {
                current = current.Parent;
                continue;
            }

            if (current.HandleEvent(resolvedEvent))
                return;

            current = current.Parent;
        }
    }
}
