// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDesktopPattern.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Theming;

/// <summary>
/// Defines the repeating pattern rendered over the desktop background.
/// </summary>
/// <remarks>
/// The background fill color is always <see cref="TuiColorRole.DesktopBackground"/>.
/// When a pattern other than <see cref="None"/> is active, individual dots or lines
/// are drawn using <see cref="TuiColorRole.DesktopPatternDot"/>.
/// </remarks>
public enum TuiDesktopPattern
{
    /// <summary>Solid background fill — no pattern is rendered.</summary>
    None,

    /// <summary>
    /// Regular grid of single-pixel dots.
    /// The classic Norton Commander / PC Tools retro aesthetic.
    /// </summary>
    DotGrid,

    /// <summary>Alternating filled and empty cells, like a chess board.</summary>
    Checkerboard,

    /// <summary>Evenly spaced horizontal lines.</summary>
    HorizontalLines,

    /// <summary>Evenly spaced vertical lines.</summary>
    VerticalLines,
}
