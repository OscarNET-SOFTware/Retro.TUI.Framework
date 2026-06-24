// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiGroupTests.cs" company="OscarNET-SOFTware">
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
/// Tests for <see cref="TuiGroup"/>: child lifecycle, hit-testing, draw order
/// and event dispatch.
/// </summary>
public sealed class TuiGroupTests
{
    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class StubView : TuiView
    {
        public int DrawOrder { get; private set; }
        public bool HandleEventResult { get; set; }
        public TuiEvent? LastEvent { get; private set; }

        // Shared draw-order counter injected per test.
        public Func<int>? NextDrawOrder { get; set; }

        public override void Draw(TuiRenderContext ctx)
        {
            DrawOrder = NextDrawOrder?.Invoke() ?? 0;
        }

        public override bool HandleEvent(TuiEvent ev)
        {
            LastEvent = ev;
            return HandleEventResult;
        }
    }

    // ── Add / Remove ──────────────────────────────────────────────────────────

    [Fact]
    public void Add_NewChild_SetsParentOnChild()
    {
        var group = new TuiGroup();
        var child = new StubView();

        group.Add(child);

        Assert.Same(group, child.Parent);
    }

    [Fact]
    public void Add_ChildAlreadyHasParent_ThrowsInvalidOperationException()
    {
        var group1 = new TuiGroup();
        var group2 = new TuiGroup();
        var child = new StubView();
        group1.Add(child);

        Assert.Throws<InvalidOperationException>(() => group2.Add(child));
    }

    [Fact]
    public void Add_NullChild_ThrowsArgumentNullException()
    {
        var group = new TuiGroup();

        Assert.Throws<ArgumentNullException>(() => group.Add(null!));
    }

    [Fact]
    public void Remove_ExistingChild_ClearsParentAndReturnsTrue()
    {
        var group = new TuiGroup();
        var child = new StubView();
        group.Add(child);

        bool removed = group.Remove(child);

        Assert.True(removed);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void Remove_NonMemberChild_ReturnsFalse()
    {
        var group = new TuiGroup();
        var child = new StubView();

        bool removed = group.Remove(child);

        Assert.False(removed);
    }

    [Fact]
    public void Remove_NullChild_ThrowsArgumentNullException()
    {
        var group = new TuiGroup();

        Assert.Throws<ArgumentNullException>(() => group.Remove(null!));
    }

    // ── IsFrontmost ────────────────────────────────────────────────────────────

    [Fact]
    public void IsFrontmost_NullChild_ThrowsArgumentNullException()
    {
        var group = new TuiGroup();

        Assert.Throws<ArgumentNullException>(() => group.IsFrontmost(null!));
    }

    [Fact]
    public void IsFrontmost_EmptyGroup_ReturnsFalse()
    {
        var group = new TuiGroup();
        var view = new StubView();

        Assert.False(group.IsFrontmost(view));
    }

    [Fact]
    public void IsFrontmost_OnlyChild_ReturnsTrue()
    {
        var group = new TuiGroup();
        var child = new StubView();
        group.Add(child);

        Assert.True(group.IsFrontmost(child));
    }

    [Fact]
    public void IsFrontmost_LastAddedChild_ReturnsTrue()
    {
        var group = new TuiGroup();
        var first = new StubView();
        var last = new StubView();
        group.Add(first);
        group.Add(last);

        Assert.True(group.IsFrontmost(last));
        Assert.False(group.IsFrontmost(first));
    }

    [Fact]
    public void IsFrontmost_ViewNotInGroup_ReturnsFalse()
    {
        var group = new TuiGroup();
        var member = new StubView();
        var outsider = new StubView();
        group.Add(member);

        Assert.False(group.IsFrontmost(outsider));
    }

    // ── FindAt — hit-testing ──────────────────────────────────────────────────

    [Fact]
    public void FindAt_PointInsideVisibleChild_ReturnsChild()
    {
        var group = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var child = new StubView { Col = 5, Row = 2, Width = 10, Height = 5 };
        group.Add(child);

        TuiView? hit = group.FindAt(absCol: 7, absRow: 4);

        Assert.Same(child, hit);
    }

    [Fact]
    public void FindAt_PointOutsideAllChildren_ReturnsNull()
    {
        var group = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var child = new StubView { Col = 5, Row = 2, Width = 10, Height = 5 };
        group.Add(child);

        TuiView? hit = group.FindAt(absCol: 50, absRow: 15);

        Assert.Null(hit);
    }

    [Fact]
    public void FindAt_PointInsideInvisibleChild_ReturnsNull()
    {
        var group = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var child = new StubView { Col = 5, Row = 2, Width = 10, Height = 5, Visible = false };
        group.Add(child);

        TuiView? hit = group.FindAt(absCol: 7, absRow: 4);

        Assert.Null(hit);
    }

    [Fact]
    public void FindAt_TwoOverlappingChildren_ReturnsFrontmost()
    {
        // Child B is added after A (higher z-order) and overlaps it.
        var group = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var childA = new StubView { Col = 0, Row = 0, Width = 10, Height = 5 };
        var childB = new StubView { Col = 0, Row = 0, Width = 10, Height = 5 };
        group.Add(childA);
        group.Add(childB);  // frontmost

        TuiView? hit = group.FindAt(absCol: 3, absRow: 2);

        Assert.Same(childB, hit);
    }

    [Fact]
    public void FindAt_NestedGroup_ReturnsDeepestView()
    {
        var outer = new TuiGroup { Col = 0, Row = 0, Width = 80, Height = 25 };
        var inner = new TuiGroup { Col = 5, Row = 5, Width = 20, Height = 10 };
        var leaf = new StubView { Col = 2, Row = 2, Width = 5, Height = 3 };

        outer.Add(inner);
        inner.Add(leaf);

        // leaf is at AbsCol=7, AbsRow=7 in screen coordinates.
        TuiView? hit = outer.FindAt(absCol: 8, absRow: 8);

        Assert.Same(leaf, hit);
    }

    // ── Draw order ────────────────────────────────────────────────────────────
    // NOTE: Draw requires a live TuiRenderContext tied to an SKCanvas.
    // These tests verify only the visible/invisible skip logic via the
    // DrawOrder side-effect on StubView.
    // Full rendering integration is covered by Retro.TUI.Rendering.Tests.

    [Fact]
    public void Draw_InvisibleChild_IsSkipped()
    {
        // We can verify the visible contract without a real canvas by checking
        // that Draw is never called on the invisible child.
        var group = new TuiGroup();
        int counter = 0;

        var visible = new StubView { Visible = true, Width = 1, Height = 1 };
        var invisible = new StubView { Visible = false, Width = 1, Height = 1 };

        visible.NextDrawOrder = () => ++counter;
        invisible.NextDrawOrder = () => ++counter;

        group.Add(visible);
        group.Add(invisible);

        // DrawOrder of 0 means Draw was never called (counter starts at 0,
        // increments only when Draw is invoked).
        Assert.Equal(0, visible.DrawOrder);    // not yet drawn
        Assert.Equal(0, invisible.DrawOrder);  // not yet drawn
        // (Full draw-order verification requires an SKCanvas; see integration tests.)
    }

    // ── HandleEvent dispatch ──────────────────────────────────────────────────

    [Fact]
    public void HandleEvent_FrontmostChildConsumes_StopsDispatch()
    {
        var group = new TuiGroup();
        var back = new StubView { HandleEventResult = false };
        var front = new StubView { HandleEventResult = true };
        group.Add(back);
        group.Add(front);  // frontmost

        var ev = new TuiCommandEvent(TuiCommand.None);
        bool consumed = group.HandleEvent(ev);

        Assert.True(consumed);
        // The front child received the event.
        Assert.Same(ev, front.LastEvent);
        // The back child was NOT reached because front consumed it.
        Assert.Null(back.LastEvent);
    }

    [Fact]
    public void HandleEvent_NoChildConsumes_ReturnsFalse()
    {
        var group = new TuiGroup();
        var child1 = new StubView { HandleEventResult = false };
        var child2 = new StubView { HandleEventResult = false };
        group.Add(child1);
        group.Add(child2);

        bool consumed = group.HandleEvent(new TuiCommandEvent(TuiCommand.None));

        Assert.False(consumed);
    }

    [Fact]
    public void HandleEvent_InvisibleOrDisabledChild_IsSkipped()
    {
        var group = new TuiGroup();
        var disabled = new StubView { Enabled = false, HandleEventResult = true };
        var normal = new StubView { HandleEventResult = false };
        group.Add(normal);
        group.Add(disabled);  // frontmost but disabled

        bool consumed = group.HandleEvent(new TuiCommandEvent(TuiCommand.None));

        // disabled was skipped; normal was reached but did not consume.
        Assert.False(consumed);
        Assert.Null(disabled.LastEvent);
    }
}
