// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicApp.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Core;
using Retro.TUI.Events;
using Retro.TUI.Windows;

namespace Retro.TUI.Sample.Basic;

/// <summary>
/// Minimal application that places two draggable <see cref="TuiWindow"/> instances
/// on the desktop to validate title bar, border, shadow, active/inactive palette
/// and z-order behaviour against the PC Tools 9.x visual reference.
/// </summary>
/// <remarks>
/// Press <b>Escape</b> or close the host window to exit.
/// Click on a window to bring it to the front (z-order activation).
/// Drag a window by its title bar to reposition it.
/// </remarks>
internal sealed class BasicApp : TuiApplication
{
    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        // Window A — positioned top-left, will start as the back window.
        var windowA = new TuiWindow(
            title: "Window A",
            col: 2, row: 2,
            width: 30, height: 10);

        // Window B — slightly offset so both title bars are visible.
        // Added last so it starts as the active (frontmost) window.
        var windowB = new TuiWindow(
            title: "Window B — Active",
            col: 10, row: 5,
            width: 35, height: 12);

        Desktop.Add(windowA);
        Desktop.Add(windowB);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Extends the base behaviour: <see cref="TuiCommand.Cancel"/> (Escape key)
    /// also exits the application in this sample. In production apps, Cancel
    /// typically closes only the active dialog or menu.
    /// </remarks>
    protected override void OnCommand(TuiCommandEvent ev)
    {
        if (ev.Command == TuiCommand.Cancel)
            RequestQuit();
        else
            base.OnCommand(ev);
    }
}
