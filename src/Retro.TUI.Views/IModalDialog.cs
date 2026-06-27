// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IModalDialog.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Views;

/// <summary>
/// Minimal contract that a modal view must satisfy so that
/// <c>TuiApplication.RunModal</c> can drive and observe its lifecycle
/// without a dependency on <c>Retro.TUI.Windows</c>.
/// </summary>
/// <remarks>
/// <see cref="IModalDialog"/> is intentionally narrow: it exposes only the two
/// members that the nested event loop needs to poll. All visual behaviour
/// (title bar, border, drag) is provided by the concrete implementation
/// (<c>TuiDialog</c> in <c>Retro.TUI.Windows</c>).
/// <para/>
/// Implementors must also derive from <see cref="TuiView"/> so they can be
/// added to <see cref="TuiDesktop"/> via <see cref="TuiDesktop.PushModal"/>.
/// </remarks>
public interface IModalDialog
{
    /// <summary>
    /// Gets a value indicating whether the dialog has requested to be closed.
    /// </summary>
    /// <remarks>
    /// Set to <see langword="true"/> by the dialog's <c>Close</c> method.
    /// Polled each iteration by <c>TuiApplication.RunModal</c> to detect
    /// that the nested loop should stop.
    /// </remarks>
    bool CloseRequested { get; }

    /// <summary>
    /// Gets the command result that the dialog will return to its caller.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="TuiCommand.Cancel"/> before <c>Close</c> is called.
    /// Read by <c>TuiApplication.RunModal</c> after the nested loop exits.
    /// </remarks>
    TuiCommand Result { get; }
}
