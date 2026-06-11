// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiView.cs" company="OscarNET-SOFTware">
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
/// Base class for all visual elements in the framework.
/// </summary>
/// <remarks>
/// <see cref="TuiView"/> manages the composition tree, relative grid coordinates,
/// lifecycle, dirty-flag propagation and event handling. Every widget, window, dialog
/// and the desktop itself ultimately derives from this class.
/// <para/>
/// <b>Coordinate system:</b> <see cref="Col"/> and <see cref="Row"/> are expressed
/// relative to the parent's origin. Use <see cref="AbsCol"/> / <see cref="AbsRow"/>
/// for screen-absolute positions needed during hit-testing and rendering.
/// <para/>
/// <b>Rendering contract:</b> override <see cref="Draw"/> to paint the view.
/// Never call <see cref="Draw"/> directly from application code — the message loop
/// calls it automatically each frame. Call <see cref="Invalidate"/> to request a
/// redraw when the view's visual state changes.
/// <para/>
/// <b>Event contract:</b> override <see cref="HandleEvent"/> to react to keyboard
/// and mouse events. Return <see langword="true"/> to mark the event as consumed
/// and stop further propagation.
/// </remarks>
public abstract class TuiView
{
    // ── Backing fields ────────────────────────────────────────────────────────

    private readonly List<TuiView> _children = [];
    private bool _isDirty = true;

    // ── Position and size (grid cells, relative to parent) ────────────────────

    /// <summary>Gets or sets the column offset relative to the parent's origin.</summary>
    public int Col { get; set; }

    /// <summary>Gets or sets the row offset relative to the parent's origin.</summary>
    public int Row { get; set; }

    /// <summary>Gets or sets the view width in grid columns.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets the view height in grid rows.</summary>
    public int Height { get; set; }

    // ── Absolute coordinates (derived from the parent chain) ──────────────────

    /// <summary>
    /// Gets the absolute column position on the screen grid.
    /// </summary>
    /// <remarks>
    /// Computed by summing <see cref="Col"/> through the entire ancestor chain.
    /// Use this value for rendering and hit-testing.
    /// </remarks>
    public int AbsCol => (Parent?.AbsCol ?? 0) + Col;

    /// <summary>
    /// Gets the absolute row position on the screen grid.
    /// </summary>
    /// <remarks>
    /// Computed by summing <see cref="Row"/> through the entire ancestor chain.
    /// Use this value for rendering and hit-testing.
    /// </remarks>
    public int AbsRow => (Parent?.AbsRow ?? 0) + Row;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets a value indicating whether the view is rendered and
    /// participates in event dispatch.
    /// </summary>
    /// <value>Defaults to <see langword="true"/>.</value>
    /// <remarks>
    /// An invisible view is completely ignored by the render pass and by
    /// <c>FindAt</c> hit-testing. It does <em>not</em> receive mouse events.
    /// </remarks>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the view can receive input events.
    /// </summary>
    /// <value>Defaults to <see langword="true"/>.</value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the view can receive keyboard focus.
    /// </summary>
    /// <value>Defaults to <see langword="false"/>.</value>
    public bool Focusable { get; set; }

    // ── Tree ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the parent of this view, or <see langword="null"/> if this is the root.
    /// </summary>
    public TuiView? Parent { get; internal set; }

    /// <summary>
    /// Gets the ordered list of child views owned by this view.
    /// </summary>
    /// <remarks>
    /// Children are stored in paint order: index 0 is painted first (back),
    /// the last index is painted last (front). Mutations are performed through
    /// <see cref="TuiGroup.Add"/> and <see cref="TuiGroup.Remove"/>.
    /// </remarks>
    protected IReadOnlyList<TuiView> Children => _children;

    // ── Dirty flag ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a value indicating whether this view needs to be redrawn.
    /// </summary>
    /// <remarks>
    /// Set to <see langword="true"/> by <see cref="Invalidate"/>.
    /// Cleared by the render pass after <see cref="Draw"/> is called.
    /// Currently the framework uses full-tree redraws; dirty tracking is
    /// prepared here for future incremental rendering.
    /// </remarks>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// Marks this view and all its ancestors as dirty, requesting a redraw
    /// on the next frame.
    /// </summary>
    /// <remarks>
    /// Propagates upward through <see cref="Parent"/> so that any group that
    /// clips its children knows it also needs repainting.
    /// </remarks>
    public void Invalidate()
    {
        _isDirty = true;
        Parent?.Invalidate();
    }

    // ── Internal tree mutation (used by TuiGroup) ─────────────────────────────

    /// <summary>
    /// Appends <paramref name="child"/> to the internal children list and
    /// sets its <see cref="Parent"/> to this view.
    /// </summary>
    /// <param name="child">The view to add. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="child"/> already has a parent.
    /// </exception>
    internal void AddChild(TuiView child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Parent is not null)
            throw new InvalidOperationException(
                $"View '{child.GetType().Name}' already has a parent. Remove it first.");

        child.Parent = this;
        _children.Add(child);
        Invalidate();
    }

    /// <summary>
    /// Removes <paramref name="child"/> from the internal children list and
    /// clears its <see cref="Parent"/> reference.
    /// </summary>
    /// <param name="child">The view to remove. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="child"/> was found and removed;
    /// <see langword="false"/> if it was not a child of this view.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    internal bool RemoveChild(TuiView child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (!_children.Remove(child))
            return false;

        child.Parent = null;
        Invalidate();
        return true;
    }

    // ── Internal dirty-flag reset (used by the render pass) ───────────────────

    /// <summary>
    /// Clears the dirty flag after the view has been drawn.
    /// </summary>
    /// <remarks>
    /// Called by the render pass after <see cref="Draw"/> completes.
    /// Not part of the public API.
    /// </remarks>
    internal void ClearDirty() => _isDirty = false;

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws this view onto the grid using the supplied render context.
    /// </summary>
    /// <param name="ctx">
    /// The active render context for the current frame.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// Override this method to paint the view's visual content. The base
    /// implementation does nothing, which is correct for invisible container
    /// views that only group children.
    /// <para/>
    /// <b>Coordinate contract:</b> draw relative to <see cref="AbsCol"/> /
    /// <see cref="AbsRow"/>. Do not assume <c>(0, 0)</c> is the view's origin.
    /// <para/>
    /// <b>Do not call this method directly</b> from application code.
    /// The message loop calls it automatically. Use <see cref="Invalidate"/>
    /// to request a redraw.
    /// </remarks>
    public virtual void Draw(TuiRenderContext ctx) { }

    // ── Event handling ────────────────────────────────────────────────────────

    /// <summary>
    /// Handles an incoming framework event.
    /// </summary>
    /// <param name="ev">
    /// The event to process. Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the event was consumed by this view and
    /// should not be propagated further; <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    /// Override this method to react to keyboard and mouse events.
    /// The base implementation always returns <see langword="false"/> (not consumed).
    /// <para/>
    /// Mouse events should only be handled when <see cref="Enabled"/> is
    /// <see langword="true"/>. Keyboard events should only be handled when
    /// the view also has focus (managed by <c>TuiFocusManager</c> in
    /// <c>Retro.TUI.Core</c>).
    /// </remarks>
    public virtual bool HandleEvent(TuiEvent ev) => false;
}
