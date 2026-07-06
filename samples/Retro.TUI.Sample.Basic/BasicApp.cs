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
using Retro.TUI.Theming;
using Retro.TUI.Views;
using Retro.TUI.Widgets;
using Retro.TUI.Windows;

namespace Retro.TUI.Sample.Basic;

/// <summary>
/// Sample application that renders all 256 EGA foreground × background color
/// combinations as a <see cref="TuiLabel"/> grid inside a <see cref="TuiWindow"/>,
/// validating <see cref="TuiLabel.ForegroundColor"/> and
/// <see cref="TuiLabel.BackgroundColor"/> against the fixed EGA palette.
/// </summary>
/// <remarks>
/// <b>Layout:</b> a 66×18 palette window showing 16 background rows × 16 foreground
/// columns, each cell displaying " Hi " in that fg/bg combination.
/// A small info window sits above to confirm z-order and active/inactive palette.
/// <para/>
/// <b>Controls:</b>
/// <list type="bullet">
///   <item><description>Drag a window by its title bar to reposition it.</description></item>
///   <item><description>Click a window to bring it to the front.</description></item>
///   <item><description>Click <c>[-]</c> on a title bar to open a confirmation dialog.</description></item>
///   <item><description>Press <b>Enter</b> in the dialog to close the window.</description></item>
///   <item><description>Press <b>Escape</b> in the dialog to cancel.</description></item>
///   <item><description>Press <b>Escape</b> with no modal open to exit.</description></item>
/// </list>
/// </remarks>
internal sealed class BasicApp : TuiApplication
{
    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        Desktop.Add(BuildInfoWindow());
        Desktop.Add(BuildPaletteWindow());
    }

    /// <inheritdoc/>
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

    // ── Window builders ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a small window at the top-right corner to validate z-order and
    /// active/inactive title bar palette alongside the palette window.
    /// </summary>
    private static TuiWindow BuildInfoWindow()
    {
        var window = new TuiWindow(
            title: "Info",
            col: 69, row: 1,
            width: 10, height: 5);

        window.Add(new TuiLabel
        {
            Text = "ESC=Exit",
            Col = 1,
            Row = 1,
            Width = 8,
            Height = 1,
        });

        window.Add(new TuiLabel
        {
            Text = "16 × 16",
            Col = 1,
            Row = 2,
            Width = 8,
            Height = 1,
        });

        window.Add(new TuiLabel
        {
            Text = "256 EGA",
            Col = 1,
            Row = 3,
            Width = 8,
            Height = 1,
            ForegroundColor = TuiEgaColor.Yellow,
        });

        return window;
    }

    /// <summary>
    /// Builds a 66×18 window containing all 256 EGA fg × bg combinations.
    /// Rows iterate over background colors (0–15); columns over foreground colors (0–15).
    /// Each cell is 4 columns wide and displays " Hi " — the padding makes the
    /// background color the dominant visual element, functioning as a true color swatch.
    /// </summary>
    private static TuiWindow BuildPaletteWindow()
    {
        var window = new TuiWindow(
            title: "EGA Palette — 16 foreground × 16 background = 256 combinations",
            col: 1, row: 5,
            width: 66, height: 18);

        TuiEgaColor[] colors = Enum.GetValues<TuiEgaColor>();

        for (int bg = 0; bg < 16; bg++)
        {
            for (int fg = 0; fg < 16; fg++)
            {
                window.Add(new TuiLabel
                {
                    Text = " Hi ",
                    Col = 1 + fg * 4,
                    Row = 1 + bg,
                    Width = 4,
                    Height = 1,
                    ForegroundColor = colors[fg],
                    BackgroundColor = colors[bg],
                });
            }
        }

        return window;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Opens a <see cref="ConfirmCloseDialog"/> over the frontmost window.
    /// If the user confirms, removes that window from the desktop.
    /// </summary>
    private void HandleCloseCommand()
    {
        TuiView? target = Desktop.Frontmost;
        if (target is null)
            return;

        var dialog = new ConfirmCloseDialog();
        TuiCommand result = RunModal(dialog, Desktop);

        if (result == TuiCommand.Ok)
            Desktop.Remove(target);
    }
}
