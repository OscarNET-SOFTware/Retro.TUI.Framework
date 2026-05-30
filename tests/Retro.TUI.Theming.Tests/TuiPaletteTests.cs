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
/// Verifies construction, immutability, role resolution and the <c>With</c> override pattern.
/// </summary>
public sealed class TuiPaletteTests
{
    // ── Construction ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullColors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TuiPalette(null!));
    }

    [Fact]
    public void Constructor_ValidColors_DoesNotThrow()
    {
        var colors = new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        };

        var palette = new TuiPalette(colors);

        Assert.Equal(SKColors.Black, palette[TuiColorRole.DesktopBackground]);
    }

    [Fact]
    public void Constructor_MutatingSourceDictionary_DoesNotAffectPalette()
    {
        // Defensive copy: changes to the original source must not affect the palette.
        var colors = new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        };

        var palette = new TuiPalette(colors);
        colors[TuiColorRole.DesktopBackground] = SKColors.Red;

        Assert.Equal(SKColors.Black, palette[TuiColorRole.DesktopBackground]);
    }

    // ── Indexer ───────────────────────────────────────────────────────────

    [Fact]
    public void Indexer_ExistingRole_ReturnsCorrectColor()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.WindowBackground] = new SKColor(0x55, 0x55, 0xFF),
        });

        Assert.Equal(new SKColor(0x55, 0x55, 0xFF), palette[TuiColorRole.WindowBackground]);
    }

    [Fact]
    public void Indexer_MissingRole_ThrowsKeyNotFoundException()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>());

        Assert.Throws<KeyNotFoundException>(() =>
            _ = palette[TuiColorRole.DesktopBackground]);
    }

    // ── GetOrDefault ──────────────────────────────────────────────────────

    [Fact]
    public void GetOrDefault_ExistingRole_ReturnsColor()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.MenuBackground] = SKColors.White,
        });

        SKColor result = palette.GetOrDefault(TuiColorRole.MenuBackground, SKColors.Magenta);

        Assert.Equal(SKColors.White, result);
    }

    [Fact]
    public void GetOrDefault_MissingRole_ReturnsFallback()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>());

        SKColor result = palette.GetOrDefault(TuiColorRole.MenuBackground, SKColors.Magenta);

        Assert.Equal(SKColors.Magenta, result);
    }

    [Fact]
    public void GetOrDefault_MissingRoleNoFallback_ReturnsEmpty()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>());

        SKColor result = palette.GetOrDefault(TuiColorRole.MenuBackground);

        Assert.Equal(SKColor.Empty, result);
    }

    // ── With ──────────────────────────────────────────────────────────────

    [Fact]
    public void With_NullOverrides_ThrowsArgumentNullException()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>());

        Assert.Throws<ArgumentNullException>(() => palette.With(null!));
    }

    [Fact]
    public void With_ValidOverrides_ReturnsNewInstance()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        });

        TuiPalette modified = palette.With(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Blue,
        });

        Assert.NotSame(palette, modified);
    }

    [Fact]
    public void With_ValidOverrides_OverriddenRoleHasNewColor()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        });

        TuiPalette modified = palette.With(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Blue,
        });

        Assert.Equal(SKColors.Blue, modified[TuiColorRole.DesktopBackground]);
    }

    [Fact]
    public void With_ValidOverrides_OriginalPaletteUnchanged()
    {
        // With() must not mutate the original palette.
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        });

        _ = palette.With(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Blue,
        });

        Assert.Equal(SKColors.Black, palette[TuiColorRole.DesktopBackground]);
    }

    [Fact]
    public void With_ValidOverrides_NonOverriddenRolePreserved()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
            [TuiColorRole.WindowBackground] = new SKColor(0x55, 0x55, 0xFF),
        });

        TuiPalette modified = palette.With(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Blue,
        });

        Assert.Equal(new SKColor(0x55, 0x55, 0xFF), modified[TuiColorRole.WindowBackground]);
    }

    [Fact]
    public void With_EmptyOverrides_ProducesEquivalentPalette()
    {
        var palette = new TuiPalette(new Dictionary<TuiColorRole, SKColor>
        {
            [TuiColorRole.DesktopBackground] = SKColors.Black,
        });

        TuiPalette copy = palette.With(new Dictionary<TuiColorRole, SKColor>());

        Assert.Equal(SKColors.Black, copy[TuiColorRole.DesktopBackground]);
    }
}
