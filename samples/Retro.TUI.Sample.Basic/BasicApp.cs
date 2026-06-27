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
using Retro.TUI.Views;
using Retro.TUI.Windows;

namespace Retro.TUI.Sample.Basic;

/// <summary>
/// Minimal application that places two draggable <see cref="TuiWindow"/> instances
/// on the desktop to validate title bar, border, shadow, active/inactive palette,
/// z-order and modal stack behaviour against the PC Tools 9.x visual reference.
/// </summary>
/// <remarks>
/// <b>Controls:</b>
/// <list type="bullet">
///   <item><description>Drag a window by its title bar to reposition it.</description></item>
///   <item><description>Click a window to bring it to the front (z-order activation).</description></item>
///   <item><description>Click <c>[-]</c> on a window title bar to open a confirmation dialog.</description></item>
///   <item><description>Press <b>Enter</b> in the dialog to close the window.</description></item>
///   <item><description>Press <b>Escape</b> in the dialog to cancel.</description></item>
///   <item><description>Press <b>Escape</b> with no modal open to exit the application.</description></item>
/// </list>
/// </remarks>
internal sealed class BasicApp : TuiApplication
{
    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        // Window A — positioned top-left, starts as the back window.
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
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="TuiCommand.Close"/>: opens a <see cref="ConfirmCloseDialog"/>
    ///     over the frontmost window. If the user confirms, the frontmost window is
    ///     removed from the desktop. The frontmost window is always the one that
    ///     emitted Close, because <see cref="TuiGroup.BringToFront"/> is called on
    ///     <c>ButtonDown</c> before the <c>[-]</c> glyph emits the command.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiCommand.Cancel"/> (Escape with no modal open): exits the
    ///     application.
    ///   </description></item>
    /// </list>
    /// </remarks>
    protected override void OnCommand(TuiCommandEvent ev)
    {
        switch (ev.Command)
        {
            case TuiCommand.Close:
                HandleCloseCommand();
                break;

            case TuiCommand.Cancel:
                RequestQuit();
                break;

            default:
                base.OnCommand(ev);
                break;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Opens a <see cref="ConfirmCloseDialog"/> over the frontmost window.
    /// If the user confirms, removes that window from the desktop.
    /// </summary>
    private void HandleCloseCommand()
    {
        // The frontmost child is always the window that emitted Close:
        // BringToFront is called on ButtonDown before [-] emits the command.
        TuiView? target = Desktop.Frontmost;
        if (target is null)
            return;

        var dialog = new ConfirmCloseDialog();
        TuiCommand result = RunModal(dialog, Desktop);

        if (result == TuiCommand.Ok)
            Desktop.Remove(target);
    }
}
