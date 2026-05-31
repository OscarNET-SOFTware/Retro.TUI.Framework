// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiDesktopTests.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Views;

/// <summary>
/// Tests for <see cref="TuiDesktop"/>: inheritance contracts, child management
/// delegation and default state.
/// </summary>
public sealed class TuiDesktopTests
{
    // ── Inheritance ───────────────────────────────────────────────────────────

    [Fact]
    public void TuiDesktop_IsAssignableFrom_TuiGroup()
    {
        var desktop = new TuiDesktop();

        Assert.IsAssignableFrom<TuiGroup>(desktop);
    }

    [Fact]
    public void TuiDesktop_IsAssignableFrom_TuiView()
    {
        var desktop = new TuiDesktop();

        Assert.IsAssignableFrom<TuiView>(desktop);
    }

    // ── Default state ─────────────────────────────────────────────────────────

    [Fact]
    public void NewDesktop_HasNoParent()
    {
        var desktop = new TuiDesktop();

        Assert.Null(desktop.Parent);
    }

    [Fact]
    public void NewDesktop_IsVisible()
    {
        var desktop = new TuiDesktop();

        Assert.True(desktop.Visible);
    }

    // ── Child management (delegates to TuiGroup) ──────────────────────────────

    [Fact]
    public void Add_Child_SetsParentToDesktop()
    {
        var desktop = new TuiDesktop();
        var child = new TuiGroup();

        desktop.Add(child);

        Assert.Same(desktop, child.Parent);
    }

    [Fact]
    public void Remove_Child_ClearsParent()
    {
        var desktop = new TuiDesktop();
        var child = new TuiGroup();
        desktop.Add(child);

        bool removed = desktop.Remove(child);

        Assert.True(removed);
        Assert.Null(child.Parent);
    }

    // ── FindAt (inherited hit-testing) ────────────────────────────────────────

    [Fact]
    public void FindAt_ChildAtPosition_ReturnsChild()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };
        var window = new TuiGroup { Col = 10, Row = 5, Width = 40, Height = 15 };
        desktop.Add(window);

        TuiView? hit = desktop.FindAt(absCol: 20, absRow: 10);

        Assert.Same(window, hit);
    }

    [Fact]
    public void FindAt_PointOutsideAllChildren_ReturnsNull()
    {
        var desktop = new TuiDesktop { Col = 0, Row = 0, Width = 80, Height = 25 };

        TuiView? hit = desktop.FindAt(absCol: 5, absRow: 5);

        Assert.Null(hit);
    }
}
