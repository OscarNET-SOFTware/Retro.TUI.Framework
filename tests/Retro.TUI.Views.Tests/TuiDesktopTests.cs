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

using Retro.TUI.Rendering;

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
}
