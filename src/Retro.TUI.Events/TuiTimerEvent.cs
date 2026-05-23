// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiTimerEvent.cs" company="OscarNET-SOFTware">
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
/// Represents a periodic timer event emitted by the message loop on every frame tick.
/// </summary>
/// <param name="Elapsed">
/// The total time elapsed since the application started, as reported by
/// the message loop's high-resolution timer.
/// </param>
/// <remarks>
/// Views that need to perform periodic updates (e.g. a clock display or animated cursor)
/// should handle <see cref="TuiTimerEvent"/> rather than managing their own timers.
/// The message loop is the single source of timing truth.
/// </remarks>
public sealed record TuiTimerEvent(
    TimeSpan Elapsed
) : TuiEvent;
