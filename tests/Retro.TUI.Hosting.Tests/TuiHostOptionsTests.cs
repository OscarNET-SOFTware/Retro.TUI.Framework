// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiHostOptionsTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Hosting;

/// <summary>
/// Tests for <see cref="TuiHostOptions"/> construction, default values and
/// the internal <c>Validate()</c> contract.
/// </summary>
public sealed class TuiHostOptionsTests
{
    // ── Default values ────────────────────────────────────────────────────

    [Fact]
    public void Defaults_Resizable_IsFalse()
    {
        var options = ValidOptions();
        Assert.False(options.Resizable);
    }

    [Fact]
    public void Defaults_HideSystemCursor_IsTrue()
    {
        var options = ValidOptions();
        Assert.True(options.HideSystemCursor);
    }

    [Fact]
    public void Defaults_CenterOnScreen_IsTrue()
    {
        var options = ValidOptions();
        Assert.True(options.CenterOnScreen);
    }

    [Fact]
    public void Defaults_Borderless_IsTrue()
    {
        var options = ValidOptions();
        Assert.True(options.Borderless);
    }

    // ── Required properties — init values round-trip ──────────────────────

    [Fact]
    public void Title_RoundTrips()
    {
        var options = new TuiHostOptions { Title = "Hello", Width = 1, Height = 1 };
        Assert.Equal("Hello", options.Title);
    }

    [Fact]
    public void Width_RoundTrips()
    {
        var options = new TuiHostOptions { Title = "T", Width = 1280, Height = 1 };
        Assert.Equal(1280, options.Width);
    }

    [Fact]
    public void Height_RoundTrips()
    {
        var options = new TuiHostOptions { Title = "T", Width = 1, Height = 720 };
        Assert.Equal(720, options.Height);
    }

    // ── Validate() — valid inputs ─────────────────────────────────────────

    [Fact]
    public void Validate_ValidOptions_DoesNotThrow()
    {
        var options = ValidOptions();
        // Should complete without throwing.
        options.Validate();
    }

    // ── Validate() — Title ────────────────────────────────────────────────

    [Fact]
    public void Validate_TitleIsEmpty_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = string.Empty, Width = 1, Height = 1 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Title", ex.ParamName);
    }

    [Fact]
    public void Validate_TitleIsWhiteSpace_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = "   ", Width = 1, Height = 1 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Title", ex.ParamName);
    }

    // ── Validate() — Width ────────────────────────────────────────────────

    [Fact]
    public void Validate_WidthIsZero_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = "T", Width = 0, Height = 1 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Width", ex.ParamName);
    }

    [Fact]
    public void Validate_WidthIsNegative_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = "T", Width = -1, Height = 1 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Width", ex.ParamName);
    }

    // ── Validate() — Height ───────────────────────────────────────────────

    [Fact]
    public void Validate_HeightIsZero_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = "T", Width = 1, Height = 0 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Height", ex.ParamName);
    }

    [Fact]
    public void Validate_HeightIsNegative_ThrowsArgumentException()
    {
        var options = new TuiHostOptions { Title = "T", Width = 1, Height = -1 };

        var ex = Assert.Throws<ArgumentException>(options.Validate);

        Assert.Equal("Height", ex.ParamName);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static TuiHostOptions ValidOptions() =>
        new() { Title = "Test Window", Width = 800, Height = 600 };
}
