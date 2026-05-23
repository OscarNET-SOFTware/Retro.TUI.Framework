// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMouseButton.cs" company="OscarNET-SOFTware">
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
/// Identifies which mouse button is involved in a <see cref="TuiMouseEvent"/>.
/// </summary>
public enum TuiMouseButton
{
    /// <summary>No button — used for <see cref="TuiMouseAction.Move"/> and <see cref="TuiMouseAction.Wheel"/> events.</summary>
    None,

    /// <summary>The primary (left) mouse button.</summary>
    Left,

    /// <summary>The secondary (right) mouse button.</summary>
    Right,

    /// <summary>The middle mouse button (scroll wheel click).</summary>
    Middle,
}
