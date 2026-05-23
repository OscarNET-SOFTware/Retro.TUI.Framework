// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMouseAction.cs" company="OscarNET-SOFTware">
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
/// Identifies the nature of a mouse interaction conveyed by a <see cref="TuiMouseEvent"/>.
/// </summary>
public enum TuiMouseAction
{
    /// <summary>The mouse cursor moved without any button being pressed.</summary>
    Move,

    /// <summary>A mouse button was pressed down.</summary>
    ButtonDown,

    /// <summary>A previously pressed mouse button was released.</summary>
    ButtonUp,

    /// <summary>A complete press-and-release cycle occurred on the same position.</summary>
    Click,

    /// <summary>Two <see cref="Click"/> events occurred in rapid succession.</summary>
    DoubleClick,

    /// <summary>The mouse scroll wheel was rotated.</summary>
    Wheel,
}
