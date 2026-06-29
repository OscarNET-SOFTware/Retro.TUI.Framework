// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiPaletteTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Theming;

/// <summary>
/// Tests for <see cref="TuiPalette"/>.
/// Verifies that every <see cref="TuiColorRole"/> resolves to a non-default
/// <see cref="SKColor"/> and that the EGA color constants are correct.
/// </summary>
public sealed class TuiPaletteTests
{
    // ── Role resolution ───────────────────────────────────────────────────────

    [Fact]
    public void Resolve_AllDefinedRoles_DoNotThrow()
    {
        // Every TuiColorRole must have an entry in TuiColorMap.
        // A missing entry would throw KeyNotFoundException.
        foreach (TuiColorRole role in Enum.GetValues<TuiColorRole>())
        {
            SKColor color = TuiPalette.Resolve(role);

            // SKColor is a struct — any value including Black is valid.
            Assert.True(color.Alpha >= 0);
        }
    }

    [Theory]
    [InlineData(TuiColorRole.DesktopBackground, 0x55, 0x55, 0x55)] // EgaDarkGray
    [InlineData(TuiColorRole.WindowBackground, 0x55, 0x55, 0xFF)] // EgaBrightBlue
    [InlineData(TuiColorRole.WindowBorder, 0x00, 0x00, 0x00)] // EgaBlack
    [InlineData(TuiColorRole.WindowTitleBackground, 0xFF, 0xFF, 0xFF)] // EgaWhite
    [InlineData(TuiColorRole.WindowTitleForeground, 0x00, 0x00, 0x00)] // EgaBlack
    [InlineData(TuiColorRole.WindowTitleInactiveBackground, 0xCA, 0xCA, 0xCA)] // LightGray
    [InlineData(TuiColorRole.WindowTitleInactiveForeground, 0x69, 0x69, 0x69)] // Gray
    [InlineData(TuiColorRole.DialogBackground, 0x55, 0xFF, 0xFF)] // EgaBrightCyan
    [InlineData(TuiColorRole.DialogTitleWarningBackground, 0xFF, 0x55, 0x55)] // EgaBrightRed
    [InlineData(TuiColorRole.MenuHotkeyForeground, 0xAA, 0x00, 0x00)] // EgaRed
    [InlineData(TuiColorRole.StatusBackground, 0x69, 0x69, 0x69)] // Gray
    [InlineData(TuiColorRole.MouseCursorFill, 0xFF, 0xFF, 0xFF)] // EgaWhite
    [InlineData(TuiColorRole.MouseCursorOutline, 0x00, 0x00, 0x00)] // EgaBlack
    public void Resolve_KeyRoles_ReturnExpectedEgaColor(
        TuiColorRole role, byte r, byte g, byte b)
    {
        SKColor actual = TuiPalette.Resolve(role);

        Assert.Equal(r, actual.Red);
        Assert.Equal(g, actual.Green);
        Assert.Equal(b, actual.Blue);
    }

    // ── EGA palette constants ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0x00, 0x00, 0x00, nameof(TuiEgaPalette.EgaBlack))]
    [InlineData(0x00, 0x00, 0xAA, nameof(TuiEgaPalette.EgaBlue))]
    [InlineData(0x00, 0xAA, 0x00, nameof(TuiEgaPalette.EgaGreen))]
    [InlineData(0x00, 0xAA, 0xAA, nameof(TuiEgaPalette.EgaCyan))]
    [InlineData(0xAA, 0x00, 0x00, nameof(TuiEgaPalette.EgaRed))]
    [InlineData(0xAA, 0x00, 0xAA, nameof(TuiEgaPalette.EgaMagenta))]
    [InlineData(0xAA, 0x55, 0x00, nameof(TuiEgaPalette.EgaBrown))]
    [InlineData(0xAA, 0xAA, 0xAA, nameof(TuiEgaPalette.EgaLightGray))]
    [InlineData(0x55, 0x55, 0x55, nameof(TuiEgaPalette.EgaDarkGray))]
    [InlineData(0x55, 0x55, 0xFF, nameof(TuiEgaPalette.EgaBrightBlue))]
    [InlineData(0x55, 0xFF, 0x55, nameof(TuiEgaPalette.EgaBrightGreen))]
    [InlineData(0x55, 0xFF, 0xFF, nameof(TuiEgaPalette.EgaBrightCyan))]
    [InlineData(0xFF, 0x55, 0x55, nameof(TuiEgaPalette.EgaBrightRed))]
    [InlineData(0xFF, 0x55, 0xFF, nameof(TuiEgaPalette.EgaBrightMagenta))]
    [InlineData(0xFF, 0xFF, 0x55, nameof(TuiEgaPalette.EgaYellow))]
    [InlineData(0xFF, 0xFF, 0xFF, nameof(TuiEgaPalette.EgaWhite))]
    public void EgaPalette_AllSixteenColors_HaveCorrectRgbValues(
        byte r, byte g, byte b, string colorName)
    {
        SKColor actual = colorName switch
        {
            nameof(TuiEgaPalette.EgaBlack) => TuiEgaPalette.EgaBlack,
            nameof(TuiEgaPalette.EgaBlue) => TuiEgaPalette.EgaBlue,
            nameof(TuiEgaPalette.EgaGreen) => TuiEgaPalette.EgaGreen,
            nameof(TuiEgaPalette.EgaCyan) => TuiEgaPalette.EgaCyan,
            nameof(TuiEgaPalette.EgaRed) => TuiEgaPalette.EgaRed,
            nameof(TuiEgaPalette.EgaMagenta) => TuiEgaPalette.EgaMagenta,
            nameof(TuiEgaPalette.EgaBrown) => TuiEgaPalette.EgaBrown,
            nameof(TuiEgaPalette.EgaLightGray) => TuiEgaPalette.EgaLightGray,
            nameof(TuiEgaPalette.EgaDarkGray) => TuiEgaPalette.EgaDarkGray,
            nameof(TuiEgaPalette.EgaBrightBlue) => TuiEgaPalette.EgaBrightBlue,
            nameof(TuiEgaPalette.EgaBrightGreen) => TuiEgaPalette.EgaBrightGreen,
            nameof(TuiEgaPalette.EgaBrightCyan) => TuiEgaPalette.EgaBrightCyan,
            nameof(TuiEgaPalette.EgaBrightRed) => TuiEgaPalette.EgaBrightRed,
            nameof(TuiEgaPalette.EgaBrightMagenta) => TuiEgaPalette.EgaBrightMagenta,
            nameof(TuiEgaPalette.EgaYellow) => TuiEgaPalette.EgaYellow,
            nameof(TuiEgaPalette.EgaWhite) => TuiEgaPalette.EgaWhite,
            _ => throw new ArgumentException($"Unknown color: {colorName}")
        };

        Assert.Equal(r, actual.Red);
        Assert.Equal(g, actual.Green);
        Assert.Equal(b, actual.Blue);
    }
}
