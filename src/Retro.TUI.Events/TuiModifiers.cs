// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiModifiers.cs" company="OscarNET-SOFTware">
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
/// Keyboard modifier keys that may accompany a <see cref="TuiKeyEvent"/>.
/// </summary>
/// <remarks>
/// This is a bit-flags enum; values can be combined with the bitwise OR operator.
/// For example, <c>TuiModifiers.Control | TuiModifiers.Shift</c> represents Ctrl+Shift.
/// </remarks>
[Flags]
public enum TuiModifiers
{
    /// <summary>No modifier key is held.</summary>
    None = 0,

    /// <summary>The Shift key is held.</summary>
    Shift = 1 << 0,

    /// <summary>The Control key is held.</summary>
    Control = 1 << 1,

    /// <summary>The Alt key is held.</summary>
    Alt = 1 << 2,
}
