// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiLabelTests.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Rendering;
using Retro.TUI.Theming;
using Retro.TUI.Views;

using SkiaSharp;

namespace Retro.TUI.Widgets;

/// <summary>
/// Tests for <see cref="TuiLabel"/>: construction defaults, the
/// <see cref="TuiLabel.Text"/> contract (null guard, invalidation, no-op on
/// unchanged value) and the <see cref="TuiLabel.Draw"/> contract.
/// </summary>
public sealed class TuiLabelTests
{
    // ── Construction defaults ────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultText_IsEmpty()
    {
        var label = new TuiLabel();

        Assert.Equal(string.Empty, label.Text);
    }

    [Fact]
    public void Constructor_IsNotFocusable()
    {
        var label = new TuiLabel();

        Assert.False(label.Focusable);
    }

    // ── Text property ─────────────────────────────────────────────────────────

    [Fact]
    public void Text_SetNull_ThrowsArgumentNullException()
    {
        var label = new TuiLabel();

        Assert.Throws<ArgumentNullException>(() => label.Text = null!);
    }

    [Fact]
    public void Text_SetDifferentValue_UpdatesAndInvalidates()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Text = "Hello", Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        // First Draw clears the dirty flag set by construction/Add.
        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.Text = "Changed";

        Assert.Equal("Changed", label.Text);
        Assert.True(label.IsDirty);
    }

    [Fact]
    public void Text_SetSameValue_DoesNotInvalidate()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Text = "Hello", Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.Text = "Hello"; // same value as before

        Assert.False(label.IsDirty);
    }

    // ── ForegroundRole / BackgroundRole properties ──────────────────────────────

    [Fact]
    public void Constructor_DefaultRoles_AreLabelRoles()
    {
        var label = new TuiLabel();

        Assert.Equal(TuiColorRole.LabelForeground, label.ForegroundRole);
        Assert.Equal(TuiColorRole.LabelBackground, label.BackgroundRole);
    }

    [Fact]
    public void ForegroundRole_SetDifferentValue_UpdatesAndInvalidates()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.ForegroundRole = TuiColorRole.StatusForeground;

        Assert.Equal(TuiColorRole.StatusForeground, label.ForegroundRole);
        Assert.True(label.IsDirty);
    }

    [Fact]
    public void ForegroundRole_SetSameValue_DoesNotInvalidate()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.ForegroundRole = TuiColorRole.LabelForeground; // same as default

        Assert.False(label.IsDirty);
    }

    [Fact]
    public void BackgroundRole_SetDifferentValue_UpdatesAndInvalidates()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.BackgroundRole = TuiColorRole.StatusBackground;

        Assert.Equal(TuiColorRole.StatusBackground, label.BackgroundRole);
        Assert.True(label.IsDirty);
    }

    [Fact]
    public void BackgroundRole_SetSameValue_DoesNotInvalidate()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var group = new TuiGroup();
        var label = new TuiLabel { Col = 0, Row = 0, Width = 10, Height = 1 };
        group.Add(label);

        ctx.RenderFrame(surface, c => group.Draw(c));
        Assert.False(label.IsDirty);

        label.BackgroundRole = TuiColorRole.LabelBackground; // same as default

        Assert.False(label.IsDirty);
    }

    [Fact]
    public void Draw_NonDefaultRoles_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var label = new TuiLabel
        {
            Text = "Error: something went wrong",
            Col = 2,
            Row = 2,
            Width = 30,
            Height = 1,
            ForegroundRole = TuiColorRole.StatusForeground,
            BackgroundRole = TuiColorRole.StatusBackground,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => label.Draw(c)));

        Assert.Null(exception);
    }

    // ── Draw contract ─────────────────────────────────────────────────────────

    [Fact]
    public void Draw_NullContext_ThrowsArgumentNullException()
    {
        var label = new TuiLabel();

        Assert.Throws<ArgumentNullException>(() => label.Draw(null!));
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var label = new TuiLabel
        {
            Text = "Hello, Retro.TUI!",
            Col = 2,
            Row = 2,
            Width = 20,
            Height = 1,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => label.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_TextLongerThanWidth_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var label = new TuiLabel
        {
            Text = "This text is intentionally longer than its declared width",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => label.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_ZeroWidth_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var label = new TuiLabel
        {
            Text = "Hello",
            Col = 2,
            Row = 2,
            Width = 0,
            Height = 1,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => label.Draw(c)));

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_InvisibleLabel_DoesNotThrow()
    {
        TuiRenderContext ctx = BuildContext(40, 20);
        using SKSurface surface = CreateSurface(40 * 9, 20 * 16);

        var label = new TuiLabel
        {
            Text = "Hidden",
            Col = 2,
            Row = 2,
            Width = 10,
            Height = 1,
            Visible = false,
        };

        var exception = Record.Exception(() =>
            ctx.RenderFrame(surface, c => label.Draw(c)));

        Assert.Null(exception);
    }

    // ── Test infrastructure ──────────────────────────────────────────────────

    private static SKSurface CreateSurface(int w, int h)
        => SKSurface.Create(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));

    private static TuiRenderContext BuildContext(int cols, int rows)
    {
        int screenW = cols * 9;
        int screenH = rows * 16;

        using var skFont = new SKFont(SKTypeface.Default, 16f);
        var grid = new TuiGrid();
        grid.Initialize(screenW, screenH, skFont);

        var font = new TuiFont();
        font.Load(SKTypeface.Default, 16f);

        return new TuiRenderContext(grid, font);
    }
}
