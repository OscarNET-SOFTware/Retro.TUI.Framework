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

    // _cursorPixelX/_cursorPixelY/_capturedView are plain fields updated on every
    // mouse event by DispatchMouseEvent. IDE0032 suppressed to avoid a spurious
    // auto-property suggestion on fields with non-trivial write paths.
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

    /// <summary>
    /// The view that has claimed exclusive mouse input via
    /// <see cref="TuiView.HasCustomMouseHandling"/>. While non-<see langword="null"/>,
    /// <see cref="DispatchMouseEvent"/> bypasses <c>FindAt</c> hit-testing and
    /// delivers every mouse event directly to this view. Cleared unconditionally
    /// on <see cref="TuiMouseAction.ButtonUp"/>.
    /// </summary>
    private TuiView? _capturedView;
#pragma warning restore IDE0032

    /// <summary>
    /// Set to <see langword="true"/> while a nested modal loop is running.
    /// Used by <see cref="RunIteration"/> to suppress the clearing of
    /// <see cref="_modalWasOpened"/> so the flag survives until the bubble
    /// loop in <see cref="DispatchMouseEvent"/> reads it after
    /// <see cref="TuiView.HandleEvent"/> returns.
    /// </summary>
    private bool _inNestedLoop;

    /// <summary>
    /// Set to <see langword="true"/> by <see cref="BeginModal"/> when
    /// <c>TuiApplication.RunModal</c> opens a nested loop during the current
    /// dispatch cycle. Cleared at the start of each <see cref="DispatchEvents"/>
    /// call so it never leaks across iterations.
    /// <para/>
    /// Used by <see cref="DispatchMouseEvent"/> to prevent establishing mouse
    /// capture after a modal was opened synchronously inside
    /// <see cref="TuiView.HandleEvent"/> — the corresponding ButtonUp will be
    /// routed to the modal and never reach the captured view.
    /// </summary>
    private bool _modalWasOpened;

    /// <summary>
    /// The <see cref="TuiFocusManager"/> currently wired to this loop's
    /// <see cref="OnFocusManagerChanged"/> handler, or <see langword="null"/>
    /// if no manager has been seen yet.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="EnsureFocusEventWiring"/> to subscribe exactly once
    /// per <see cref="TuiFocusManager"/> instance, even across repeated
    /// <see cref="RunIteration"/> calls.
    /// </remarks>
    private TuiFocusManager? _wiredFocusManager;

    /// <summary>
    /// The view that last received <see cref="TuiFocusAction.Gained"/>, used to
    /// compute which view receives <see cref="TuiFocusAction.Lost"/> on the next
    /// focus change. Mirrors <see cref="TuiFocusManager.Current"/> but is cached
    /// locally because <see cref="TuiFocusManager.FocusChanged"/> fires after the
    /// manager's internal state has already moved to the new value.
    /// </summary>
    private TuiView? _lastFocusedView;

    // ── Internal state accessors (for testing) ────────────────────────────────

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

    /// <summary>
    /// Gets the view that currently holds exclusive mouse capture, or
    /// <see langword="null"/> when no capture is active.
    /// Visible for testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    internal TuiView? CapturedView => _capturedView;

    /// <summary>
    /// Signals that a nested modal loop is starting.
    /// Sets <see cref="_modalWasOpened"/> so the bubble loop will not establish
    /// mouse capture, and sets <see cref="_inNestedLoop"/> so
    /// <see cref="RunIteration"/> does not clear the flag during nested iterations.
    /// </summary>
    internal void BeginModal()
    {
        _modalWasOpened = true;
        _inNestedLoop = true;
    }

    /// <summary>
    /// Signals that the nested modal loop has exited.
    /// Clears <see cref="_inNestedLoop"/> so the outer <see cref="RunIteration"/>
    /// resumes clearing <see cref="_modalWasOpened"/> on the next iteration.
    /// Called from <c>TuiApplication.RunModal</c> in the <c>finally</c> block
    /// before <c>PopModal</c> — <see cref="_modalWasOpened"/> remains
    /// <see langword="true"/> here so the bubble loop can still read it.
    /// </summary>
    internal void EndModal() => _inNestedLoop = false;

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
            if (!RunIteration(host, queue, desktop, focusManager, renderCtx, onCommand))
                break;
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
    /// The active character grid, used to recompute <see cref="TuiMouseEvent.Col"/>
    /// and <see cref="TuiMouseEvent.Row"/> from pixel coordinates.
    /// </param>
    /// <param name="onCommand">
    /// Optional callback invoked for each <see cref="TuiCommandEvent"/> before it
    /// is forwarded to the desktop tree. Pass <see langword="null"/> to skip.
    /// </param>
    /// <remarks>
    /// When <see cref="TuiDesktop.HasModal"/> is <see langword="true"/>, keyboard
    /// events and mouse events are routed exclusively to
    /// <see cref="TuiDesktop.ActiveModal"/>. The application-level
    /// <paramref name="onCommand"/> callback is suppressed while a modal is active
    /// so that commands from inside the dialog do not reach the outer
    /// <c>OnCommand</c> handler.
    /// </remarks>
    internal void DispatchEvents(
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        TuiGrid grid,
        Action<TuiCommandEvent>? onCommand = null)
    {
        EnsureFocusEventWiring(focusManager);

        while (queue.TryRead(out TuiEvent? ev))
        {
            if (ev is null)
                continue;

            switch (ev)
            {
                case TuiKeyEvent keyEvent:
                    DispatchKeyEvent(keyEvent, desktop, focusManager, onCommand);
                    break;

                case TuiMouseEvent mouseEvent:
                    DispatchMouseEvent(mouseEvent, desktop, grid);
                    break;

                case TuiCommandEvent commandEvent:
                    // While a modal is active, commands go only to the modal.
                    // The outer onCommand callback is intentionally suppressed.
                    if (desktop.HasModal)
                    {
                        desktop.ActiveModal?.HandleEvent(commandEvent);
                    }
                    else
                    {
                        // Commands posted to the queue (e.g. from the host) are
                        // delivered via onCommand first, then to the desktop tree.
                        // Commands emitted from within the tree (e.g. TuiWindow [-])
                        // reach the application via Desktop.CommandSink instead —
                        // they never enter the queue and are not processed here.
                        onCommand?.Invoke(commandEvent);
                        desktop.HandleEvent(commandEvent);
                    }
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
    ///   <item><description>
    ///     Escape → <see cref="TuiCommand.Cancel"/> to the active modal when
    ///     <see cref="TuiDesktop.HasModal"/>; otherwise posted via
    ///     <paramref name="onCommand"/> so the application handles it directly.
    ///   </description></item>
    ///   <item><description>
    ///     Enter → delivered to the active modal when <see cref="TuiDesktop.HasModal"/>;
    ///     otherwise routed to the focused view via <see cref="TuiFocusManager.Current"/>.
    ///   </description></item>
    ///   <item><description>All other keys → focused view via <see cref="TuiFocusManager.Current"/></description></item>
    /// </list>
    /// </summary>
    private static void DispatchKeyEvent(
        TuiKeyEvent keyEvent,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        Action<TuiCommandEvent>? onCommand)
    {
        if (keyEvent.Key == TuiKey.Tab)
        {
            if ((keyEvent.Modifiers & TuiModifiers.Shift) != 0)
                focusManager.FocusPrevious();
            else
                focusManager.FocusNext();

            return;
        }

        // Escape — with modal: goes only to the modal.
        // Without modal: goes to onCommand (application), NOT desktop.HandleEvent.
        // Cancel is an application-level command; the desktop tree has nothing to do with it.
        if (keyEvent.Key == TuiKey.Escape)
        {
            var cancelCmd = new TuiCommandEvent(TuiCommand.Cancel);

            if (desktop.HasModal)
                desktop.ActiveModal?.HandleEvent(cancelCmd);
            else
                onCommand?.Invoke(cancelCmd);

            return;
        }

        // When a modal is active, all non-Tab/Escape keys go directly to it.
        // focusManager.Current may be null (no focusable controls registered yet),
        // so bypassing it is required for the modal to receive Enter and other keys.
        if (desktop.HasModal)
        {
            desktop.ActiveModal?.HandleEvent(keyEvent);
            return;
        }

        focusManager.Current?.HandleEvent(keyEvent);
    }

    /// <summary>
    /// Recomputes grid coordinates from pixel coordinates, updates the tracked
    /// mouse-cursor pixel position, then routes the event to the appropriate view.
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
    /// <b>Mouse capture:</b> when <see cref="_capturedView"/> is non-null, all
    /// events are delivered directly to it — <c>FindAt</c> hit-testing is skipped
    /// entirely. Capture is established on <see cref="TuiMouseAction.ButtonDown"/>
    /// by walking the ancestor chain from the hit-tested target and assigning the
    /// first ancestor whose <see cref="TuiView.HasCustomMouseHandling"/> is
    /// <see langword="true"/>. Capture is released unconditionally on
    /// <see cref="TuiMouseAction.ButtonUp"/>, after the event has been delivered.
    /// <para/>
    /// <b>Normal dispatch (no capture):</b> <c>FindAt</c> locates the deepest
    /// visible view at the cursor position. The event then bubbles up the ancestor
    /// chain; pure <see cref="TuiGroup"/> instances without
    /// <see cref="TuiView.HasCustomMouseHandling"/> are skipped to avoid delivering
    /// the event twice to the same logical target.
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

        // ── Modal fast path ───────────────────────────────────────────────────
        // When a modal is active, all mouse events go exclusively to it.
        // FindAt and the normal bubble are bypassed entirely.
        // On ButtonUp, also clear any stale _capturedView: the ButtonUp that
        // corresponds to the ButtonDown that triggered RunModal will never reach
        // the captured view — the nested loop routes everything to the modal.
        // Without this reset on ButtonUp, _capturedView remains orphaned.
        if (desktop.HasModal)
        {
            if (resolvedEvent.Action == TuiMouseAction.ButtonUp)
                _capturedView = null;

            desktop.ActiveModal?.HandleEvent(resolvedEvent);
            return;
        }

        // ── Captured-view fast path ───────────────────────────────────────────
        // While a view holds capture, FindAt is irrelevant: every mouse event
        // belongs to the capturing view regardless of cursor position. This is
        // what makes drag operations work correctly when the cursor moves outside
        // the window chrome before ButtonUp arrives.
        if (_capturedView is not null)
        {
            _capturedView.HandleEvent(resolvedEvent);

            // ButtonUp always releases capture, unconditionally. The captured view
            // already received the event above and can reset its own drag state.
            if (resolvedEvent.Action == TuiMouseAction.ButtonUp)
                _capturedView = null;

            return;
        }

        // ── Normal dispatch: hit-test + bubble ────────────────────────────────
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
            {
                // On ButtonDown, establish capture on the first ancestor with
                // HasCustomMouseHandling. Capture is intentional and explicit:
                // a plain button consuming ButtonDown does not imply drag intent.
                //
                // Guard: do NOT capture if a modal was opened during HandleEvent.
                // RunModal is synchronous — it can be called from inside HandleEvent
                // (e.g. TuiWindow [-] → CommandSink → RunModal), opening and closing
                // a modal before HandleEvent returns. By the time we reach this line
                // HasModal is already false. _modalWasOpened is set by BeginModal()
                // at the start of RunModal and cleared at the next DispatchEvents
                // call — it remains true here if a modal was opened and closed
                // during this HandleEvent call, preventing orphaned capture.
                if (resolvedEvent.Action == TuiMouseAction.ButtonDown
                    && current.HasCustomMouseHandling
                    && !_modalWasOpened)
                {
                    _capturedView = current;
                }

                return;
            }

            current = current.Parent;
        }
    }

    // ── Single-iteration entry point (used by TuiApplication.RunModal) ────────

    /// <summary>
    /// Executes one poll → dispatch → render → present iteration.
    /// </summary>
    /// <param name="host">The native window host.</param>
    /// <param name="queue">The event queue shared with the host.</param>
    /// <param name="desktop">The root view of the visual tree.</param>
    /// <param name="focusManager">The keyboard focus manager.</param>
    /// <param name="renderCtx">The render context for the current session.</param>
    /// <param name="onCommand">
    /// Optional callback invoked for each <see cref="TuiCommandEvent"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the host is still running and the loop should
    /// continue; <see langword="false"/> if the host signalled shutdown.
    /// </returns>
    /// <remarks>
    /// Extracted from <see cref="Run"/> so that <c>TuiApplication.RunModal</c>
    /// can drive the same cycle inside a nested loop without duplicating logic.
    /// Visibility is <c>internal</c>: only <c>TuiApplication</c> (same assembly
    /// via <c>InternalsVisibleTo</c>) may call this method.
    /// </remarks>
    internal bool RunIteration(
        ITuiHost host,
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        TuiRenderContext renderCtx,
        Action<TuiCommandEvent>? onCommand = null)
    {
        bool keepRunning = host.PollEvents(queue);
        if (!keepRunning)
            return false;

        // Clear _modalWasOpened only in the outer loop — not while a nested modal
        // loop is running. The flag is set by BeginModal() when RunModal opens a
        // dialog synchronously from inside HandleEvent. It must survive until the
        // bubble loop in DispatchMouseEvent reads it after HandleEvent returns.
        // _inNestedLoop prevents nested DispatchEvents calls from clearing it
        // prematurely; EndModal() clears _inNestedLoop before RunModal returns,
        // so the outer RunIteration resumes clearing on the next iteration.
        if (!_inNestedLoop)
            _modalWasOpened = false;

        DispatchEvents(queue, desktop, focusManager, renderCtx.Grid, onCommand);

        renderCtx.RenderFrame(host.AcquireRenderSurface(), ctx =>
        {
            desktop.Draw(ctx);
            ctx.DrawMouseCursor(_cursorPixelX, _cursorPixelY);
        });

        host.Present();
        return true;
    }

    /// <summary>
    /// Subscribes to <paramref name="focusManager"/>'s <see cref="TuiFocusManager.FocusChanged"/>
    /// event exactly once, translating each change into <see cref="TuiFocusEvent"/>
    /// instances delivered to the affected views via <see cref="TuiView.HandleEvent"/>.
    /// Visible for testing via <c>InternalsVisibleTo</c>.
    /// </summary>
    /// <param name="focusManager">The focus manager driving the current session.</param>
    /// <remarks>
    /// Idempotent per manager instance: re-entering with the same
    /// <paramref name="focusManager"/> is a no-op. If a different manager was
    /// previously wired, the old subscription is removed first — defensive
    /// robustness only; not exercised by a dedicated test today because each
    /// <c>TuiApplication.Run</c> session creates a fresh <see cref="TuiMessageLoop"/>
    /// (see <c>TuiApplication.Run</c>, step 6), so no <see cref="TuiMessageLoop"/>
    /// instance is ever rewired to a second <see cref="TuiFocusManager"/> in
    /// practice. Add a test here if that lifecycle ever changes.
    /// </remarks>
    internal void EnsureFocusEventWiring(TuiFocusManager focusManager)
    {
        if (ReferenceEquals(_wiredFocusManager, focusManager))
            return;

        _wiredFocusManager?.FocusChanged -= OnFocusManagerChanged;

        focusManager.FocusChanged += OnFocusManagerChanged;
        _wiredFocusManager = focusManager;
        _lastFocusedView = focusManager.Current;
    }

    /// <summary>
    /// Translates a <see cref="TuiFocusManager.FocusChanged"/> notification into
    /// <see cref="TuiFocusEvent"/> deliveries: <see cref="TuiFocusAction.Lost"/> to
    /// the previously focused view (if any), then <see cref="TuiFocusAction.Gained"/>
    /// to the newly focused view (if any).
    /// </summary>
    /// <remarks>
    /// Lost is always dispatched before Gained, matching Turbo Vision's
    /// focus-transition ordering. When focus is cleared via
    /// <see cref="TuiFocusManager.Clear"/>, only Lost is dispatched.
    /// </remarks>
    private void OnFocusManagerChanged(object? sender, TuiFocusChangedEventArgs e)
    {
        TuiView? previous = _lastFocusedView;
        TuiView? next = e.FocusedView;

        if (ReferenceEquals(previous, next))
            return;

        previous?.HandleEvent(new TuiFocusEvent(TuiFocusAction.Lost));
        next?.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        _lastFocusedView = next;
    }
}
