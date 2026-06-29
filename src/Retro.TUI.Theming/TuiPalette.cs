// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiPalette.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

namespace Retro.TUI.Theming;

/// <summary>
/// Provides color resolution for all <see cref="TuiColorRole"/> values
/// using the framework's fixed EGA-based color scheme.
/// </summary>
/// <remarks>
/// The color scheme is fixed and cannot be customized at runtime.
/// All 16 base colors are defined in <see cref="TuiEgaPalette"/>.
/// The mapping from semantic roles to concrete colors is defined in
/// <see cref="TuiColorMap"/>.
/// </remarks>
public static class TuiPalette
{
    /// <summary>
    /// Resolves the <see cref="SKColor"/> assigned to <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The semantic color role to resolve.</param>
    /// <returns>The <see cref="SKColor"/> mapped to <paramref name="role"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when <paramref name="role"/> has no entry in the color map.
    /// </exception>
    public static SKColor Resolve(TuiColorRole role) => TuiColorMap.Resolve(role);
}
