// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SdlKeyMapperTests.cs" company="OscarNET-SOFTware">
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

using Silk.NET.SDL;

namespace Retro.TUI.Hosting;

/// <summary>
/// Unit tests for <see cref="SdlKeyMapper"/>.
/// </summary>
/// <remarks>
/// These tests exercise the static mapping logic only — no SDL2 native library
/// is required at runtime. <see cref="SdlKeyMapper"/> is <c>internal</c>;
/// visibility is granted via <c>InternalsVisibleTo</c> in the production project.
/// </remarks>
public sealed class SdlKeyMapperTests
{
    // ── ToTuiKey — mapped keys ────────────────────────────────────────────

    [Theory]
    [InlineData(KeyCode.KReturn, TuiKey.Enter)]
    [InlineData(KeyCode.KEscape, TuiKey.Escape)]
    [InlineData(KeyCode.KTab, TuiKey.Tab)]
    [InlineData(KeyCode.KBackspace, TuiKey.BackSpace)]
    [InlineData(KeyCode.KDelete, TuiKey.Delete)]
    [InlineData(KeyCode.KInsert, TuiKey.Insert)]
    [InlineData(KeyCode.KHome, TuiKey.Home)]
    [InlineData(KeyCode.KEnd, TuiKey.End)]
    [InlineData(KeyCode.KPageup, TuiKey.PageUp)]
    [InlineData(KeyCode.KPagedown, TuiKey.PageDown)]
    public void ToTuiKey_NavigationKey_ReturnsMappedValue(KeyCode sdlKey, TuiKey expected)
    {
        TuiKey result = SdlKeyMapper.ToTuiKey((int)sdlKey);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(KeyCode.KLeft, TuiKey.Left)]
    [InlineData(KeyCode.KRight, TuiKey.Right)]
    [InlineData(KeyCode.KUp, TuiKey.Up)]
    [InlineData(KeyCode.KDown, TuiKey.Down)]
    public void ToTuiKey_ArrowKey_ReturnsMappedValue(KeyCode sdlKey, TuiKey expected)
    {
        TuiKey result = SdlKeyMapper.ToTuiKey((int)sdlKey);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(KeyCode.KF1, TuiKey.F1)]
    [InlineData(KeyCode.KF2, TuiKey.F2)]
    [InlineData(KeyCode.KF3, TuiKey.F3)]
    [InlineData(KeyCode.KF4, TuiKey.F4)]
    [InlineData(KeyCode.KF5, TuiKey.F5)]
    [InlineData(KeyCode.KF6, TuiKey.F6)]
    [InlineData(KeyCode.KF7, TuiKey.F7)]
    [InlineData(KeyCode.KF8, TuiKey.F8)]
    [InlineData(KeyCode.KF9, TuiKey.F9)]
    [InlineData(KeyCode.KF10, TuiKey.F10)]
    [InlineData(KeyCode.KF11, TuiKey.F11)]
    [InlineData(KeyCode.KF12, TuiKey.F12)]
    public void ToTuiKey_FunctionKey_ReturnsMappedValue(KeyCode sdlKey, TuiKey expected)
    {
        TuiKey result = SdlKeyMapper.ToTuiKey((int)sdlKey);

        Assert.Equal(expected, result);
    }

    // ── ToTuiKey — unmapped keys return None ──────────────────────────────

    [Fact]
    public void ToTuiKey_PrintableCharKey_ReturnsNone()
    {
        // Printable characters (e.g. 'A') are not in the map;
        // they arrive via SDL_TEXTINPUT and carry a KeyChar instead.
        TuiKey result = SdlKeyMapper.ToTuiKey((int)KeyCode.KA);

        Assert.Equal(TuiKey.None, result);
    }

    [Fact]
    public void ToTuiKey_UnknownKey_ReturnsNone()
    {
        TuiKey result = SdlKeyMapper.ToTuiKey((int)KeyCode.KUnknown);

        Assert.Equal(TuiKey.None, result);
    }

    [Fact]
    public void ToTuiKey_ArbitraryUnmappedValue_ReturnsNone()
    {
        // Negative / out-of-range SDL keycodes must never throw.
        TuiKey result = SdlKeyMapper.ToTuiKey(-1);

        Assert.Equal(TuiKey.None, result);
    }

    // ── ToTuiModifiers — no modifiers ─────────────────────────────────────

    [Fact]
    public void ToTuiModifiers_NoModifiers_ReturnsNone()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.None);

        Assert.Equal(TuiModifiers.None, result);
    }

    // ── ToTuiModifiers — individual modifiers ─────────────────────────────

    [Fact]
    public void ToTuiModifiers_LeftShift_ReturnsShift()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Lshift);

        Assert.Equal(TuiModifiers.Shift, result);
    }

    [Fact]
    public void ToTuiModifiers_RightShift_ReturnsShift()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Rshift);

        Assert.Equal(TuiModifiers.Shift, result);
    }

    [Fact]
    public void ToTuiModifiers_LeftCtrl_ReturnsControl()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Lctrl);

        Assert.Equal(TuiModifiers.Control, result);
    }

    [Fact]
    public void ToTuiModifiers_RightCtrl_ReturnsControl()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Rctrl);

        Assert.Equal(TuiModifiers.Control, result);
    }

    [Fact]
    public void ToTuiModifiers_LeftAlt_ReturnsAlt()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Lalt);

        Assert.Equal(TuiModifiers.Alt, result);
    }

    [Fact]
    public void ToTuiModifiers_RightAlt_ReturnsAlt()
    {
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Ralt);

        Assert.Equal(TuiModifiers.Alt, result);
    }

    // ── ToTuiModifiers — combined modifiers ───────────────────────────────

    [Fact]
    public void ToTuiModifiers_CtrlShift_ReturnsBothFlags()
    {
        ushort sdlMod = (ushort)(Keymod.Lctrl | Keymod.Lshift);

        TuiModifiers result = SdlKeyMapper.ToTuiModifiers(sdlMod);

        Assert.True(result.HasFlag(TuiModifiers.Control));
        Assert.True(result.HasFlag(TuiModifiers.Shift));
        Assert.False(result.HasFlag(TuiModifiers.Alt));
    }

    [Fact]
    public void ToTuiModifiers_CtrlAltShift_ReturnsAllThreeFlags()
    {
        ushort sdlMod = (ushort)(Keymod.Lctrl | Keymod.Lalt | Keymod.Lshift);

        TuiModifiers result = SdlKeyMapper.ToTuiModifiers(sdlMod);

        Assert.True(result.HasFlag(TuiModifiers.Control));
        Assert.True(result.HasFlag(TuiModifiers.Alt));
        Assert.True(result.HasFlag(TuiModifiers.Shift));
    }

    [Fact]
    public void ToTuiModifiers_LeftAndRightShift_ReturnsSingleShiftFlag()
    {
        // Both sides held simultaneously must collapse to a single Shift flag.
        ushort sdlMod = (ushort)(Keymod.Lshift | Keymod.Rshift);

        TuiModifiers result = SdlKeyMapper.ToTuiModifiers(sdlMod);

        Assert.Equal(TuiModifiers.Shift, result);
    }

    // ── ToTuiModifiers — unrelated modifier bits are ignored ──────────────

    [Fact]
    public void ToTuiModifiers_NumLockOnly_ReturnsNone()
    {
        // SDL Num Lock bit must not map to any TuiModifiers value.
        TuiModifiers result = SdlKeyMapper.ToTuiModifiers((ushort)Keymod.Num);

        Assert.Equal(TuiModifiers.None, result);
    }
}
