// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMouseEvent.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Events;

/// <summary>
/// Represents a mouse event: cursor movement, button press/release, click or scroll wheel.
/// </summary>
/// <param name="Action">The type of mouse interaction that occurred.</param>
/// <param name="Col">
/// Horizontal position of the cursor in grid columns at the time of the event.
/// Column 0 is the leftmost column.
/// </param>
/// <param name="Row">
/// Vertical position of the cursor in grid rows at the time of the event.
/// Row 0 is the topmost row.
/// </param>
/// <param name="Button">
/// The mouse button involved in this event, or <see cref="TuiMouseButton.None"/>
/// for <see cref="TuiMouseAction.Move"/> and <see cref="TuiMouseAction.Wheel"/> events.
/// </param>
/// <param name="PixelX">
/// Horizontal position of the cursor in screen pixels at the time of the event.
/// Pixel 0 is the leftmost column of the window.
/// </param>
/// <param name="PixelY">
/// Vertical position of the cursor in screen pixels at the time of the event.
/// Pixel 0 is the topmost row of the window.
/// </param>
/// <remarks>
/// <b>Coordinate contract:</b> the host (<c>SdlHost</c>) posts this event with
/// <see cref="Col"/> and <see cref="Row"/> set to <c>0</c> and <see cref="PixelX"/> /
/// <see cref="PixelY"/> set to the real screen-pixel position reported by SDL2.
/// <c>0</c> is used as an explicit "not yet computed" placeholder rather than a
/// plausible-looking pixel value, to avoid the appearance of a valid grid coordinate.
/// <para/>
/// <c>Retro.TUI.Core.TuiMessageLoop</c> recomputes <see cref="Col"/> and
/// <see cref="Row"/> from <see cref="PixelX"/> / <see cref="PixelY"/> using
/// <c>TuiGrid.CellCol</c> / <c>TuiGrid.CellRow</c> before dispatching the event to
/// the view tree. Code that consumes <see cref="Col"/> / <see cref="Row"/> from an
/// event inside <c>HandleEvent</c> always sees the recomputed, correct grid
/// coordinates — only code observing the raw event between <c>SdlHost</c> posting it
/// and <c>TuiMessageLoop</c> recomputing it (e.g. host-level tests) sees the
/// placeholder <c>0</c>.
/// </remarks>
public sealed record TuiMouseEvent(
    TuiMouseAction Action,
    int Col,
    int Row,
    TuiMouseButton Button,
    float PixelX = 0f,
    float PixelY = 0f
) : TuiEvent;
