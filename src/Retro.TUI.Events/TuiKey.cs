// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiKey.cs" company="OscarNET-SOFTware">
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
/// Identifies a special (non-printable) key on the keyboard.
/// </summary>
/// <remarks>
/// Printable characters are conveyed via <see cref="TuiKeyEvent.KeyChar"/> instead.
/// <see cref="None"/> is used when the event is character-only and no special key applies.
/// </remarks>
public enum TuiKey
{
    /// <summary>No special key — the event carries a printable character only.</summary>
    None,

    // ── Navigation ────────────────────────────────────────────────────────
    /// <summary>Enter / Return key.</summary>
    Enter,

    /// <summary>Escape key.</summary>
    Escape,

    /// <summary>Tab key.</summary>
    Tab,

    /// <summary>Backspace key.</summary>
    BackSpace,

    /// <summary>Delete key.</summary>
    Delete,

    /// <summary>Insert key.</summary>
    Insert,

    /// <summary>Home key.</summary>
    Home,

    /// <summary>End key.</summary>
    End,

    /// <summary>Page Up key.</summary>
    PageUp,

    /// <summary>Page Down key.</summary>
    PageDown,

    // ── Arrow keys ────────────────────────────────────────────────────────
    /// <summary>Left arrow key.</summary>
    Left,

    /// <summary>Right arrow key.</summary>
    Right,

    /// <summary>Up arrow key.</summary>
    Up,

    /// <summary>Down arrow key.</summary>
    Down,

    // ── Function keys ─────────────────────────────────────────────────────
    /// <summary>F1 function key.</summary>
    F1,

    /// <summary>F2 function key.</summary>
    F2,

    /// <summary>F3 function key.</summary>
    F3,

    /// <summary>F4 function key.</summary>
    F4,

    /// <summary>F5 function key.</summary>
    F5,

    /// <summary>F6 function key.</summary>
    F6,

    /// <summary>F7 function key.</summary>
    F7,

    /// <summary>F8 function key.</summary>
    F8,

    /// <summary>F9 function key.</summary>
    F9,

    /// <summary>F10 function key.</summary>
    F10,

    /// <summary>F11 function key.</summary>
    F11,

    /// <summary>F12 function key.</summary>
    F12,
}
