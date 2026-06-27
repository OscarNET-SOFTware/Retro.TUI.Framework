// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDialogTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Windows;

public sealed class TuiDialogTests
{
    // ── Close / Result ────────────────────────────────────────────────────────

    [Fact]
    public void Close_SetsCloseRequestedTrue()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        dialog.Close();
        Assert.True(dialog.CloseRequested);
    }

    [Fact]
    public void Close_DefaultResult_IsCancel()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        dialog.Close();
        Assert.Equal(TuiCommand.Cancel, dialog.Result);
    }

    [Fact]
    public void Close_WithOkResult_ReturnsOk()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        dialog.Close(TuiCommand.Ok);
        Assert.Equal(TuiCommand.Ok, dialog.Result);
    }

    [Fact]
    public void Close_CalledTwice_SecondCallIsNoop()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        dialog.Close(TuiCommand.Ok);
        dialog.Close(TuiCommand.Cancel); // second call must not overwrite
        Assert.Equal(TuiCommand.Ok, dialog.Result);
    }

    [Fact]
    public void Result_BeforeClose_IsCancel()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        Assert.Equal(TuiCommand.Cancel, dialog.Result);
    }

    [Fact]
    public void CloseRequested_BeforeClose_IsFalse()
    {
        var dialog = new TuiDialog("Test", 0, 0, 20, 10);
        Assert.False(dialog.CloseRequested);
    }
}
