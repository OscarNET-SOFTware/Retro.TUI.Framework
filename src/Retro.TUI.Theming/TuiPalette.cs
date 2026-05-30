// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiPalette.cs" company="OscarNET-SOFTware">
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
/// An immutable map from <see cref="TuiColorRole"/> values to concrete <see cref="SKColor"/> instances.
/// </summary>
/// <remarks>
/// A palette is always fully populated: every <see cref="TuiColorRole"/> defined in the enum
/// must have a corresponding entry. Theme authors should use <see cref="TuiTheme"/> to construct
/// and validate a complete palette before use.
/// <para/>
/// Instances are immutable after construction. To produce a modified copy, use <see cref="With"/>.
/// </remarks>
public sealed class TuiPalette
{
    private readonly Dictionary<TuiColorRole, SKColor> _colors;

    /// <summary>
    /// Initializes a new <see cref="TuiPalette"/> with the provided color map.
    /// </summary>
    /// <param name="colors">
    /// A dictionary mapping every <see cref="TuiColorRole"/> to a concrete <see cref="SKColor"/>.
    /// The dictionary is copied defensively; subsequent changes to the source are not reflected.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="colors"/> is <see langword="null"/>.
    /// </exception>
    public TuiPalette(IReadOnlyDictionary<TuiColorRole, SKColor> colors)
    {
        ArgumentNullException.ThrowIfNull(colors);
        _colors = new Dictionary<TuiColorRole, SKColor>(colors);
    }

    /// <summary>
    /// Returns the <see cref="SKColor"/> assigned to the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The color role to resolve.</param>
    /// <returns>The concrete color for the given role.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when <paramref name="role"/> has no entry in this palette.
    /// Use <see cref="GetOrDefault"/> for a safe lookup with a fallback.
    /// </exception>
    public SKColor this[TuiColorRole role] => _colors[role];

    /// <summary>
    /// Attempts to return the <see cref="SKColor"/> assigned to <paramref name="role"/>.
    /// Returns <paramref name="fallback"/> if the role is not present in this palette.
    /// </summary>
    /// <param name="role">The color role to resolve.</param>
    /// <param name="fallback">
    /// The color to return when <paramref name="role"/> is not found.
    /// Defaults to <see cref="SKColor.Empty"/>.
    /// </param>
    /// <returns>
    /// The color for <paramref name="role"/>, or <paramref name="fallback"/> if not found.
    /// </returns>
    public SKColor GetOrDefault(TuiColorRole role, SKColor fallback = default) =>
        _colors.TryGetValue(role, out SKColor color) ? color : fallback;

    /// <summary>
    /// Creates a new <see cref="TuiPalette"/> that is identical to this one except that
    /// the roles present in <paramref name="overrides"/> are replaced with their new values.
    /// </summary>
    /// <param name="overrides">
    /// A dictionary containing only the roles whose colors should change.
    /// Roles not present in <paramref name="overrides"/> are copied unchanged.
    /// </param>
    /// <returns>A new <see cref="TuiPalette"/> instance with the overrides applied.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="overrides"/> is <see langword="null"/>.
    /// </exception>
    public TuiPalette With(IReadOnlyDictionary<TuiColorRole, SKColor> overrides)
    {
        ArgumentNullException.ThrowIfNull(overrides);

        var merged = new Dictionary<TuiColorRole, SKColor>(_colors);

        foreach (KeyValuePair<TuiColorRole, SKColor> entry in overrides)
        {
            merged[entry.Key] = entry.Value;
        }

        return new TuiPalette(merged);
    }
}
