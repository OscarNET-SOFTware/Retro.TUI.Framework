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
/// <remarks>
/// Coordinates are expressed in character-grid units (columns / rows), not pixels.
/// The rendering layer is responsible for the pixel-to-cell mapping.
/// </remarks>
public sealed record TuiMouseEvent(
    TuiMouseAction Action,
    int Col,
    int Row,
    TuiMouseButton Button
) : TuiEvent;
