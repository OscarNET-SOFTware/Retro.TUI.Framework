// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiGroup.cs" company="OscarNET-SOFTware">
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
/// A <see cref="TuiView"/> that owns and manages an ordered collection of child views.
/// </summary>
/// <remarks>
/// <see cref="TuiGroup"/> is the primary composition primitive in the framework.
/// Windows, dialogs and the desktop all derive from it.
/// <para/>
/// <b>Paint order:</b> children are drawn back-to-front (index 0 first, last index on
/// top). The same reverse order is used for event dispatch and hit-testing so that the
/// frontmost view gets priority.
/// <para/>
/// <b>Clipping:</b> each child is drawn inside a clip rectangle that matches its own
/// bounds. A child cannot paint outside its declared <see cref="TuiView.Width"/> ×
/// <see cref="TuiView.Height"/> area.
/// <para/>
/// <b>Visibility rule:</b> children with <see cref="TuiView.Visible"/> set to
/// <see langword="false"/> are skipped entirely — they are neither drawn nor considered
/// during hit-testing or event dispatch.
/// </remarks>
public class TuiGroup : TuiView
{
    // ── Child management ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds <paramref name="child"/> as the frontmost child of this group.
    /// </summary>
    /// <param name="child">The view to add. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="child"/> already belongs to a parent view.
    /// </exception>
    public void Add(TuiView child) => AddChild(child);

    /// <summary>
    /// Removes <paramref name="child"/> from this group.
    /// </summary>
    /// <param name="child">The view to remove. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="child"/> was found and removed;
    /// <see langword="false"/> if it was not a direct child of this group.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    public bool Remove(TuiView child) => RemoveChild(child);

    // ── Hit-testing ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the frontmost visible child that contains the given absolute
    /// grid position, or <see langword="null"/> if no child matches.
    /// </summary>
    /// <param name="absCol">Screen-absolute column to test.</param>
    /// <param name="absRow">Screen-absolute row to test.</param>
    /// <returns>
    /// The deepest, frontmost <see cref="TuiView"/> that contains
    /// (<paramref name="absCol"/>, <paramref name="absRow"/>), or
    /// <see langword="null"/> if the point falls outside all visible children.
    /// </returns>
    /// <remarks>
    /// The search is <em>depth-first, reverse-order</em>: the last child in the
    /// list (the one painted on top) is tested first. Invisible children
    /// (<see cref="TuiView.Visible"/> = <see langword="false"/>) are skipped
    /// entirely and never receive mouse events.
    /// </remarks>
    public TuiView? FindAt(int absCol, int absRow)
    {
        // Iterate in reverse so that the frontmost (last-added) child wins.
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            TuiView child = Children[i];

            // Invisible children do not participate in hit-testing.
            if (!child.Visible)
                continue;

            if (absCol >= child.AbsCol && absCol < child.AbsCol + child.Width &&
                absRow >= child.AbsRow && absRow < child.AbsRow + child.Height)
            {
                // Recurse into groups for a more precise hit.
                if (child is TuiGroup group)
                {
                    TuiView? deeper = group.FindAt(absCol, absRow);
                    return deeper ?? child;
                }

                return child;
            }
        }

        return null;
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws this group and all its visible children, back-to-front.
    /// </summary>
    /// <param name="ctx">
    /// The active render context for the current frame.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// Each child is drawn inside a clip rectangle that matches its own bounds,
    /// preventing it from painting outside its declared area. The clip is pushed
    /// before calling the child's <see cref="TuiView.Draw"/> and popped immediately
    /// after, so the stack is always balanced even if <c>Draw</c> throws.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        foreach (TuiView child in Children)
        {
            if (!child.Visible)
                continue;

            ctx.PushClip(child.AbsCol, child.AbsRow, child.Width, child.Height);
            try
            {
                child.Draw(ctx);
                child.ClearDirty();
            }
            finally
            {
                ctx.PopClip();
            }
        }
    }

    // ── Event handling ────────────────────────────────────────────────────────

    /// <summary>
    /// Dispatches the event to visible, enabled children in reverse z-order
    /// (frontmost child first) until one consumes it.
    /// </summary>
    /// <param name="ev">The event to dispatch.</param>
    /// <returns>
    /// <see langword="true"/> if a child consumed the event;
    /// <see langword="false"/> if no child handled it.
    /// </returns>
    /// <remarks>
    /// The group itself does not consume any events — it only acts as a
    /// dispatcher. Subclasses may override this method to intercept events
    /// before or after child dispatch.
    /// </remarks>
    public override bool HandleEvent(TuiEvent ev)
    {
        ArgumentNullException.ThrowIfNull(ev);

        // Dispatch to children in reverse z-order (front-to-back).
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            TuiView child = Children[i];

            if (!child.Visible || !child.Enabled)
                continue;

            if (child.HandleEvent(ev))
                return true;
        }

        return false;
    }
}
