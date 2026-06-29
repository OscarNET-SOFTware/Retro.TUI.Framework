// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiWindow.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Windows;

/// <summary>
/// A movable, closable top-level container with a title bar, border and drop shadow.
/// </summary>
/// <remarks>
/// <see cref="TuiWindow"/> is the framework's equivalent of Turbo Vision's
/// <c>TWindow</c>, adapted to the PC Tools 9.x visual style: a single-row title bar
/// (system-menu glyph + centered title) acts as the top border, a left border line
/// and a bottom border line are drawn by <see cref="TuiRenderContext.DrawBorder"/>,
/// and an optional drop shadow is cast to the bottom-right.
/// <para/>
/// <b>Layout contract:</b> the title bar occupies row 0. Row 1 is a one-row visual
/// margin belonging to the client area — child views are free to use it, but
/// <see cref="InnerRow"/> / <see cref="InnerHeight"/> always reserve it. The left
/// column and the bottom row are reserved for the border. See <see cref="InnerCol"/>,
/// <see cref="InnerRow"/>, <see cref="InnerWidth"/> and <see cref="InnerHeight"/>.
/// <para/>
/// <b>Active state:</b> a window is considered <em>active</em> when it is the
/// frontmost child of its parent <see cref="TuiGroup"/> (typically <c>TuiDesktop</c>),
/// per <see cref="TuiGroup.IsFrontmost"/>. The active state selects which palette
/// roles are used for the title bar — see <see cref="IsActive"/>.
/// <para/>
/// <b>Event handling:</b> mouse interaction (drag-to-move, click-to-activate, the
/// system-menu close button) is implemented by the <see cref="TuiView.HandleEvent"/>
/// override introduced alongside <c>HasCustomMouseHandling</c>. This type currently
/// covers construction, layout and rendering only.
/// </remarks>
public class TuiWindow : TuiGroup
{
    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new <see cref="TuiWindow"/> with the given title and bounds.
    /// </summary>
    /// <param name="title">The text shown in the title bar. Must not be <see langword="null"/>.</param>
    /// <param name="col">Column offset relative to the parent's origin.</param>
    /// <param name="row">Row offset relative to the parent's origin.</param>
    /// <param name="width">Width in grid columns, including the border.</param>
    /// <param name="height">Height in grid rows, including the title bar and border.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="title"/> is <see langword="null"/>.
    /// </exception>
    public TuiWindow(string title, int col, int row, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(title);

        Title = title;
        Col = col;
        Row = row;
        Width = width;
        Height = height;
    }

    // ── Public properties ─────────────────────────────────────────────────────

    /// <summary>Gets or sets the text shown centered in the title bar.</summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the window can be repositioned by
    /// dragging its title bar with the mouse.
    /// </summary>
    /// <value>Defaults to <see langword="true"/>.</value>
    public bool Movable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a drop shadow is cast below and to
    /// the right of the window.
    /// </summary>
    /// <value>Defaults to <see langword="true"/>.</value>
    public bool ShowShadow { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the title bar (and therefore the
    /// system-menu glyph and title text) is drawn.
    /// </summary>
    /// <value>Defaults to <see langword="true"/>.</value>
    /// <remarks>
    /// Even when <see langword="false"/>, row 0 remains reserved by
    /// <see cref="InnerRow"/> / <see cref="InnerHeight"/> for layout consistency;
    /// it is simply left unpainted (background color only).
    /// </remarks>
    public bool ShowTitle { get; set; } = true;

    // ── Inner area (excludes border, title bar and visual margin) ──────────────

    /// <summary>
    /// Gets the absolute column of the leftmost cell available to child views.
    /// </summary>
    /// <remarks>Skips the 1-column left border.</remarks>
    public int InnerCol => AbsCol + 1;

    /// <summary>
    /// Gets the absolute row of the topmost cell available to child views.
    /// </summary>
    /// <remarks>
    /// Skips the title bar (row 0) and the 1-row visual margin (row 1) below it.
    /// </remarks>
    public int InnerRow => AbsRow + 2;

    /// <summary>
    /// Gets the number of grid columns available to child views.
    /// </summary>
    /// <remarks>Excludes the 1-column left border. The right edge has no reserved column.</remarks>
    public int InnerWidth => Width - 2;

    /// <summary>
    /// Gets the number of grid rows available to child views.
    /// </summary>
    /// <remarks>
    /// Excludes the title bar, the visual margin row below it, and the 1-row
    /// bottom border.
    /// </remarks>
    public int InnerHeight => Height - 3;

    // ── Active state ─────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a value indicating whether this window is the active (topmost) window.
    /// </summary>
    /// <remarks>
    /// A window is active when it is the frontmost child of its parent
    /// <see cref="TuiGroup"/> — see <see cref="TuiGroup.IsFrontmost"/>. A window
    /// with no parent, or whose parent is not a <see cref="TuiGroup"/>, is
    /// considered inactive.
    /// <para/>
    /// Used by <see cref="Draw"/> to select between
    /// <see cref="TuiColorRole.WindowTitleBackground"/> /
    /// <see cref="TuiColorRole.WindowTitleForeground"/> (active) and
    /// <see cref="TuiColorRole.WindowTitleInactiveBackground"/> /
    /// <see cref="TuiColorRole.WindowTitleInactiveForeground"/> (inactive).
    /// </remarks>
    public bool IsActive => Parent is TuiGroup parentGroup && parentGroup.IsFrontmost(this);

    // ── Mouse handling ────────────────────────────────────────────────────────

    // Drag state — reset to false/0 whenever a drag ends or is never started.
#pragma warning disable IDE0032
    private bool _isDragging;
#pragma warning restore IDE0032
    private int _dragOffsetCol;
    private int _dragOffsetRow;

    /// <inheritdoc/>
    /// <remarks>
    /// Always <see langword="true"/> for <see cref="TuiWindow"/>: the message loop
    /// must include this view in mouse bubble-up so that title-bar clicks
    /// (close button, drag) are delivered to <see cref="HandleEvent"/>.
    /// </remarks>
    public override bool HasCustomMouseHandling => true;

    /// <summary>
    /// Handles mouse events targeted at this window's chrome.
    /// </summary>
    /// <param name="ev">The event to process.</param>
    /// <returns>
    /// <see langword="true"/> if the event was consumed; <see langword="false"/>
    /// to continue propagation.
    /// </returns>
    /// <remarks>
    /// Handled cases, evaluated in order:
    /// <list type="bullet">
    ///   <item><description>
    ///     Any <see cref="TuiMouseAction.ButtonDown"/> anywhere on the window:
    ///     calls <see cref="TuiGroup.BringToFront"/> on the parent so the window
    ///     becomes the active (frontmost) child before further processing.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiMouseAction.ButtonDown"/> on the system-menu close glyph
    ///     (columns <see cref="TuiView.AbsCol"/> and <see cref="TuiView.AbsCol"/>+1,
    ///     row <see cref="TuiView.AbsRow"/>) when <see cref="ShowTitle"/> is
    ///     <see langword="true"/>: emits
    ///     <see cref="TuiCommandEvent"/>(<see cref="TuiCommand.Close"/>) to the
    ///     parent chain and returns <see langword="true"/>.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiMouseAction.ButtonDown"/> anywhere else on the title bar row
    ///     when <see cref="Movable"/> is <see langword="true"/>: begins a drag
    ///     operation, capturing the cursor offset relative to <see cref="TuiView.Col"/>
    ///     / <see cref="TuiView.Row"/>.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiMouseAction.Move"/> while dragging: repositions the window,
    ///     clamped to the parent bounds via <see cref="TuiView.Parent"/>.
    ///     Calls <see cref="TuiView.Invalidate"/> to request a redraw.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiMouseAction.ButtonUp"/> while dragging: ends the drag.
    ///   </description></item>
    ///   <item><description>
    ///     All other events are forwarded to <see cref="TuiGroup.HandleEvent"/>
    ///     (child dispatch).
    ///   </description></item>
    /// </list>
    /// TODO (M4): clicking the close glyph should open the system-menu popup
    /// rather than emitting <see cref="TuiCommand.Close"/> directly.
    /// </remarks>
    public override bool HandleEvent(TuiEvent ev)
    {
        if (ev is not TuiMouseEvent mouse)
            return base.HandleEvent(ev);

        // ── Bring to front on any click ───────────────────────────────────────
        // Any ButtonDown on the window activates it (raises it to the front of
        // the z-order) before processing the specific click target. This means
        // even a click on [-] brings the window forward before emitting Close.
        if (mouse.Action == TuiMouseAction.ButtonDown)
        {
            if (Parent is TuiGroup parentGroup)
                parentGroup.BringToFront(this);
        }

        // ── Close button ([-]) ────────────────────────────────────────────────
        // The glyph is drawn as 18×16 px (CellHeight+2 wide × CellHeight tall)
        // per PC Tools 9.x reference. On a 9 px wide cell it overflows into
        // AbsCol+1. Both cells are included in the hit-test; the rightmost 1 px
        // of the glyph falls inside AbsCol+2 but is not individually targetable
        // at cell resolution.
        if (mouse.Action == TuiMouseAction.ButtonDown
            && ShowTitle
            && (mouse.Col == AbsCol || mouse.Col == AbsCol + 1)
            && mouse.Row == AbsRow)
        {
            Parent?.HandleEvent(new TuiCommandEvent(TuiCommand.Close));
            return true;
        }

        // ── Drag — begin ──────────────────────────────────────────────────────
        if (mouse.Action == TuiMouseAction.ButtonDown
            && Movable
            && ShowTitle
            && mouse.Row == AbsRow
            && mouse.Col != AbsCol
            && mouse.Col != AbsCol + 1)
        {
            _isDragging = true;
            _dragOffsetCol = mouse.Col - Col;
            _dragOffsetRow = mouse.Row - Row;
            return true;
        }

        // ── Drag — move ───────────────────────────────────────────────────────
        if (mouse.Action == TuiMouseAction.Move && _isDragging)
        {
            int newCol = mouse.Col - _dragOffsetCol;
            int newRow = mouse.Row - _dragOffsetRow;

            // Clamp to parent bounds so the window cannot be dragged off-screen.
            int maxCol = (Parent?.Width ?? int.MaxValue) - Width;
            int maxRow = (Parent?.Height ?? int.MaxValue) - Height;
            Col = Math.Clamp(newCol, 0, Math.Max(0, maxCol));
            Row = Math.Clamp(newRow, 0, Math.Max(0, maxRow));

            Invalidate();
            return true;
        }

        // ── Drag — end ────────────────────────────────────────────────────────
        if (mouse.Action == TuiMouseAction.ButtonUp && _isDragging)
        {
            _isDragging = false;
            return true;
        }

        return base.HandleEvent(ev);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the window chrome (title bar, border, shadow) followed by all
    /// visible children.
    /// </summary>
    /// <param name="ctx">
    /// The active render context for the current frame.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// Drawing order: drop shadow (if <see cref="ShowShadow"/>, painted without
    /// clip so it extends outside the window bounds), window background fill,
    /// border via <see cref="TuiRenderContext.DrawBorder"/>, title bar (if
    /// <see cref="ShowTitle"/>), then children clipped to the inner area
    /// (<see cref="InnerCol"/>, <see cref="InnerRow"/>, <see cref="InnerWidth"/>,
    /// <see cref="InnerHeight"/>).
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (ShowShadow)
            ctx.DrawShadow(AbsCol, AbsRow, Width, Height);

        ctx.FillRect(AbsCol, AbsRow, Width, Height, TuiColorRole.WindowBackground);
        ctx.DrawBorder(AbsCol, AbsRow, Width, Height, TuiColorRole.WindowBorder);

        if (ShowTitle)
            DrawTitleBar(ctx);

        ctx.PushClip(InnerCol, InnerRow, InnerWidth, InnerHeight);
        try
        {
            DrawChildViews(ctx);
        }
        finally
        {
            ctx.PopClip();
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Draws the title bar row. Override in subclasses to use different
    /// color roles (e.g. <see cref="TuiDialog"/> uses <c>Dialog*</c> roles).
    /// </summary>
    /// <param name="ctx">The active render context.</param>
    protected virtual void DrawTitleBar(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        bool active = IsActive;

        TuiColorRole titleBg = active
            ? TuiColorRole.WindowTitleBackground
            : TuiColorRole.WindowTitleInactiveBackground;

        TuiColorRole titleFg = active
            ? TuiColorRole.WindowTitleForeground
            : TuiColorRole.WindowTitleInactiveForeground;

        // Fill the entire title bar row first.
        ctx.FillRect(AbsCol, AbsRow, Width, 1, titleBg);

        // Title text, centered over the remaining width (columns 1..Width-1).
        if (Width > 1)
            ctx.DrawTextCentered(AbsCol + 1, AbsRow, Width - 1, Title, titleFg, titleBg);

        // System-menu close glyph — uses active or inactive title roles
        // depending on whether this window is the frontmost child.
        // In inactive state, glyphFg uses WindowBorder (black) so the glyph
        // outline remains crisp regardless of the title bar color.
        TuiColorRole glyphFg = active
            ? TuiColorRole.WindowCloseButtonForeground
            : TuiColorRole.WindowBorder;

        TuiColorRole glyphBg = active
            ? TuiColorRole.WindowCloseButtonBackground
            : TuiColorRole.WindowTitleInactiveBackground;

        ctx.DrawSystemMenuGlyph(AbsCol, AbsRow, glyphFg, glyphBg);
    }
}
