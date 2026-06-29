// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDesktopTests.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

namespace Retro.TUI.Views;

/// <summary>
/// Tests for <see cref="TuiDesktop"/>: inheritance contracts, child management
/// delegation and default state.
/// </summary>
public sealed class TuiDesktopTests
{
    // ── Inheritance ───────────────────────────────────────────────────────────

    [Fact]
    public void TuiDesktop_IsAssignableFrom_TuiGroup()
    {
        var desktop = new TuiDesktop();

        Assert.IsType<TuiGroup>(desktop, exactMatch: false);
    }

    [Fact]
    public void TuiDesktop_IsAssignableFrom_TuiView()
    {
        var desktop = new TuiDesktop();

        Assert.IsType<TuiView>(desktop, exactMatch: false);
    }

    // ── Default state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewDesktop_HasNoParent()
    {
        var desktop = new TuiDesktop();

        Assert.Null(desktop.Parent);
    }

    [Fact]
    public void NewDesktop_IsVisible()
    {
        var desktop = new TuiDesktop();

        Assert.True(desktop.Visible);
    }

    // ── Child management (delegates to TuiGroup) ──────────────────────────────

    [Fact]
    public void Add_Child_SetsParentToDesktop()
    {
        var desktop = new TuiDesktop();
        var child = new TuiGroup();

        desktop.Add(child);

        Assert.Same(desktop, child.Parent);
    }

    [Fact]
    public void Remove_Child_ClearsParent()
    {
        var desktop = new TuiDesktop();
        var child = new TuiGroup();
        desktop.Add(child);

        bool removed = desktop.Remove(child);

        Assert.True(removed);
        Assert.Null(child.Parent);
    }

    // ── FindAt (inherited hit-testing) ────────────────────────────────────────

    [Fact]
    public void FindAt_ChildAtPosition_ReturnsChild()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiGroup { Col = 10, Row = 5, Width = 40, Height = 15 };
        desktop.Add(window);

        TuiView? hit = desktop.FindAt(absCol: 20, absRow: 10);

        Assert.Same(window, hit);
    }

    [Fact]
    public void FindAt_PointOutsideAllChildren_ReturnsNull()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };

        TuiView? hit = desktop.FindAt(absCol: 5, absRow: 5);

        Assert.Null(hit);
    }

    // ── Modal stack ───────────────────────────────────────────────────────────

    [Fact]
    public void HasModal_WhenEmpty_IsFalse()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        Assert.False(desktop.HasModal);
    }

    [Fact]
    public void ActiveModal_WhenEmpty_IsNull()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        Assert.Null(desktop.ActiveModal);
    }

    [Fact]
    public void PushModal_SetsHasModalTrue_AndAddsChild()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var dialog = new StubView { Col = 0, Row = 0, Width = 20, Height = 10 };

        desktop.PushModal(dialog);

        Assert.True(desktop.HasModal);
        Assert.Equal(dialog, desktop.ActiveModal);
        // Verify the dialog participates in hit-testing (i.e. it was added as a child).
        Assert.Equal(dialog, desktop.FindAt(0, 0));
    }

    [Fact]
    public void PopModal_RemovesTopModal_AndChild()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var dialog = new StubView { Col = 0, Row = 0, Width = 20, Height = 10 };
        desktop.PushModal(dialog);

        var popped = desktop.PopModal();

        Assert.Equal(dialog, popped);
        Assert.False(desktop.HasModal);
        // Verify the dialog no longer participates in hit-testing (removed from children).
        Assert.Null(desktop.FindAt(0, 0));
    }

    [Fact]
    public void PopModal_WhenEmpty_ReturnsNull()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        Assert.Null(desktop.PopModal());
    }

    [Fact]
    public void PushModal_NestedModals_ActiveModalIsTopmost()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var first = new StubView { Width = 20, Height = 10 };
        var second = new StubView { Width = 20, Height = 10 };

        desktop.PushModal(first);
        desktop.PushModal(second);

        Assert.Equal(second, desktop.ActiveModal);
    }

    [Fact]
    public void PopModal_NestedModals_RestoresPreviousModal()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var first = new StubView { Width = 20, Height = 10 };
        var second = new StubView { Width = 20, Height = 10 };

        desktop.PushModal(first);
        desktop.PushModal(second);
        desktop.PopModal();

        Assert.Equal(first, desktop.ActiveModal);
        Assert.True(desktop.HasModal);
    }

    private sealed class StubView : TuiView
    {
        public override void Draw(TuiRenderContext ctx) { }
    }

    // ── Draw() — settles M2 tech debt ─────────────────────────────────────────

    [Fact]
    public void Draw_EmptyDesktop_DoesNotThrow()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var ex = Record.Exception(() => ExecuteDraw(desktop));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_WithChild_DoesNotThrow()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var child = new StubView { Col = 0, Row = 0, Width = 10, Height = 5 };
        desktop.Add(child);

        var ex = Record.Exception(() => ExecuteDraw(desktop));
        Assert.Null(ex);
    }

    [Fact]
    public void Draw_WithActiveModal_DoesNotThrow()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var modal = new StubView { Col = 5, Row = 5, Width = 20, Height = 10 };
        desktop.PushModal(modal);

        Assert.True(desktop.HasModal);
        var ex = Record.Exception(() => ExecuteDraw(desktop));
        Assert.Null(ex);
    }

    // ── Draw() helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Runs <see cref="TuiDesktop.Draw"/> inside a valid
    /// <see cref="TuiRenderContext.RenderFrame"/> call using an in-memory
    /// CPU-rasterized surface. No SDL2 or GPU required.
    /// </summary>
    private static void ExecuteDraw(TuiDesktop desktop)
    {
        using var surface = SKSurface.Create(
            new SKImageInfo(720, 400, SKColorType.Bgra8888, SKAlphaType.Premul));

        using var skFont = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(720, 400, skFont);

        var font = new TuiFont();
        font.Load(SKTypeface.Default, 16f);

        using var ctx = new TuiRenderContext(grid, font);
        ctx.RenderFrame(surface, _ => desktop.Draw(ctx));
    }

    // ── CommandSink ───────────────────────────────────────────────────────────

    [Fact]
    public void CommandSink_DefaultValue_IsNull()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        Assert.Null(desktop.CommandSink);
    }

    [Fact]
    public void HandleEvent_CommandNotConsumedByChildren_InvokesCommandSink()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        TuiCommandEvent? received = null;
        desktop.CommandSink = ev => received = ev;

        var cmd = new TuiCommandEvent(TuiCommand.Close);
        desktop.HandleEvent(cmd);

        Assert.NotNull(received);
        Assert.Equal(TuiCommand.Close, received.Command);
    }

    [Fact]
    public void HandleEvent_CommandConsumedByChild_CommandSinkNotInvoked()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var consumer = new CommandConsumingView { Col = 0, Row = 0, Width = 10, Height = 5 };
        desktop.Add(consumer);

        bool sinkInvoked = false;
        desktop.CommandSink = _ => sinkInvoked = true;

        desktop.HandleEvent(new TuiCommandEvent(TuiCommand.Close));

        Assert.False(sinkInvoked);
    }

    [Fact]
    public void HandleEvent_NoCommandSink_DoesNotThrow()
    {
        var desktop = new TuiDesktop { Width = 80, Height = 25 };
        var ex = Record.Exception(() =>
            desktop.HandleEvent(new TuiCommandEvent(TuiCommand.Close)));
        Assert.Null(ex);
    }

    // ── CommandConsumingView helper ───────────────────────────────────────────

    private sealed class CommandConsumingView : TuiView
    {
        public override void Draw(TuiRenderContext ctx) { }
        public override bool HandleEvent(TuiEvent ev) => ev is TuiCommandEvent;
    }
}
