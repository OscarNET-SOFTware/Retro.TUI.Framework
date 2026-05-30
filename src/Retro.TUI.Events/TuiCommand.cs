// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiCommand.cs" company="OscarNET-SOFTware">
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
/// High-level semantic command identifiers, independent of the raw input source.
/// </summary>
/// <remarks>
/// Commands enable decoupled communication: a button, a function key and a menu item
/// can all dispatch the same <see cref="TuiCommandEvent"/> carrying the same
/// <see cref="TuiCommand"/> value, without any component knowing about the others.
/// <para/>
/// Values <c>0</c>–<c>99</c> are reserved by the framework.
/// Application-defined commands must start at <see cref="UserDefined"/> (<c>100</c>).
/// </remarks>
public enum TuiCommand
{
    // ── System commands (framework-reserved: 0–9) ─────────────────────────

    /// <summary>No command — default / unset value.</summary>
    None = 0,

    /// <summary>Requests the application to quit.</summary>
    Quit = 1,

    /// <summary>Closes the active window or dialog.</summary>
    Close = 2,

    /// <summary>Opens context-sensitive help.</summary>
    Help = 3,

    /// <summary>Confirms a dialog with an OK result.</summary>
    Ok = 4,

    /// <summary>Cancels a dialog.</summary>
    Cancel = 5,

    /// <summary>Answers "Yes" to a question dialog.</summary>
    Yes = 6,

    /// <summary>Answers "No" to a question dialog.</summary>
    No = 7,

    /// <summary>
    /// Notifies that the host window has been resized by the operating system
    /// or window manager.
    /// </summary>
    /// <remarks>
    /// Posted by <c>SdlHost</c> only when <c>TuiHostOptions.Resizable</c> is
    /// <see langword="true"/>. When <c>Resizable</c> is <see langword="false"/> the host
    /// vetoes the resize by restoring the original dimensions and no event is posted.
    /// <para/>
    /// The <c>Parameter</c> field of the accompanying <see cref="TuiCommandEvent"/>
    /// carries a <c>(int Width, int Height)</c> value tuple with the new physical pixel
    /// dimensions so that consumers can update their layout without querying the host.
    /// </remarks>
    Resize = 8,

    // ── Window commands (framework-reserved: 10–19) ───────────────────────

    /// <summary>Toggles the active window between its normal and maximised sizes.</summary>
    ZoomWindow = 10,

    /// <summary>Enters interactive window-resize mode.</summary>
    ResizeWindow = 11,

    /// <summary>Enters interactive window-move mode.</summary>
    MoveWindow = 12,

    /// <summary>Switches focus to the next window in Z-order.</summary>
    NextWindow = 13,

    /// <summary>Switches focus to the previous window in Z-order.</summary>
    PrevWindow = 14,

    // ── Application range (values 100 and above) ──────────────────────────

    /// <summary>
    /// First value available for application-defined commands.
    /// Applications should define their own enum starting at this value or higher.
    /// </summary>
    UserDefined = 100,
}
