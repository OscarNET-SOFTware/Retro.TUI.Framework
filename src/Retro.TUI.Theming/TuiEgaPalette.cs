// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEgaPalette.cs" company="OscarNET-SOFTware">
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
/// The standard 16-color EGA palette as <see cref="SKColor"/> constants.
/// </summary>
/// <remarks>
/// Colors are ordered by their canonical EGA index (0–15) as defined by the
/// IBM Enhanced Graphics Adapter specification. These constants are the single
/// source of truth for all color values used throughout the framework.
/// </remarks>
public static class TuiEgaPalette
{
    /// <summary>EGA index 0 — Black <c>#000000</c>.</summary>
    public static readonly SKColor EgaBlack = new(0x00, 0x00, 0x00);

    /// <summary>EGA index 1 — Blue <c>#0000AA</c>.</summary>
    public static readonly SKColor EgaBlue = new(0x00, 0x00, 0xAA);

    /// <summary>EGA index 2 — Green <c>#00AA00</c>.</summary>
    public static readonly SKColor EgaGreen = new(0x00, 0xAA, 0x00);

    /// <summary>EGA index 3 — Cyan <c>#00AAAA</c>.</summary>
    public static readonly SKColor EgaCyan = new(0x00, 0xAA, 0xAA);

    /// <summary>EGA index 4 — Red <c>#AA0000</c>.</summary>
    public static readonly SKColor EgaRed = new(0xAA, 0x00, 0x00);

    /// <summary>EGA index 5 — Magenta <c>#AA00AA</c>.</summary>
    public static readonly SKColor EgaMagenta = new(0xAA, 0x00, 0xAA);

    /// <summary>EGA index 6 — Brown <c>#AA5500</c>.</summary>
    public static readonly SKColor EgaBrown = new(0xAA, 0x55, 0x00);

    /// <summary>EGA index 7 — Light Gray <c>#AAAAAA</c>.</summary>
    public static readonly SKColor EgaLightGray = new(0xAA, 0xAA, 0xAA);

    /// <summary>EGA index 8 — Dark Gray <c>#555555</c>.</summary>
    public static readonly SKColor EgaDarkGray = new(0x55, 0x55, 0x55);

    /// <summary>EGA index 9 — Bright Blue <c>#5555FF</c>.</summary>
    public static readonly SKColor EgaBrightBlue = new(0x55, 0x55, 0xFF);

    /// <summary>EGA index 10 — Bright Green <c>#55FF55</c>.</summary>
    public static readonly SKColor EgaBrightGreen = new(0x55, 0xFF, 0x55);

    /// <summary>EGA index 11 — Bright Cyan <c>#55FFFF</c>.</summary>
    public static readonly SKColor EgaBrightCyan = new(0x55, 0xFF, 0xFF);

    /// <summary>EGA index 12 — Bright Red <c>#FF5555</c>.</summary>
    public static readonly SKColor EgaBrightRed = new(0xFF, 0x55, 0x55);

    /// <summary>EGA index 13 — Bright Magenta <c>#FF55FF</c>.</summary>
    public static readonly SKColor EgaBrightMagenta = new(0xFF, 0x55, 0xFF);

    /// <summary>EGA index 14 — Yellow <c>#FFFF55</c>.</summary>
    public static readonly SKColor EgaYellow = new(0xFF, 0xFF, 0x55);

    /// <summary>EGA index 15 — White <c>#FFFFFF</c>.</summary>
    public static readonly SKColor EgaWhite = new(0xFF, 0xFF, 0xFF);
}
