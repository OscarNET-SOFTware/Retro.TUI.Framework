// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiMessageLoopFocusEventTests.cs" company="OscarNET-SOFTware">
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
/// Tests for the <see cref="TuiFocusEvent"/> dispatch wired via
/// <see cref="TuiMessageLoop.EnsureFocusEventWiring"/>, which translates
/// <see cref="TuiFocusManager.FocusChanged"/> notifications into Lost/Gained
/// deliveries on the affected views.
/// </summary>
/// <remarks>
/// Calls <see cref="TuiMessageLoop.EnsureFocusEventWiring"/> directly — it is
/// <c>internal</c>, visible here via <c>InternalsVisibleTo</c> — rather than
/// indirectly through <see cref="TuiMessageLoop.DispatchEvents"/>, to avoid
/// coupling these tests to the unrelated detail of where in the dispatch body
/// the wiring call happens to live.
/// </remarks>
public sealed class TuiMessageLoopFocusEventTests
{
    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class RecordingView : TuiView
    {
        public RecordingView() => Focusable = true;
        public List<TuiEvent> ReceivedEvents { get; } = [];

        public override void Draw(TuiRenderContext ctx) { }

        public override bool HandleEvent(TuiEvent ev)
        {
            ReceivedEvents.Add(ev);
            return false;
        }
    }

    // ── Lost / Gained ordering ───────────────────────────────────────────────

    [Fact]
    public void FocusChanged_FromViewAToViewB_DispatchesLostThenGained()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        var viewB = new RecordingView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewA);

        var loop = new TuiMessageLoop();

        // Establish wiring while viewA already holds focus, mirroring the
        // real TuiApplication flow where FocusManager.SetFocus(initialView)
        // happens in OnInitialize, before the loop starts.
        loop.EnsureFocusEventWiring(fm);

        fm.SetFocus(viewB);

        Assert.Single(viewA.ReceivedEvents);
        Assert.Equal(TuiFocusAction.Lost, Assert.IsType<TuiFocusEvent>(viewA.ReceivedEvents[0]).Action);

        Assert.Single(viewB.ReceivedEvents);
        Assert.Equal(TuiFocusAction.Gained, Assert.IsType<TuiFocusEvent>(viewB.ReceivedEvents[0]).Action);
    }

    [Fact]
    public void FocusChanged_Clear_DispatchesOnlyLost()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        fm.Register(viewA);
        fm.SetFocus(viewA);

        var loop = new TuiMessageLoop();
        loop.EnsureFocusEventWiring(fm);

        fm.Clear();

        TuiFocusEvent received = Assert.IsType<TuiFocusEvent>(Assert.Single(viewA.ReceivedEvents));
        Assert.Equal(TuiFocusAction.Lost, received.Action);
    }

    [Fact]
    public void FocusChanged_FromNoFocusToView_DispatchesOnlyGained()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        fm.Register(viewA);
        // No SetFocus yet — manager starts with Current == null.

        var loop = new TuiMessageLoop();
        loop.EnsureFocusEventWiring(fm);

        fm.SetFocus(viewA);

        TuiFocusEvent received = Assert.IsType<TuiFocusEvent>(Assert.Single(viewA.ReceivedEvents));
        Assert.Equal(TuiFocusAction.Gained, received.Action);
    }

    // ── Wiring idempotency ───────────────────────────────────────────────────

    [Fact]
    public void EnsureFocusEventWiring_CalledRepeatedly_DoesNotDoubleSubscribe()
    {
        var fm = new TuiFocusManager();
        var viewA = new RecordingView();
        var viewB = new RecordingView();
        fm.Register(viewA);
        fm.Register(viewB);
        fm.SetFocus(viewA);

        var loop = new TuiMessageLoop();

        // Simulate several loop iterations before the focus change.
        loop.EnsureFocusEventWiring(fm);
        loop.EnsureFocusEventWiring(fm);
        loop.EnsureFocusEventWiring(fm);

        fm.SetFocus(viewB);

        // If EnsureFocusEventWiring subscribed three times instead of once,
        // viewA would receive three Lost events here instead of one.
        Assert.Single(viewA.ReceivedEvents);
    }
}
