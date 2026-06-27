// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfirmCloseDialog.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Events;
using Retro.TUI.Windows;

namespace Retro.TUI.Sample.Basic;

/// <summary>
/// A minimal confirmation dialog that asks the user whether to close a window.
/// </summary>
/// <remarks>
/// This dialog has no widget controls yet (those arrive in M4). The available
/// actions are communicated through the title bar itself and handled via keyboard:
/// <list type="bullet">
///   <item><description>Enter → <see cref="TuiCommand.Ok"/> (confirm close)</description></item>
///   <item><description>Escape → <see cref="TuiCommand.Cancel"/> (keep window open)</description></item>
/// </list>
/// It serves as a reference pattern for modal dialogs until proper button
/// widgets are available.
/// </remarks>
internal sealed class ConfirmCloseDialog()
    : TuiDialog(
        title: "Close window?  [Enter] Yes  [Esc] No",
        col: 15, row: 8,
        width: 42, height: 6)
{
    /// <inheritdoc/>
    /// <remarks>
    /// Handles Enter → <see cref="TuiCommand.Ok"/> and lets Escape reach the
    /// message loop as <see cref="TuiCommand.Cancel"/>, which the modal routing
    /// in <c>TuiMessageLoop</c> delivers back here as a
    /// <see cref="TuiCommandEvent"/>.
    /// </remarks>
    public override bool HandleEvent(TuiEvent ev)
    {
        // Enter key — confirm close.
        if (ev is TuiKeyEvent { Key: TuiKey.Enter })
        {
            Close(TuiCommand.Ok);
            return true;
        }

        // TuiCommand.Cancel arrives here when Escape is pressed while this
        // dialog is the active modal (routed by TuiMessageLoop.DispatchKeyEvent).
        if (ev is TuiCommandEvent { Command: TuiCommand.Cancel })
        {
            Close(TuiCommand.Cancel);
            return true;
        }

        return base.HandleEvent(ev);
    }
}
