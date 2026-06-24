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
    /// Drawing order: drop shadow (if <see cref="ShowShadow"/>), left and bottom
    /// border via <see cref="TuiRenderContext.DrawBorder"/>, title bar (if
    /// <see cref="ShowTitle"/>), then <see cref="TuiGroup.Draw"/> for the children.
    /// The shadow is drawn first so that the border and title bar paint over any
    /// overlap at the window's own edges.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (ShowShadow)
            ctx.DrawShadow(AbsCol, AbsRow, Width, Height);

        ctx.DrawBorder(AbsCol, AbsRow, Width, Height, TuiColorRole.WindowBorder);

        if (ShowTitle)
            DrawTitleBar(ctx);

        base.Draw(ctx);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Draws row 0: the system-menu close glyph in the leftmost cell, followed by
    /// the title text centered over the remaining width.
    /// </summary>
    /// <remarks>
    /// Colors are selected based on <see cref="IsActive"/>: active windows use
    /// <see cref="TuiColorRole.WindowTitleBackground"/> /
    /// <see cref="TuiColorRole.WindowTitleForeground"/>; inactive windows use
    /// <see cref="TuiColorRole.WindowTitleInactiveBackground"/> /
    /// <see cref="TuiColorRole.WindowTitleInactiveForeground"/>.
    /// <para/>
    /// TODO (M3/M4): the close glyph is drawn here but does not yet respond to
    /// clicks — see the <c>HandleEvent</c> override introduced alongside
    /// <c>HasCustomMouseHandling</c>, which will dispatch
    /// <see cref="Retro.TUI.Events.TuiCommandEvent"/> with
    /// <see cref="Retro.TUI.Events.TuiCommand.Close"/> when the glyph is clicked.
    /// </remarks>
    private void DrawTitleBar(TuiRenderContext ctx)
    {
        bool active = IsActive;

        TuiColorRole titleBg = active
            ? TuiColorRole.WindowTitleBackground
            : TuiColorRole.WindowTitleInactiveBackground;

        TuiColorRole titleFg = active
            ? TuiColorRole.WindowTitleForeground
            : TuiColorRole.WindowTitleInactiveForeground;

        // Fill the entire title bar row first.
        ctx.FillRect(AbsCol, AbsRow, Width, 1, titleBg);

        // System-menu close glyph in the leftmost cell ('-' inside its own
        // background, matching the PC Tools 9.x "[-]" system box).
        ctx.DrawText(AbsCol, AbsRow, "-", TuiColorRole.WindowCloseButtonForeground,
                      TuiColorRole.WindowCloseButtonBackground);

        // Title text, centered over the remaining width (columns 1..Width-1).
        if (Width > 1)
            ctx.DrawTextCentered(AbsCol + 1, AbsRow, Width - 1, Title, titleFg, titleBg);
    }
}
