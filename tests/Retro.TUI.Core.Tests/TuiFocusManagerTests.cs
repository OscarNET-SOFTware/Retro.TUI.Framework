// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiFocusManagerTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Core;

/// <summary>
/// Tests for <see cref="TuiFocusManager"/>: registration, tab-order cycling,
/// SetFocus, Clear and the <see cref="TuiFocusManager.FocusChanged"/> event.
/// </summary>
public sealed class TuiFocusManagerTests
{
    // ── Test double ───────────────────────────────────────────────────────────

    private sealed class FocusableView : TuiView
    {
        public FocusableView() => Focusable = true;
        public override void Draw(TuiRenderContext ctx) { }
        public override bool HandleEvent(TuiEvent ev) => false;
    }

    private sealed class NonFocusableView : TuiView
    {
        // Focusable defaults to false.
        public override void Draw(TuiRenderContext ctx) { }
        public override bool HandleEvent(TuiEvent ev) => false;
    }

    // ── Default state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewManager_CurrentIsNull()
    {
        var fm = new TuiFocusManager();

        Assert.Null(fm.Current);
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public void Register_FocusableView_DoesNotThrow()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();

        var ex = Record.Exception(() => fm.Register(view));

        Assert.Null(ex);
    }

    [Fact]
    public void Register_NonFocusableView_ThrowsArgumentException()
    {
        var fm = new TuiFocusManager();
        var view = new NonFocusableView();

        Assert.Throws<ArgumentException>(() => fm.Register(view));
    }

    [Fact]
    public void Register_NullView_ThrowsArgumentNullException()
    {
        var fm = new TuiFocusManager();

        Assert.Throws<ArgumentNullException>(() => fm.Register(null!));
    }

    [Fact]
    public void Register_AlreadyRegistered_ThrowsArgumentException()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);

        Assert.Throws<ArgumentException>(() => fm.Register(view));
    }

    // ── Unregister ────────────────────────────────────────────────────────────

    [Fact]
    public void Unregister_FocusedView_ClearsFocus()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);
        fm.SetFocus(view);

        fm.Unregister(view);

        Assert.Null(fm.Current);
    }

    [Fact]
    public void Unregister_NonFocusedView_DoesNotChangeFocus()
    {
        var fm = new TuiFocusManager();
        var viewA = new FocusableView();
        var viewB = new FocusableView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewA);

        fm.Unregister(viewB);

        Assert.Same(viewA, fm.Current);
    }

    // ── SetFocus ──────────────────────────────────────────────────────────────

    [Fact]
    public void SetFocus_RegisteredView_SetsCurrent()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);

        fm.SetFocus(view);

        Assert.Same(view, fm.Current);
    }

    [Fact]
    public void SetFocus_UnregisteredView_ThrowsArgumentException()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();  // not registered

        Assert.Throws<ArgumentException>(() => fm.SetFocus(view));
    }

    [Fact]
    public void SetFocus_NullView_ThrowsArgumentNullException()
    {
        var fm = new TuiFocusManager();

        Assert.Throws<ArgumentNullException>(() => fm.SetFocus(null!));
    }

    // ── IsFocused ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsFocused_CurrentView_ReturnsTrue()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);
        fm.SetFocus(view);

        Assert.True(fm.IsFocused(view));
    }

    [Fact]
    public void IsFocused_NonFocusedView_ReturnsFalse()
    {
        var fm = new TuiFocusManager();
        var viewA = new FocusableView();
        var viewB = new FocusableView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewA);

        Assert.False(fm.IsFocused(viewB));
    }

    // ── FocusNext ─────────────────────────────────────────────────────────────

    [Fact]
    public void FocusNext_NoViewRegistered_IsNoOp()
    {
        var fm = new TuiFocusManager();

        var ex = Record.Exception(() => fm.FocusNext());

        Assert.Null(ex);
        Assert.Null(fm.Current);
    }

    [Fact]
    public void FocusNext_NoneCurrentlyFocused_FocusesFirst()
    {
        var fm = new TuiFocusManager();
        var first = new FocusableView();
        fm.Register(first);
        fm.Register(new FocusableView());

        fm.FocusNext();

        Assert.Same(first, fm.Current);
    }

    [Fact]
    public void FocusNext_FromLast_WrapsToFirst()
    {
        var fm = new TuiFocusManager();
        var first = new FocusableView();
        var last = new FocusableView();
        fm.Register(first);
        fm.Register(last);
        fm.SetFocus(last);

        fm.FocusNext();

        Assert.Same(first, fm.Current);
    }

    [Fact]
    public void FocusNext_CyclesInRegistrationOrder()
    {
        var fm = new TuiFocusManager();
        var a = new FocusableView();
        var b = new FocusableView();
        var c = new FocusableView();
        fm.Register(a);
        fm.Register(b);
        fm.Register(c);
        fm.SetFocus(a);

        fm.FocusNext();
        Assert.Same(b, fm.Current);

        fm.FocusNext();
        Assert.Same(c, fm.Current);

        fm.FocusNext();
        Assert.Same(a, fm.Current);  // wrap-around
    }

    // ── FocusPrevious ─────────────────────────────────────────────────────────

    [Fact]
    public void FocusPrevious_NoneCurrentlyFocused_FocusesLast()
    {
        var fm = new TuiFocusManager();
        var last = new FocusableView();
        fm.Register(new FocusableView());
        fm.Register(last);

        fm.FocusPrevious();

        Assert.Same(last, fm.Current);
    }

    [Fact]
    public void FocusPrevious_FromFirst_WrapsToLast()
    {
        var fm = new TuiFocusManager();
        var first = new FocusableView();
        var last = new FocusableView();
        fm.Register(first);
        fm.Register(last);
        fm.SetFocus(first);

        fm.FocusPrevious();

        Assert.Same(last, fm.Current);
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clear_WithFocusedView_SetsCurrentToNull()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);
        fm.SetFocus(view);

        fm.Clear();

        Assert.Null(fm.Current);
    }

    [Fact]
    public void Clear_WhenAlreadyEmpty_IsNoOp()
    {
        var fm = new TuiFocusManager();

        var ex = Record.Exception(() => fm.Clear());

        Assert.Null(ex);
        Assert.Null(fm.Current);
    }

    // ── FocusChanged event ────────────────────────────────────────────────────

    [Fact]
    public void FocusChanged_RaisedWhenFocusMoves()
    {
        var fm = new TuiFocusManager();
        var viewA = new FocusableView();
        var viewB = new FocusableView();
        fm.Register(viewA);
        fm.Register(viewB);

        TuiView? receivedView = null;
        fm.FocusChanged += (_, args) => receivedView = args.FocusedView;

        fm.SetFocus(viewA);

        Assert.Same(viewA, receivedView);
    }

    [Fact]
    public void FocusChanged_NotRaisedWhenFocusDoesNotChange()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);
        fm.SetFocus(view);

        int callCount = 0;
        fm.FocusChanged += (_, _) => callCount++;

        fm.SetFocus(view);  // same view — no change

        Assert.Equal(0, callCount);
    }

    [Fact]
    public void FocusChanged_RaisedWithNullWhenCleared()
    {
        var fm = new TuiFocusManager();
        var view = new FocusableView();
        fm.Register(view);
        fm.SetFocus(view);

        TuiView? receivedView = view;  // pre-set to non-null
        fm.FocusChanged += (_, args) => receivedView = args.FocusedView;

        fm.Clear();

        Assert.Null(receivedView);
    }
}
