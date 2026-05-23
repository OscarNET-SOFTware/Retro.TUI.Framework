// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiCommandEvent.cs" company="OscarNET-SOFTware">
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
/// Represents a high-level command event that enables decoupled communication between components.
/// </summary>
/// <param name="Command">The semantic command being dispatched.</param>
/// <param name="Parameter">
/// An optional payload associated with the command, or <see langword="null"/> when no
/// additional data is required.
/// </param>
/// <remarks>
/// <see cref="TuiCommandEvent"/> is the primary mechanism for component-to-component
/// communication without hard dependencies. A button, a menu item and a keyboard shortcut
/// can all dispatch the same command value and be handled uniformly by the target view.
/// <para/>
/// Application-defined commands must use values starting at <see cref="TuiCommand.UserDefined"/>.
/// </remarks>
public sealed record TuiCommandEvent(
    TuiCommand Command,
    object? Parameter = null
) : TuiEvent;
