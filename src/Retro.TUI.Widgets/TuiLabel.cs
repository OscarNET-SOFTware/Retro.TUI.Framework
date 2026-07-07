// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiLabel.cs" company="OscarNET-SOFTware">
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
/// Static, non-focusable single-line text display.
/// </summary>
/// <remarks>
/// <see cref="TuiLabel"/> is the simplest widget in the framework: it has no
/// children, never receives keyboard focus (<see cref="TuiView.Focusable"/> stays
/// <see langword="false"/>), and does not override <see cref="TuiView.HandleEvent"/>.
/// <para/>
/// <b>Sizing contract:</b> <see cref="TuiView.Width"/> and <see cref="TuiView.Height"/>
/// are <em>not</em> derived automatically from <see cref="Text"/>. The owning
/// container or application code must set them explicitly. If <see cref="Text"/> is
/// longer than <see cref="TuiView.Width"/>, the rendered text is clipped — no
/// wrapping, no implicit resize.
/// <para/>
/// <b>Color contract:</b> each instance has fixed default semantic roles
/// (<see cref="TuiColorRole.LabelForeground"/> / <see cref="TuiColorRole.LabelBackground"/>)
/// that are always resolved via <see cref="TuiPalette.Resolve(TuiColorRole)"/>.
/// Optionally, <see cref="ForegroundColor"/> and <see cref="BackgroundColor"/> can
/// override those defaults with any of the 16 canonical EGA colors — still resolved
/// via <see cref="TuiPalette.Resolve(TuiEgaColor)"/>, never a raw
/// <see cref="SKColor"/>. This is the canonical color pattern for all widgets in
/// the framework.
/// <para/>
/// <b>Background fill:</b> the declared <see cref="TuiView.Width"/> ×
/// <see cref="TuiView.Height"/> rectangle is always painted opaque, consistent with
/// <c>TuiWindow</c> / <c>TuiDialog</c> always painting their own interior.
/// </remarks>
public sealed class TuiLabel : TuiView
{
    // ── Default color roles (fixed per type, not overridable from outside) ────

    private readonly TuiColorRole _foregroundRole = TuiColorRole.LabelForeground;
    private readonly TuiColorRole _backgroundRole = TuiColorRole.LabelBackground;

    // ── Backing fields ────────────────────────────────────────────────────────
#pragma warning disable IDE0032
    private string _text = string.Empty;
    private TuiEgaColor? _foregroundColor;
    private TuiEgaColor? _backgroundColor;
#pragma warning restore IDE0032

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the text displayed by this label.
    /// </summary>
    /// <value>Defaults to <see cref="string.Empty"/>.</value>
    /// <remarks>
    /// Setting this property to a different value calls
    /// <see cref="TuiView.Invalidate"/> automatically. Setting it to the same
    /// value is a no-op.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see langword="null"/> is assigned.
    /// </exception>
    public string Text
    {
        get => _text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (_text == value)
                return;

            _text = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets an optional EGA color that overrides the default foreground role
    /// for this instance.
    /// </summary>
    /// <value>
    /// <see langword="null"/> (default) — the label uses
    /// <see cref="TuiColorRole.LabelForeground"/> resolved via
    /// <see cref="TuiPalette.Resolve(TuiColorRole)"/>.
    /// Any <see cref="TuiEgaColor"/> value — that specific EGA color is used instead,
    /// resolved via <see cref="TuiPalette.Resolve(TuiEgaColor)"/>.
    /// </value>
    /// <remarks>
    /// The fixed EGA palette from M3.5 stays closed: callers pick among the 16
    /// named EGA entries, never a raw <see cref="SKColor"/>. Setting to the same
    /// value (including <see langword="null"/> → <see langword="null"/>) is a no-op.
    /// </remarks>
    public TuiEgaColor? ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (_foregroundColor == value)
                return;

            _foregroundColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets an optional EGA color that overrides the default background role
    /// for this instance.
    /// </summary>
    /// <value>
    /// <see langword="null"/> (default) — the label uses
    /// <see cref="TuiColorRole.LabelBackground"/> resolved via
    /// <see cref="TuiPalette.Resolve(TuiColorRole)"/>.
    /// Any <see cref="TuiEgaColor"/> value — that specific EGA color is used instead,
    /// resolved via <see cref="TuiPalette.Resolve(TuiEgaColor)"/>.
    /// </value>
    /// <remarks>
    /// See <see cref="ForegroundColor"/> for the same color-override contract,
    /// applied to the background fill painted by <see cref="Draw"/>.
    /// </remarks>
    public TuiEgaColor? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor == value)
                return;

            _backgroundColor = value;
            Invalidate();
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Fills the declared rectangle with the resolved background color, then draws
    /// <see cref="Text"/> left-aligned, clipped to <see cref="TuiView.Width"/>.
    /// Colors are resolved from <see cref="ForegroundColor"/> / <see cref="BackgroundColor"/>
    /// when set, falling back to the default semantic roles otherwise.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (!Visible || Width <= 0 || Height <= 0)
            return;

        SKColor fg = _foregroundColor.HasValue
            ? TuiPalette.Resolve(_foregroundColor.Value)
            : TuiPalette.Resolve(_foregroundRole);

        SKColor bg = _backgroundColor.HasValue
            ? TuiPalette.Resolve(_backgroundColor.Value)
            : TuiPalette.Resolve(_backgroundRole);

        ctx.FillRect(AbsCol, AbsRow, Width, Height, bg);
        ctx.DrawTextClipped(AbsCol, AbsRow, Width, Text, fg, bg);
    }
}
