// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEgaColor.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Theming;

/// <summary>
/// Identifies one of the 16 canonical EGA colors available for direct use
/// in widget instances.
/// </summary>
/// <remarks>
/// <para>
/// Values are ordered by their canonical EGA index (0–15) as defined by the
/// IBM Enhanced Graphics Adapter specification, matching the constants in
/// <see cref="TuiEgaPalette"/>, which remains the single source of truth for
/// the concrete <see cref="SkiaSharp.SKColor"/> values.
/// </para>
/// <para>
/// This enum is the widget-facing API for direct color selection. It intentionally
/// excludes the two non-EGA grays (<c>#696969</c>, <c>#CACACA</c>) used internally
/// by certain semantic roles — those are only reachable via <see cref="TuiColorRole"/>
/// and <see cref="TuiPalette.Resolve(TuiColorRole)"/>.
/// </para>
/// <para>
/// Callers never supply raw <see cref="SkiaSharp.SKColor"/> values; they pick among
/// these 16 named entries, which <see cref="TuiPalette.Resolve(TuiEgaColor)"/> maps
/// to the corresponding <see cref="TuiEgaPalette"/> constant. The fixed EGA palette
/// established in M3.5 stays closed.
/// </para>
/// </remarks>
public enum TuiEgaColor
{
    /// <summary>EGA index 0 — Black <c>#000000</c>.</summary>
    Black = 0,

    /// <summary>EGA index 1 — Blue <c>#0000AA</c>.</summary>
    Blue = 1,

    /// <summary>EGA index 2 — Green <c>#00AA00</c>.</summary>
    Green = 2,

    /// <summary>EGA index 3 — Cyan <c>#00AAAA</c>.</summary>
    Cyan = 3,

    /// <summary>EGA index 4 — Red <c>#AA0000</c>.</summary>
    Red = 4,

    /// <summary>EGA index 5 — Magenta <c>#AA00AA</c>.</summary>
    Magenta = 5,

    /// <summary>EGA index 6 — Brown <c>#AA5500</c>.</summary>
    Brown = 6,

    /// <summary>EGA index 7 — Light Gray <c>#AAAAAA</c>.</summary>
    LightGray = 7,

    /// <summary>EGA index 8 — Dark Gray <c>#555555</c>.</summary>
    DarkGray = 8,

    /// <summary>EGA index 9 — Bright Blue <c>#5555FF</c>.</summary>
    BrightBlue = 9,

    /// <summary>EGA index 10 — Bright Green <c>#55FF55</c>.</summary>
    BrightGreen = 10,

    /// <summary>EGA index 11 — Bright Cyan <c>#55FFFF</c>.</summary>
    BrightCyan = 11,

    /// <summary>EGA index 12 — Bright Red <c>#FF5555</c>.</summary>
    BrightRed = 12,

    /// <summary>EGA index 13 — Bright Magenta <c>#FF55FF</c>.</summary>
    BrightMagenta = 13,

    /// <summary>EGA index 14 — Yellow <c>#FFFF55</c>.</summary>
    Yellow = 14,

    /// <summary>EGA index 15 — White <c>#FFFFFF</c>.</summary>
    White = 15,
}
