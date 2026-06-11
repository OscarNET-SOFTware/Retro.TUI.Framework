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
/// This means the pattern and color come entirely from <c>ctx.Theme</c> —
/// <see cref="TuiDesktop"/> does not store a reference to the application or theme.
/// <para/>
/// To open a window, add it to the desktop via <see cref="TuiGroup.Add"/>.
/// To close it, remove it via <see cref="TuiGroup.Remove"/>.
/// </remarks>
public sealed class TuiDesktop : TuiGroup
{
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
}
