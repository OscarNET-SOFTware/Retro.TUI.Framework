// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiKeyEvent.cs" company="OscarNET-SOFTware">
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
/// Represents a keyboard event: either a special key press or a printable character input.
/// </summary>
/// <param name="Key">
/// The special key that was pressed, or <see cref="TuiKey.None"/> when the event
/// carries only a printable character.
/// </param>
/// <param name="KeyChar">
/// The printable character associated with the key press, or <c>'\0'</c> when
/// the event carries only a special key (i.e. <paramref name="Key"/> is not <see cref="TuiKey.None"/>).
/// </param>
/// <param name="Modifiers">
/// Modifier keys (Shift, Control, Alt) held at the time of the event.
/// </param>
/// <remarks>
/// A single keystroke always produces exactly one <see cref="TuiKeyEvent"/>.
/// The host maps raw SDL2 key symbols to this record and posts it to the
/// <see cref="TuiEventQueue"/>.
/// </remarks>
public sealed record TuiKeyEvent(
    TuiKey Key,
    char KeyChar,
    TuiModifiers Modifiers
) : TuiEvent;
