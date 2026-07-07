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
/// Sample application that validates <see cref="TuiLabel"/>, <see cref="TuiButton"/>
/// and the EGA palette visually.
/// </summary>
/// <remarks>
/// <b>Controls:</b>
/// <list type="bullet">
///   <item><description>Drag a window by its title bar to reposition it.</description></item>
///   <item><description>Click a window to bring it to the front.</description></item>
///   <item><description><b>Tab / Shift+Tab</b> — cycle focus between buttons.</description></item>
///   <item><description><b>Enter</b> or click — activate the focused button.</description></item>
///   <item><description>Click <c>[-]</c> on a title bar to open a confirmation dialog.</description></item>
///   <item><description><b>Escape</b> with no modal open — exit.</description></item>
/// </list>
/// </remarks>
internal sealed class BasicApp : TuiApplication
{
    private TuiLabel? _statusLabel;

    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        // Palette and Info go to the back; Button Demo is added last (frontmost).
        Desktop.Add(BuildInfoWindow());
        Desktop.Add(BuildPaletteWindow());
        Desktop.Add(BuildButtonDemoWindow());
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

            case TuiCommand.Ok:
            case TuiCommand.Yes:
            case TuiCommand.No:
            case TuiCommand.Abort:
            case TuiCommand.Retry:
            case TuiCommand.Ignore:
                _statusLabel?.Text = $"Last command: {ev.Command,-8}";
                break;

            default:
                base.OnCommand(ev);
                break;
        }
    }

    // ── Window builders ───────────────────────────────────────────────────────

    private static TuiWindow BuildInfoWindow()
    {
        var window = new TuiWindow(
            title: "Info",
            col: 69, row: 1,
            width: 10, height: 5);

        window.Add(new TuiLabel { Text = "ESC=Exit", Col = 1, Row = 1, Width = 8, Height = 1 });
        window.Add(new TuiLabel { Text = "16 × 16", Col = 1, Row = 2, Width = 8, Height = 1 });
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

    private static TuiWindow BuildPaletteWindow()
    {
        var window = new TuiWindow(
            title: " EGA Palette — 16 foreground × 16 background = 256 combinations",
            col: 1, row: 3,
            width: 66, height: 18);

        TuiEgaColor[] colors = Enum.GetValues<TuiEgaColor>();

        for (int bg = 0; bg < 16; bg++)
            for (int fg = 0; fg < 16; fg++)
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

        return window;
    }

    /// <summary>
    /// Button demo window — starts frontmost over the palette to validate:
    /// factory buttons, accelerator rendering, focus highlight (Tab/Shift+Tab),
    /// keyboard/mouse activation, color overrides and the status label update.
    /// Drag it aside to reveal the palette beneath (z-order test).
    /// </summary>
    private TuiWindow BuildButtonDemoWindow()
    {
        var window = new TuiWindow(
            title: " Button Demo",
            col: 6, row: 11,
            width: 72, height: 13);

        // ── Row 1: factory buttons group 1 — Ok, Cancel, Yes, No ─────────────
        // 4 × width 10, gap 4 between buttons: 4×10 + 3×4 = 52 cols.
        TuiButton[] group1 = [TuiButton.Ok, TuiButton.Cancel, TuiButton.Yes, TuiButton.No];
        for (int i = 0; i < group1.Length; i++)
        {
            group1[i].Col = 1 + i * 14;  // 10 width + 4 gap
            group1[i].Row = 2;
            group1[i].Width = 10;
            group1[i].Height = 1;
            window.Add(group1[i]);
        }

        // ── Row 3: factory buttons group 2 — Abort, Retry, Ignore ────────────
        // 3 × width 10, gap 4: 3×10 + 2×4 = 38 cols.
        TuiButton[] group2 = [TuiButton.Abort, TuiButton.Retry, TuiButton.Ignore];
        for (int i = 0; i < group2.Length; i++)
        {
            group2[i].Col = 1 + i * 14;
            group2[i].Row = 4;
            group2[i].Width = 10;
            group2[i].Height = 1;
            window.Add(group2[i]);
        }

        // ── Row 6: disabled button example ───────────────────────────────────
        window.Add(new TuiButton
        {
            Text = "~D~isabled",
            Command = TuiCommand.Ok,
            Col = 1,
            Row = 6,
            Width = 12,
            Height = 1,
            Enabled = false,
        });

        // ── Row 8: status label ───────────────────────────────────────────────
        _statusLabel = new TuiLabel
        {
            Text = "Last command: —           ",
            Col = 1,
            Row = 8,
            Width = 36,
            Height = 1,
            ForegroundColor = TuiEgaColor.BrightGreen,
        };
        window.Add(_statusLabel);

        // ── Row 10: hint ───────────────────────────────────────────────────────
        window.Add(new TuiLabel
        {
            Text = "Tab/Shift+Tab: move focus    Enter or click: activate",
            Col = 1,
            Row = 10,
            Width = 54,
            Height = 1,
        });

        return window;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

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
