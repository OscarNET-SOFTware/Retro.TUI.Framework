// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEvent.cs" company="OscarNET-SOFTware">
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
/// Base type for all framework events.
/// </summary>
/// <remarks>
/// All events are immutable <c>record</c> types that inherit from <see cref="TuiEvent"/>.
/// This guarantees immutability, structural equality and native pattern matching support.
/// <para/>
/// Event hierarchy:
/// <code>
/// TuiEvent  (abstract record)
/// ├── TuiKeyEvent
/// ├── TuiMouseEvent
/// ├── TuiCommandEvent
/// ├── TuiTimerEvent
/// └── TuiFocusEvent
/// </code>
/// </remarks>
public abstract record TuiEvent;
