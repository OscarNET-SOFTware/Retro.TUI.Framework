// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PcTools9FontIntegrationTests.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Theme.PcTools9;

namespace Retro.TUI.Rendering;

/// <summary>
/// Integration tests that verify the font-loading contract between
/// <see cref="PcTools9Theme"/> and <see cref="TuiFont"/>.
/// </summary>
/// <remarks>
/// These tests require the embedded font resource to be present in the
/// <c>Retro.TUI.Theme.PcTools9</c> assembly — i.e. the file
/// <c>themes/Retro.TUI.Theme.PcTools9/Fonts/PxPlus_IBM_VGA_9x16.ttf</c>
/// must exist before building. They will fail at build time if the resource
/// is missing, which is the intended fail-fast behaviour.
/// </remarks>
public sealed class PcTools9FontIntegrationTests
{
    // ── TuiFont.Load via PcTools9Theme.Typeface ───────────────────────────────

    [Fact]
    public void TuiFont_Load_WithPcTools9Typeface_DoesNotThrow()
    {
        // Accessing Instance triggers the static field initializer of PcTools9Theme,
        // which loads PxPlus_IBM_VGA_9x16.ttf from the embedded resource and
        // exposes it via PcTools9Theme.Typeface.
        _ = PcTools9Theme.Instance;

        using var font = new TuiFont();

        var ex = Record.Exception(() =>
            font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize));

        Assert.Null(ex);
    }

    [Fact]
    public void TuiFont_Load_WithPcTools9Typeface_ExposesNonNullSkFont()
    {
        _ = PcTools9Theme.Instance;

        using var font = new TuiFont();
        font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize);

        Assert.NotNull(font.SkFont);
    }

    [Fact]
    public void TuiFont_Load_WithPcTools9Typeface_ReportsCorrectFamilyName()
    {
        _ = PcTools9Theme.Instance;

        using var font = new TuiFont();
        font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize);

        Assert.Equal(PcTools9Theme.FontFamilyName, font.SkFont.Typeface.FamilyName);
    }

    [Fact]
    public void TuiFont_Load_WithPcTools9Typeface_ReturnsValidMetrics()
    {
        _ = PcTools9Theme.Instance;

        using var font = new TuiFont();
        font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize);

        // Ascent is negative in SkiaSharp convention (above baseline).
        // Descent is positive (below baseline). Both must be non-zero.
        Assert.True(font.Metrics.Ascent < 0f, "Ascent must be negative (above baseline).");
        Assert.True(font.Metrics.Descent > 0f, "Descent must be positive (below baseline).");
    }

    [Fact]
    public void TuiFont_Load_CalledTwice_DoesNotThrow()
    {
        // A second Load must release the previous SKFont and reload cleanly.
        _ = PcTools9Theme.Instance;

        using var font = new TuiFont();
        font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize);

        var ex = Record.Exception(() =>
            font.Load(PcTools9Theme.Typeface, PcTools9Theme.Instance.FontSize));

        Assert.Null(ex);
    }

    // ── Theme / typeface consistency ──────────────────────────────────────────

    [Fact]
    public void Instance_FontFamily_MatchesFontFamilyNameConstant()
    {
        // TuiTheme.FontFamily and PcTools9Theme.FontFamilyName must always agree.
        // A divergence would cause TuiFont to load the wrong typeface at runtime.
        Assert.Equal(PcTools9Theme.FontFamilyName, PcTools9Theme.Instance.FontFamily);
    }

    [Fact]
    public void Typeface_FamilyName_MatchesFontFamilyNameConstant()
    {
        // The family name embedded in the .ttf file itself must match
        // PcTools9Theme.FontFamilyName. A mismatch indicates a wrong font file.
        Assert.Equal(PcTools9Theme.FontFamilyName, PcTools9Theme.Typeface.FamilyName);
    }
}
