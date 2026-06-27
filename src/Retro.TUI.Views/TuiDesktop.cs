// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDesktop.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Views;

/// <summary>
/// The root view that fills the entire screen with a themed background pattern
/// and acts as the top-level container for all windows, dialogs and overlays.
/// </summary>
/// <remarks>
/// <see cref="TuiDesktop"/> is a <see cref="TuiGroup"/> that occupies the full
/// screen grid (<c>Col = 0, Row = 0, Width = grid.Columns, Height = grid.Rows</c>).
/// It is created and owned by <c>TuiApplication</c>, which sizes it after the
/// host and grid are initialized.
/// <para/>
/// The desktop has no visual state of its own beyond what is encoded in the active
/// theme. It calls <see cref="TuiRenderContext.DrawDesktopPattern"/> to paint the
/// background, then delegates to <see cref="TuiGroup.Draw"/> to render all children.
/// <para/>
/// <b>Modal stack:</b> when one or more modal views are active (pushed via
/// <see cref="PushModal"/>), <see cref="HasModal"/> returns <see langword="true"/>
/// and the active modal is accessible via <see cref="ActiveModal"/>. The message
/// loop uses this to restrict event dispatch to the topmost modal. Call
/// <see cref="PopModal"/> to remove the active modal when it closes.
/// <para/>
/// To open a non-modal window, add it via <see cref="TuiGroup.Add"/>.
/// To close it, remove it via <see cref="TuiGroup.Remove"/>.
/// </remarks>
public sealed class TuiDesktop : TuiGroup
{
    // ── Modal stack ───────────────────────────────────────────────────────────

    private readonly Stack<TuiView> _modalStack = new();

    /// <summary>
    /// Gets a value indicating whether one or more modal views are currently active.
    /// </summary>
    public bool HasModal => _modalStack.Count > 0;

    /// <summary>
    /// Gets the topmost modal view, or <see langword="null"/> when no modal is active.
    /// </summary>
    public TuiView? ActiveModal => _modalStack.Count > 0 ? _modalStack.Peek() : null;

    // ── Command sink ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the callback invoked when a <see cref="TuiCommandEvent"/>
    /// bubbles up through the entire view tree without being consumed by any child.
    /// </summary>
    /// <remarks>
    /// <see cref="TuiDesktop"/> is the natural boundary between the view tree and
    /// the application layer. A command that no child handles reaches this point
    /// and is forwarded here so the application can react without any downward
    /// coupling to the event queue or the message loop.
    /// <para>
    /// Assigned by <c>TuiApplication.Run</c> before <c>OnInitialize</c> is called,
    /// so views added during initialization can already emit commands that reach
    /// the application.
    /// </para>
    /// </remarks>
    public Action<TuiCommandEvent>? CommandSink { get; set; }

    /// <summary>
    /// Pushes <paramref name="modal"/> onto the modal stack and adds it as a child
    /// of this desktop so it participates in rendering.
    /// </summary>
    /// <param name="modal">The view to open modally. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="modal"/> is <see langword="null"/>.
    /// </exception>
    public void PushModal(TuiView modal)
    {
        ArgumentNullException.ThrowIfNull(modal);

        _modalStack.Push(modal);
        Add(modal);
    }

    /// <summary>
    /// Removes the topmost modal view from the stack and from the desktop's
    /// child collection.
    /// </summary>
    /// <returns>
    /// The view that was removed, or <see langword="null"/> when the stack was empty.
    /// </returns>
    public TuiView? PopModal()
    {
        if (_modalStack.Count == 0)
            return null;

        TuiView modal = _modalStack.Pop();
        Remove(modal);
        return modal;
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the desktop background pattern followed by all child views.
    /// </summary>
    /// <param name="ctx">
    /// The active render context for the current frame.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// The background is always repainted in full before children are drawn,
    /// which implements the current full-redraw strategy. When incremental
    /// rendering is activated in a future milestone, the background will only
    /// be repainted for dirty regions.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        // Paint the full-screen background (solid color + optional dot pattern).
        ctx.DrawDesktopPattern(ctx.Theme.DesktopPattern);

        // Draw all children on top of the background.
        base.Draw(ctx);
    }

    /// <summary>
    /// Dispatches the event to children; if the event is a
    /// <see cref="TuiCommandEvent"/> that no child consumed, forwards it to
    /// <see cref="CommandSink"/>.
    /// </summary>
    /// <param name="ev">The event to dispatch.</param>
    /// <returns>
    /// <see langword="true"/> if a child or the <see cref="CommandSink"/> consumed
    /// the event; <see langword="false"/> otherwise.
    /// </returns>
    public override bool HandleEvent(TuiEvent ev)
    {
        ArgumentNullException.ThrowIfNull(ev);

        bool consumed = base.HandleEvent(ev);

        if (!consumed && ev is TuiCommandEvent commandEvent && CommandSink is not null)
        {
            CommandSink(commandEvent);
            return true;
        }

        return consumed;
    }
}
