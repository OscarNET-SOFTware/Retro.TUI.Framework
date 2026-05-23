// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiFocusAction.cs" company="OscarNET-SOFTware">
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
/// Identifies the direction of a focus change conveyed by a <see cref="TuiFocusEvent"/>.
/// </summary>
public enum TuiFocusAction
{
    /// <summary>The view or control has received keyboard focus.</summary>
    Gained,

    /// <summary>The view or control has lost keyboard focus.</summary>
    Lost,
}
