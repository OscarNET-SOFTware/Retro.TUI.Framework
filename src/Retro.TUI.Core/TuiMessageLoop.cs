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
/// Each iteration of the loop performs five steps, in order:
/// <list type="number">
///   <item><description>
///     <b>PollEvents</b> — asks the host to drain the OS event queue and post
///     translated <see cref="TuiEvent"/> instances to the queue.
///   </description></item>
///   <item><description>
///     <b>DispatchEvents</b> — routes each pending event to the appropriate
///     view: Tab / Shift+Tab update focus; Escape posts a Cancel command to the
///     desktop tree; all other keyboard events go to the focused view; mouse
///     events are hit-tested against the desktop tree.
///   </description></item>
///   <item><description>
///     <b>UpdateTimers</b> — reserved for future timer support (no-op in M2).
///   </description></item>
///   <item><description>
///     <b>Render</b> — acquires the host surface, calls
///     calls <see cref="TuiRenderContext.RenderFrame"/> which draws the full
///     desktop tree inside a matched begin/end pair.
///   </description></item>
///   <item><description>
///     <b>Present</b> — asks the host to flip the rendered frame to the screen.
///   </description></item>
/// </list>
/// </remarks>
internal sealed class TuiMessageLoop
{
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when any argument is <see langword="null"/>.
    /// </exception>
    public static void Run(
        ITuiHost host,
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager,
        TuiRenderContext renderCtx,
        CancellationToken ct)
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
            DispatchEvents(queue, desktop, focusManager);

            // ── Step 3: update timers (reserved for future milestone) ──────
            // UpdateTimers();

            // ── Step 4: render ─────────────────────────────────────────────────────
            renderCtx.RenderFrame(host.AcquireRenderSurface(), ctx => desktop.Draw(ctx));

            // ── Step 5: present ───────────────────────────────────────────
            host.Present();
        }
    }

    // ── Event dispatch ────────────────────────────────────────────────────────

    /// <summary>
    /// Drains the event queue and routes each event according to its type.
    /// </summary>
    internal static void DispatchEvents(
        TuiEventQueue queue,
        TuiDesktop desktop,
        TuiFocusManager focusManager)
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
                    DispatchMouseEvent(mouseEvent, desktop);
                    break;

                default:
                    // Command events and timer events propagate to the desktop.
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
    /// Routes a mouse event to the deepest visible view under the cursor,
    /// then bubbles up the ancestor chain until consumed.
    /// </summary>
    /// <remarks>
    /// <see cref="TuiGroup"/> instances are skipped during the bubble phase
    /// because their <see cref="TuiGroup.HandleEvent"/> implementation
    /// re-dispatches downward to children — which would deliver the event a
    /// second time to the same target that was already tried via hit-testing.
    /// Subclasses of <see cref="TuiGroup"/> that override
    /// <see cref="TuiView.HandleEvent"/> with their own logic (e.g.
    /// <c>TuiWindow</c>) are <em>not</em> skipped and will receive the event
    /// normally during the bubble phase.
    /// </remarks>
    private static void DispatchMouseEvent(TuiMouseEvent mouseEvent, TuiDesktop desktop)
    {
        // Hit-test the desktop tree to find the frontmost view at the cursor position.
        TuiView? target = desktop.FindAt(mouseEvent.Col, mouseEvent.Row);

        if (target is null)
            return;

        // Bubble up the ancestor chain until the event is consumed.
        // Pure TuiGroup instances are skipped: their HandleEvent only
        // re-dispatches to children and would deliver the event twice.
        TuiView? current = target;
        while (current is not null)
        {
            // TODO (M3): 'is TuiGroup' skips all TuiGroup subclasses during mouse bubble-up,
            // including future TuiWindow instances that override HandleEvent with their own logic.
            // When TuiWindow is implemented, revisit this condition — consider introducing
            // IMouseEventHandler or a HandlesMouseEvents property on TuiView to distinguish
            // pure containers from views with custom event handling.
            if (!current.Enabled || current is TuiGroup)
            {
                current = current.Parent;
                continue;
            }

            if (current.HandleEvent(mouseEvent))
                return;

            current = current.Parent;
        }
    }
}
