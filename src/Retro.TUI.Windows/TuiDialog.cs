// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDialog.cs" company="OscarNET-SOFTware">
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
using Retro.TUI.Rendering;
using Retro.TUI.Theming;
using Retro.TUI.Views;

namespace Retro.TUI.Windows;

/// <summary>
/// A modal dialog window that blocks the application event loop until it is closed.
/// </summary>
/// <remarks>
/// <see cref="TuiDialog"/> extends <see cref="TuiWindow"/> with modal semantics.
/// It is never opened directly — use <c>TuiApplication.RunModal</c> to push the
/// dialog onto the desktop modal stack, run a nested event loop, and clean up
/// automatically when <see cref="Close"/> is called.
/// <para/>
/// <b>Closing:</b> call <see cref="Close"/> from any event handler inside the
/// dialog (e.g. an OK or Cancel button). The nested loop started by
/// <c>TuiApplication.RunModal</c> will exit on the next iteration and the caller
/// will receive the <see cref="TuiCommand"/> result passed to <see cref="Close"/>.
/// <para/>
/// <b>Escape:</b> the message loop routes <see cref="TuiCommand.Cancel"/> to the
/// active modal rather than to the application. Override
/// <see cref="TuiWindow.HandleEvent"/> to react to it.
/// </remarks>
/// <remarks>
/// Initializes a new <see cref="TuiDialog"/> with the given title and bounds.
/// </remarks>
/// <param name="title">The text shown in the title bar. Must not be <see langword="null"/>.</param>
/// <param name="col">Column offset relative to the parent's origin.</param>
/// <param name="row">Row offset relative to the parent's origin.</param>
/// <param name="width">Width in grid columns, including the border.</param>
/// <param name="height">Height in grid rows, including the title bar and border.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="title"/> is <see langword="null"/>.
/// </exception>
public class TuiDialog(string title, int col, int row, int width, int height)
    : TuiWindow(title, col, row, width, height), IModalDialog
{
    // ── Modal result ──────────────────────────────────────────────────────────

    // Backing field for the result set by Close(). IDE0032 suppressed: the field
    // is written from Close() and read by TuiApplication.RunModal — not a simple
    // auto-property pattern.
#pragma warning disable IDE0032
    private TuiCommand _result = TuiCommand.Cancel;
#pragma warning restore IDE0032

    /// <summary>
    /// Gets the command result set by the most recent call to <see cref="Close"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="TuiCommand.Cancel"/> — the safe "nothing happened"
    /// result if the dialog is closed without an explicit <see cref="Close"/> call
    /// (e.g. the user pressed Escape).
    /// Read by <c>TuiApplication.RunModal</c> after the nested loop exits.
    /// </remarks>
    public TuiCommand Result => _result;

    /// <summary>
    /// Gets a value indicating whether <see cref="Close"/> has been called and the
    /// nested loop should exit.
    /// </summary>
    /// <remarks>
    /// Set to <see langword="true"/> by <see cref="Close"/>. Polled each iteration
    /// by <c>TuiApplication.RunModal</c> to detect that the dialog requested
    /// termination and the nested loop should stop.
    /// </remarks>
    public bool CloseRequested { get; private set; }

    /// <summary>
    /// Signals that the dialog should close, returning <paramref name="result"/>
    /// to the caller of <c>TuiApplication.RunModal</c>.
    /// </summary>
    /// <param name="result">
    /// The command to return. Defaults to <see cref="TuiCommand.Cancel"/>.
    /// </param>
    /// <remarks>
    /// This method only sets the close flag and result — it does not remove the
    /// dialog from the desktop or pop the modal stack. That cleanup is performed
    /// by <c>TuiApplication.RunModal</c> after the nested loop exits.
    /// <para/>
    /// Calling <see cref="Close"/> more than once is safe: only the first call
    /// sets the result; subsequent calls are no-ops.
    /// </remarks>
    public void Close(TuiCommand result = TuiCommand.Cancel)
    {
        if (CloseRequested)
            return;

        _result = result;
        CloseRequested = true;
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Uses <see cref="TuiColorRole.DialogTitleBackground"/> and related
    /// <c>Dialog*</c> roles instead of the <c>Window*</c> equivalents
    /// so that the theming system can assign distinct colors to dialogs
    /// independently of regular windows.
    /// </remarks>
    protected override void DrawTitleBar(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        // Fill the entire title bar row with the dialog title background.
        ctx.FillRect(AbsCol, AbsRow, Width, 1, TuiColorRole.DialogTitleBackground);

        // Title text, centered over the remaining width (columns 2..Width-2),
        // leaving column 1 as visual separation from the system-menu glyph.
        if (Width > 1)
            ctx.DrawTextCentered(AbsCol + 2, AbsRow, Width - 2, Title,
                     TuiColorRole.DialogTitleForeground,
                     TuiColorRole.DialogTitleBackground);

        // System-menu close glyph — uses dialog title roles.
        ctx.DrawSystemMenuGlyph(AbsCol, AbsRow,
                                TuiColorRole.DialogTitleForeground,
                                TuiColorRole.DialogTitleBackground);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses <see cref="TuiColorRole.DialogBackground"/> and
    /// <see cref="TuiColorRole.DialogBorder"/> instead of the
    /// <c>Window*</c> equivalents.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (ShowShadow)
            ctx.DrawShadow(AbsCol, AbsRow, Width, Height);

        ctx.FillRect(AbsCol, AbsRow, Width, Height, TuiColorRole.DialogBackground);
        ctx.DrawBorder(AbsCol, AbsRow, Width, Height, TuiColorRole.DialogBorder);

        if (ShowTitle)
            DrawTitleBar(ctx);

        ctx.PushClip(InnerCol, InnerRow, InnerWidth, InnerHeight);
        try
        {
            DrawChildViews(ctx);
        }
        finally
        {
            ctx.PopClip();
        }
    }
}
