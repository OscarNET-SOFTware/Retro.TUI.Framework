// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PcTools9ThemeTests.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

using Retro.TUI.Theme.PcTools9;

namespace Retro.TUI.Theming;

/// <summary>
/// Tests for <see cref="PcTools9Theme"/>.
/// Verifies palette completeness, key color values confirmed by Paint.NET measurement,
/// theme metadata and singleton identity.
/// </summary>
public sealed class PcTools9ThemeTests
{
    // ── Singleton ─────────────────────────────────────────────────────────

    [Fact]
    public void Instance_CalledTwice_ReturnsSameReference()
    {
        Assert.Same(PcTools9Theme.Instance, PcTools9Theme.Instance);
    }

    [Fact]
    public void Instance_Name_IsPcTools9x()
    {
        Assert.Equal("PC Tools 9.x", PcTools9Theme.Instance.Name);
    }

    [Fact]
    public void Instance_Palette_IsNotNull()
    {
        Assert.NotNull(PcTools9Theme.Instance.Palette);
    }

    // ── Palette completeness ──────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(AllColorRoles))]
    public void Palette_AllRoles_HaveEntry(TuiColorRole role)
    {
        // Every defined TuiColorRole must resolve without throwing.
        var color = PcTools9Theme.Instance.Palette[role];

        // SKColor is a struct; any value (including Black) is valid.
        Assert.True(color.Alpha >= 0);
    }

    public static IEnumerable<object[]> AllColorRoles() =>
        Enum.GetValues<TuiColorRole>().Select(r => new object[] { r });

    // ── Key color values (Paint.NET confirmed) ────────────────────────────

    [Theory]
    [InlineData(TuiColorRole.DesktopBackground, 0x69, 0x69, 0x69)] // PcToolsGray
    [InlineData(TuiColorRole.DesktopPatternDot, 0x69, 0x69, 0x69)] // same as desktop
    [InlineData(TuiColorRole.AppTitleBackground, 0x55, 0x55, 0xFF)] // EgaBrightBlue
    [InlineData(TuiColorRole.AppTitleForeground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.AppTitleClockForeground, 0xFF, 0xFF, 0x55)] // EgaBrightYellow
    [InlineData(TuiColorRole.WindowBackground, 0x55, 0x55, 0xFF)] // EgaBrightBlue
    [InlineData(TuiColorRole.WindowTitleBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.WindowTitleForeground, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.WindowTitleInactiveBackground, 0xCA, 0xCA, 0xCA)] // PcToolsLightGray
    [InlineData(TuiColorRole.WindowTitleInactiveForeground, 0x69, 0x69, 0x69)] // PcToolsGray
    [InlineData(TuiColorRole.WindowShadow, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.DialogBackground, 0x55, 0x55, 0xFF)] // EgaBrightBlue
    [InlineData(TuiColorRole.DialogTitleBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.DialogTitleWarningBackground, 0xFF, 0x55, 0x55)] // EgaBrightRed
    [InlineData(TuiColorRole.DialogHighlightForeground, 0xFF, 0xFF, 0x55)] // EgaBrightYellow
    [InlineData(TuiColorRole.MenuBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.MenuForeground, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.MenuHotkeyForeground, 0xAA, 0x00, 0x00)] // EgaDarkRed
    [InlineData(TuiColorRole.MenuPopupBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.MenuPopupHotkeyForeground, 0xAA, 0x00, 0x00)] // EgaDarkRed
    [InlineData(TuiColorRole.StatusBackground, 0x69, 0x69, 0x69)] // PcToolsGray
    [InlineData(TuiColorRole.StatusKeyForeground, 0xFF, 0xFF, 0x55)] // EgaBrightYellow
    [InlineData(TuiColorRole.InputBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.InputForeground, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.ButtonBackground, 0xFF, 0xFF, 0xFF)] // White (not #AAAAAA)
    [InlineData(TuiColorRole.ButtonAcceleratorForeground, 0xAA, 0x00, 0x00)] // EgaDarkRed
    [InlineData(TuiColorRole.ButtonShadow, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.CheckAcceleratorForeground, 0xFF, 0xFF, 0x55)] // EgaBrightYellow
    [InlineData(TuiColorRole.ListBackground, 0x55, 0x55, 0xFF)] // EgaBrightBlue
    [InlineData(TuiColorRole.ListSelectedBackground, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.ListHeaderBackground, 0x00, 0x00, 0x00)] // Black
    [InlineData(TuiColorRole.ScrollBarBackground, 0xCA, 0xCA, 0xCA)] // PcToolsLightGray
    [InlineData(TuiColorRole.ScrollBarActiveBackground, 0xFF, 0xFF, 0xFF)] // White
    [InlineData(TuiColorRole.ScrollBarThumb, 0x69, 0x69, 0x69)] // PcToolsGray
    [InlineData(TuiColorRole.ScrollBarArrow, 0x00, 0x00, 0x00)] // Black
    public void Palette_KeyRole_HasExpectedColor(TuiColorRole role, byte r, byte g, byte b)
    {
        SKColor expected = new(r, g, b);
        SKColor actual = PcTools9Theme.Instance.Palette[role];

        Assert.Equal(expected, actual);
    }

    // ── Raw color constants ───────────────────────────────────────────────

    [Fact]
    public void PcToolsGray_HasExpectedRgb()
    {
        Assert.Equal(new SKColor(0x69, 0x69, 0x69), PcTools9Theme.PcToolsGray);
    }

    [Fact]
    public void PcToolsLightGray_HasExpectedRgb()
    {
        Assert.Equal(new SKColor(0xCA, 0xCA, 0xCA), PcTools9Theme.PcToolsLightGray);
    }

    [Fact]
    public void EgaBrightBlue_HasExpectedRgb()
    {
        Assert.Equal(new SKColor(0x55, 0x55, 0xFF), PcTools9Theme.EgaBrightBlue);
    }

    [Fact]
    public void EgaDarkRed_HasExpectedRgb()
    {
        Assert.Equal(new SKColor(0xAA, 0x00, 0x00), PcTools9Theme.EgaDarkRed);
    }

    // ── Theme settings ────────────────────────────────────────────────────

    [Fact]
    public void Instance_DesktopPattern_IsNone()
    {
        Assert.Equal(TuiDesktopPattern.None, PcTools9Theme.Instance.DesktopPattern);
    }

    [Fact]
    public void Instance_FontFamily_IsIbmVga9x16()
    {
        Assert.Equal("PxPlus IBM VGA 9x16", PcTools9Theme.Instance.FontFamily);
    }

    [Fact]
    public void Instance_FontSize_Is16()
    {
        Assert.Equal(16f, PcTools9Theme.Instance.FontSize);
    }

    [Fact]
    public void Instance_ShadowOpacity_Is1()
    {
        Assert.Equal(1.0f, PcTools9Theme.Instance.ShadowOpacity);
    }
}
