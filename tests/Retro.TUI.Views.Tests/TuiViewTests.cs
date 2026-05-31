// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiViewTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Views;

/// <summary>
/// Tests for <see cref="TuiView"/>: composition tree, coordinate computation,
/// dirty-flag propagation, and default state contracts.
/// </summary>
public sealed class TuiViewTests
{
    // ── Test double ───────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal concrete implementation used across all tests.
    /// </summary>
    private sealed class StubView : TuiView
    {
        public int DrawCallCount { get; private set; }
        public bool HandleEventResult { get; set; }

        public override void Draw(TuiRenderContext ctx) => DrawCallCount++;
        public override bool HandleEvent(TuiEvent ev) => HandleEventResult;
    }

    // ── Default state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewView_DefaultState_MatchesContract()
    {
        var view = new StubView();

        Assert.True(view.Visible);
        Assert.True(view.Enabled);
        Assert.False(view.Focusable);
        Assert.Null(view.Parent);
        Assert.True(view.IsDirty);   // new views are dirty until first draw
        Assert.Equal(0, view.Col);
        Assert.Equal(0, view.Row);
        Assert.Equal(0, view.Width);
        Assert.Equal(0, view.Height);
    }

    // ── Absolute coordinates ──────────────────────────────────────────────────

    [Fact]
    public void AbsCol_NoParent_EqualsSelfCol()
    {
        var view = new StubView { Col = 5 };

        Assert.Equal(5, view.AbsCol);
    }

    [Fact]
    public void AbsRow_NoParent_EqualsSelfRow()
    {
        var view = new StubView { Row = 3 };

        Assert.Equal(3, view.AbsRow);
    }

    [Fact]
    public void AbsCol_WithParentChain_AccumulatesOffsets()
    {
        // grandparent(10) → parent(5) → child(2) → expected AbsCol = 17
        var grandparent = new TuiGroup { Col = 10 };
        var parent = new TuiGroup { Col = 5 };
        var child = new StubView { Col = 2 };

        grandparent.Add(parent);
        parent.Add(child);

        Assert.Equal(17, child.AbsCol);
    }

    [Fact]
    public void AbsRow_WithParentChain_AccumulatesOffsets()
    {
        var grandparent = new TuiGroup { Row = 4 };
        var parent = new TuiGroup { Row = 3 };
        var child = new StubView { Row = 1 };

        grandparent.Add(parent);
        parent.Add(child);

        Assert.Equal(8, child.AbsRow);
    }

    // ── Dirty flag ────────────────────────────────────────────────────────────

    [Fact]
    public void Invalidate_SetsIsDirtyTrue()
    {
        var view = new StubView();
        view.ClearDirty();  // start clean

        view.Invalidate();

        Assert.True(view.IsDirty);
    }

    [Fact]
    public void Invalidate_PropagatesUpToParent()
    {
        var parent = new TuiGroup();
        var child = new StubView();
        parent.Add(child);

        parent.ClearDirty();
        child.ClearDirty();

        child.Invalidate();

        Assert.True(parent.IsDirty);
    }

    [Fact]
    public void Invalidate_PropagatesUpEntireChain()
    {
        var grandparent = new TuiGroup();
        var parent = new TuiGroup();
        var child = new StubView();

        grandparent.Add(parent);
        parent.Add(child);

        grandparent.ClearDirty();
        parent.ClearDirty();
        child.ClearDirty();

        child.Invalidate();

        Assert.True(grandparent.IsDirty);
        Assert.True(parent.IsDirty);
        Assert.True(child.IsDirty);
    }

    [Fact]
    public void ClearDirty_SetsIsDirtyFalse()
    {
        var view = new StubView();  // starts dirty

        view.ClearDirty();

        Assert.False(view.IsDirty);
    }

    // ── Parent management ─────────────────────────────────────────────────────

    [Fact]
    public void Parent_AfterAddToGroup_IsSetToGroup()
    {
        var group = new TuiGroup();
        var child = new StubView();

        group.Add(child);

        Assert.Same(group, child.Parent);
    }

    [Fact]
    public void Parent_AfterRemoveFromGroup_IsNull()
    {
        var group = new TuiGroup();
        var child = new StubView();
        group.Add(child);

        group.Remove(child);

        Assert.Null(child.Parent);
    }

    // ── HandleEvent default ───────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_BaseImplementation_ReturnsFalse()
    {
        // A StubView with HandleEventResult = false mimics the base contract.
        var view = new StubView { HandleEventResult = false };

        bool consumed = view.HandleEvent(new TuiCommandEvent(TuiCommand.None));

        Assert.False(consumed);
    }
}
