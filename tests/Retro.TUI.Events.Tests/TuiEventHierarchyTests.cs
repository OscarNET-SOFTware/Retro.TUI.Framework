// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEventHierarchyTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Events;

/// <summary>
/// Tests for the <see cref="TuiEvent"/> record hierarchy.
/// Verifies immutability, structural equality and pattern matching behaviour.
/// </summary>
public sealed class TuiEventHierarchyTests
{
    // ── TuiKeyEvent ───────────────────────────────────────────────────────

    [Fact]
    public void TuiKeyEvent_StructuralEquality_EqualWhenSameValues()
    {
        var a = new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None);
        var b = new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None);

        Assert.Equal(a, b);
    }

    [Fact]
    public void TuiKeyEvent_StructuralEquality_NotEqualWhenDifferentKey()
    {
        var a = new TuiKeyEvent(TuiKey.Enter, '\0', TuiModifiers.None);
        var b = new TuiKeyEvent(TuiKey.Escape, '\0', TuiModifiers.None);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TuiModifiers_CombinedFlags_CanRepresentMultipleModifiers()
    {
        TuiModifiers combo = TuiModifiers.Control | TuiModifiers.Shift;

        Assert.True(combo.HasFlag(TuiModifiers.Control));
        Assert.True(combo.HasFlag(TuiModifiers.Shift));
        Assert.False(combo.HasFlag(TuiModifiers.Alt));
    }

    // ── TuiMouseEvent ─────────────────────────────────────────────────────

    [Fact]
    public void TuiMouseEvent_StructuralEquality_EqualWhenSameValues()
    {
        var a = new TuiMouseEvent(TuiMouseAction.Click, 10, 5, TuiMouseButton.Left);
        var b = new TuiMouseEvent(TuiMouseAction.Click, 10, 5, TuiMouseButton.Left);

        Assert.Equal(a, b);
    }

    [Fact]
    public void TuiMouseEvent_StructuralEquality_NotEqualWhenDifferentPosition()
    {
        var a = new TuiMouseEvent(TuiMouseAction.Move, 0, 0, TuiMouseButton.None);
        var b = new TuiMouseEvent(TuiMouseAction.Move, 1, 0, TuiMouseButton.None);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TuiMouseEvent_PixelCoordinates_DefaultToZero()
    {
        var ev = new TuiMouseEvent(TuiMouseAction.Move, 0, 0, TuiMouseButton.None);

        Assert.Equal(0f, ev.PixelX);
        Assert.Equal(0f, ev.PixelY);
    }

    [Fact]
    public void TuiMouseEvent_PixelCoordinates_CanBeSetIndependentlyOfCellCoordinates()
    {
        // Col/Row are placeholders (0) until TuiMessageLoop recomputes them;
        // PixelX/PixelY carry the real screen-pixel position from the host.
        var ev = new TuiMouseEvent(
            TuiMouseAction.Move,
            Col: 0,
            Row: 0,
            TuiMouseButton.None,
            PixelX: 123.5f,
            PixelY: 47.0f);

        Assert.Equal(0, ev.Col);
        Assert.Equal(0, ev.Row);
        Assert.Equal(123.5f, ev.PixelX);
        Assert.Equal(47.0f, ev.PixelY);
    }

    [Fact]
    public void TuiMouseEvent_StructuralEquality_NotEqualWhenDifferentPixelPosition()
    {
        var a = new TuiMouseEvent(TuiMouseAction.Move, 0, 0, TuiMouseButton.None, PixelX: 10f, PixelY: 10f);
        var b = new TuiMouseEvent(TuiMouseAction.Move, 0, 0, TuiMouseButton.None, PixelX: 20f, PixelY: 10f);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TuiMouseEvent_WithExpression_CanRecomputeCellCoordinates()
    {
        // Simulates what TuiMessageLoop does: take the raw event and produce
        // a new instance with Col/Row recomputed from PixelX/PixelY.
        var raw = new TuiMouseEvent(TuiMouseAction.Move, 0, 0, TuiMouseButton.None, PixelX: 95f, PixelY: 33f);

        var recomputed = raw with { Col = 10, Row = 2 };

        Assert.Equal(10, recomputed.Col);
        Assert.Equal(2, recomputed.Row);
        Assert.Equal(95f, recomputed.PixelX);
        Assert.Equal(33f, recomputed.PixelY);
    }

    // ── TuiCommandEvent ───────────────────────────────────────────────────

    [Fact]
    public void TuiCommandEvent_StructuralEquality_EqualWhenSameCommandNoParameter()
    {
        var a = new TuiCommandEvent(TuiCommand.Ok);
        var b = new TuiCommandEvent(TuiCommand.Ok);

        Assert.Equal(a, b);
    }

    [Fact]
    public void TuiCommandEvent_StructuralEquality_EqualWhenSameCommandAndParameter()
    {
        var a = new TuiCommandEvent(TuiCommand.Ok, "payload");
        var b = new TuiCommandEvent(TuiCommand.Ok, "payload");

        Assert.Equal(a, b);
    }

    [Fact]
    public void TuiCommandEvent_StructuralEquality_NotEqualWhenDifferentParameter()
    {
        var a = new TuiCommandEvent(TuiCommand.Ok, "x");
        var b = new TuiCommandEvent(TuiCommand.Ok, "y");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TuiCommandEvent_Parameter_DefaultsToNull()
    {
        var evt = new TuiCommandEvent(TuiCommand.Cancel);

        Assert.Null(evt.Parameter);
    }

    // ── TuiTimerEvent ─────────────────────────────────────────────────────

    [Fact]
    public void TuiTimerEvent_StructuralEquality_EqualWhenSameElapsed()
    {
        var elapsed = TimeSpan.FromSeconds(1.5);
        var a = new TuiTimerEvent(elapsed);
        var b = new TuiTimerEvent(elapsed);

        Assert.Equal(a, b);
    }

    // ── TuiFocusEvent ─────────────────────────────────────────────────────

    [Fact]
    public void TuiFocusEvent_StructuralEquality_EqualWhenSameAction()
    {
        var a = new TuiFocusEvent(TuiFocusAction.Gained);
        var b = new TuiFocusEvent(TuiFocusAction.Gained);

        Assert.Equal(a, b);
    }

    [Fact]
    public void TuiFocusEvent_StructuralEquality_NotEqualWhenDifferentAction()
    {
        var a = new TuiFocusEvent(TuiFocusAction.Gained);
        var b = new TuiFocusEvent(TuiFocusAction.Lost);

        Assert.NotEqual(a, b);
    }

    // ── Pattern matching ──────────────────────────────────────────────────

    [Fact]
    public void PatternMatching_SwitchExpression_CorrectlyDispatchesEachEventType()
    {
        // Arrange — one instance of each concrete event type, boxed as TuiEvent
        TuiEvent[] events =
        [
            new TuiKeyEvent(TuiKey.F1, '\0', TuiModifiers.None),
            new TuiMouseEvent(TuiMouseAction.Click, 0, 0, TuiMouseButton.Left),
            new TuiCommandEvent(TuiCommand.Quit),
            new TuiTimerEvent(TimeSpan.Zero),
            new TuiFocusEvent(TuiFocusAction.Gained),
        ];

        // Act — classify each event via pattern matching
        string[] labels = [.. events.Select(e => e switch
        {
            TuiKeyEvent => "key",
            TuiMouseEvent => "mouse",
            TuiCommandEvent => "command",
            TuiTimerEvent => "timer",
            TuiFocusEvent => "focus",
            _ => "unknown",
        })];

        // Assert
        Assert.Equal(["key", "mouse", "command", "timer", "focus"], labels);
    }

    [Fact]
    public void PatternMatching_Deconstruction_ExtractsPropertiesCorrectly()
    {
        TuiEvent evt = new TuiKeyEvent(TuiKey.Escape, '\0', TuiModifiers.Alt);

        if (evt is TuiKeyEvent(TuiKey.Escape, _, TuiModifiers modifiers))
        {
            Assert.True(modifiers.HasFlag(TuiModifiers.Alt));
        }
        else
        {
            Assert.Fail("Pattern match should have succeeded for TuiKeyEvent(Escape).");
        }
    }

    // ── Immutability ──────────────────────────────────────────────────────

    [Fact]
    public void TuiKeyEvent_WithExpression_ProducesNewInstanceWithUpdatedValue()
    {
        var original = new TuiKeyEvent(TuiKey.F1, '\0', TuiModifiers.None);
        var modified = original with { Modifiers = TuiModifiers.Shift };

        // Original is unchanged
        Assert.Equal(TuiModifiers.None, original.Modifiers);

        // Modified has the new value
        Assert.Equal(TuiModifiers.Shift, modified.Modifiers);

        // They are distinct instances
        Assert.NotEqual(original, modified);
    }

    // ── TuiCommand reserved range ─────────────────────────────────────────

    [Fact]
    public void TuiCommand_UserDefined_ValueIsAtLeast100()
    {
        Assert.True((int)TuiCommand.UserDefined >= 100);
    }

    [Fact]
    public void TuiCommand_FrameworkReservedCommands_AllBelowUserDefined()
    {
        TuiCommand[] reserved =
        [
            TuiCommand.None, TuiCommand.Quit, TuiCommand.Close, TuiCommand.Help,
            TuiCommand.Ok,   TuiCommand.Cancel, TuiCommand.Yes,  TuiCommand.No,
            TuiCommand.ZoomWindow, TuiCommand.ResizeWindow, TuiCommand.MoveWindow,
            TuiCommand.NextWindow, TuiCommand.PrevWindow,
        ];

        foreach (TuiCommand cmd in reserved)
        {
            Assert.True((int)cmd < (int)TuiCommand.UserDefined,
                $"{cmd} must be below UserDefined ({(int)TuiCommand.UserDefined}).");
        }
    }
}
