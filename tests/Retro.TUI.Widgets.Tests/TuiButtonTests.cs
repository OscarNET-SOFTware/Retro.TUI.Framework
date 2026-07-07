// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiButtonTests.cs" company="OscarNET-SOFTware">
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
using Retro.TUI.Views;

using SkiaSharp;

namespace Retro.TUI.Widgets;

/// <summary>
/// Tests for <see cref="TuiButton"/>: construction defaults, property contracts,
/// event handling, factory properties, and the <see cref="TuiButton.Draw"/> contract.
/// </summary>
public sealed class TuiButtonTests
{
    // ── Construction defaults ────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultText_IsEmpty()
    {
        var btn = new TuiButton();
        Assert.Equal(string.Empty, btn.Text);
    }

    [Fact]
    public void Constructor_DefaultCommand_IsOk()
    {
        var btn = new TuiButton();
        Assert.Equal(TuiCommand.Ok, btn.Command);
    }

    [Fact]
    public void Constructor_IsFocusable()
    {
        var btn = new TuiButton();
        Assert.True(btn.Focusable);
    }

    [Fact]
    public void Constructor_HasCustomMouseHandling()
    {
        var btn = new TuiButton();
        Assert.True(btn.HasCustomMouseHandling);
    }

    [Fact]
    public void Constructor_DefaultHeight_IsOne()
    {
        var btn = new TuiButton();
        Assert.Equal(1, btn.Height);
    }

    // ── Text property ─────────────────────────────────────────────────────────

    [Fact]
    public void Text_SetNull_ThrowsArgumentNullException()
    {
        var btn = new TuiButton();
        Assert.Throws<ArgumentNullException>(() => btn.Text = null!);
    }

    [Fact]
    public void Text_SetDifferentValue_UpdatesAndInvalidates()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var btn = new TuiButton { Text = "~O~K", Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(btn);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(btn.IsDirty);

        btn.Text = "~C~ancel";

        Assert.Equal("~C~ancel", btn.Text);
        Assert.True(btn.IsDirty);
    }

    [Fact]
    public void Text_SetSameValue_DoesNotInvalidate()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var btn = new TuiButton { Text = "~O~K", Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(btn);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(btn.IsDirty);

        btn.Text = "~O~K";

        Assert.False(btn.IsDirty);
    }

    // ── Focus event handling ──────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_FocusGained_InvalidatesAndReturnsFalse()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var btn = new TuiButton { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(btn);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(btn.IsDirty);

        bool consumed = btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        Assert.False(consumed);  // focus events never consumed
        Assert.True(btn.IsDirty);
    }

    [Fact]
    public void HandleEvent_FocusLost_InvalidatesAndReturnsFalse()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var btn = new TuiButton { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(btn);

        // Give focus first so Lost actually changes state.
        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));
        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(btn.IsDirty);

        bool consumed = btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Lost));

        Assert.False(consumed);
        Assert.True(btn.IsDirty);
    }

    [Fact]
    public void HandleEvent_FocusGained_Twice_DoesNotInvalidateSecondTime()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var btn = new TuiButton { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(btn);

        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));
        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(btn.IsDirty);

        // Second Gained with no Lost in between — no state change.
        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        Assert.False(btn.IsDirty);
    }

    // ── Keyboard activation ───────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_EnterWhenFocused_EmitsCommandAndReturnsTrue()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 2,
            Width = 10,
            Height = 1,
        };

        var desktop = new TuiDesktop();
        var received = new List<TuiCommandEvent>();
        desktop.CommandSink = received.Add;
        desktop.Add(btn);

        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        bool consumed = btn.HandleEvent(
            new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None));

        Assert.True(consumed);
        Assert.Single(received);
        Assert.Equal(TuiCommand.Ok, received[0].Command);
    }

    [Fact]
    public void HandleEvent_EnterWhenNotFocused_DoesNotEmitCommand()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 0,
            Width = 10,
            Height = 1,
        };

        var parent = new RecordingGroup();
        parent.Add(btn);

        // No focus granted — button should ignore Enter.
        bool consumed = btn.HandleEvent(
            new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None));

        Assert.False(consumed);
        Assert.Empty(parent.ReceivedEvents);
    }

    // ── Mouse activation ──────────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_MouseButtonDown_EmitsCommandAndReturnsTrue()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 2,
            Width = 10,
            Height = 1,
        };

        var desktop = new TuiDesktop();
        var received = new List<TuiCommandEvent>();
        desktop.CommandSink = received.Add;
        desktop.Add(btn);

        bool consumed = btn.HandleEvent(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, 0, 0, 0f, 0f));

        Assert.True(consumed);
        Assert.Single(received);
        Assert.Equal(TuiCommand.Ok, received[0].Command);
    }

    [Fact]
    public void HandleEvent_MouseButtonUp_DoesNotEmitCommand()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 0,
            Width = 10,
            Height = 1,
        };

        var parent = new RecordingGroup();
        parent.Add(btn);

        bool consumed = btn.HandleEvent(
            new TuiMouseEvent(TuiMouseAction.ButtonUp, 0, 0, 0f, 0f));

        Assert.False(consumed);
        Assert.Empty(parent.ReceivedEvents);
    }

    // ── Factory properties ────────────────────────────────────────────────────

    [Theory]
    [InlineData(nameof(TuiButton.Ok), TuiCommand.Ok)]
    [InlineData(nameof(TuiButton.Cancel), TuiCommand.Cancel)]
    [InlineData(nameof(TuiButton.Yes), TuiCommand.Yes)]
    [InlineData(nameof(TuiButton.No), TuiCommand.No)]
    [InlineData(nameof(TuiButton.Abort), TuiCommand.Abort)]
    [InlineData(nameof(TuiButton.Retry), TuiCommand.Retry)]
    [InlineData(nameof(TuiButton.Ignore), TuiCommand.Ignore)]
    public void FactoryProperty_ReturnsNewInstanceWithCorrectCommand(
        string propertyName, TuiCommand expectedCommand)
    {
        var prop = typeof(TuiButton).GetProperty(propertyName)!;
        var btn = (TuiButton)prop.GetValue(null)!;

        Assert.Equal(expectedCommand, btn.Command);
        Assert.NotEmpty(btn.Text);
    }

    [Fact]
    public void FactoryProperty_Ok_ReturnsNewInstanceEachTime()
    {
        var a = TuiButton.Ok;
        var b = TuiButton.Ok;
        Assert.NotSame(a, b);
    }

    // ── Draw contract ─────────────────────────────────────────────────────────

    [Fact]
    public void Draw_NullContext_ThrowsArgumentNullException()
    {
        var btn = new TuiButton();
        Assert.Throws<ArgumentNullException>(() => btn.Draw(null!));
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "~O~K",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
        };

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_WithFocus_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "~O~K",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
        };
        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_WidthBelowMinimum_ClampsToMinWidth_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "~O~K",
            Col = 2,
            Row = 2,
            Width = 4,
            Height = 1, // below MinWidth
        };

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_TextWithoutAccelerator_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "OK",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
        };

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_Invisible_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "~O~K",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
            Visible = false,
        };

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }

    // ── Test doubles ──────────────────────────────────────────────────────────

    /// <summary>
    /// A <see cref="TuiGroup"/> that records events passed to
    /// <see cref="HandleEvent"/> by its children, used to verify command emission.
    /// </summary>
    private sealed class RecordingGroup : TuiGroup
    {
        public List<TuiEvent> ReceivedEvents { get; } = [];

        public override bool HandleEvent(TuiEvent ev)
        {
            ReceivedEvents.Add(ev);
            return false;
        }
    }

    // ── Test infrastructure ───────────────────────────────────────────────────

    private static SKSurface CreateSurface(int w, int h)
        => SKSurface.Create(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));

    private static TuiRenderContext BuildContext(int cols, int rows)
    {
        int screenW = cols * 9;
        int screenH = rows * 16;

        using var skFont = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(screenW, screenH, skFont);

        var font = new TuiFont();
        font.Load(SKTypeface.Default, 16f);

        return new TuiRenderContext(grid, font);
    }

    // ── Disabled state ────────────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_EnterWhenDisabled_DoesNotEmitCommand()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 2,
            Width = 10,
            Height = 1,
            Enabled = false,
        };
        var parent = new RecordingGroup();
        parent.Add(btn);
        btn.HandleEvent(new TuiFocusEvent(TuiFocusAction.Gained));

        bool consumed = btn.HandleEvent(
            new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None));

        Assert.False(consumed);
        Assert.Empty(parent.ReceivedEvents);
    }

    [Fact]
    public void HandleEvent_MouseButtonDownWhenDisabled_DoesNotEmitCommand()
    {
        var btn = new TuiButton
        {
            Text = "~O~K",
            Command = TuiCommand.Ok,
            Col = 0,
            Row = 2,
            Width = 10,
            Height = 1,
            Enabled = false,
        };
        var parent = new RecordingGroup();
        parent.Add(btn);

        bool consumed = btn.HandleEvent(
            new TuiMouseEvent(TuiMouseAction.ButtonDown, 0, 0, 0f, 0f));

        Assert.False(consumed);
        Assert.Empty(parent.ReceivedEvents);
    }

    [Fact]
    public void Draw_DisabledButton_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var btn = new TuiButton
        {
            Text = "~O~K",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
            Enabled = false,
        };

        var ex = Record.Exception(() => ctx.RenderFrame(surface, c => btn.Draw(c)));
        Assert.Null(ex);
    }
}
