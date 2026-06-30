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
/// container or application code must set them explicitly, exactly as Turbo Vision
/// requires an explicit <c>TRect</c> for every view. If <see cref="Text"/> is longer
/// than <see cref="TuiView.Width"/>, the rendered text is clipped — no wrapping, no
/// implicit resize. A future explicit <c>SizeToFit()</c> helper may be added if a
/// real use case appears; it must remain opt-in, never automatic.
/// <para/>
/// <b>Color contract:</b> colors are always resolved via
/// <see cref="TuiPalette.Resolve(TuiColorRole)"/>, never hardcoded. By default the
/// label uses <see cref="TuiColorRole.LabelForeground"/> and
/// <see cref="TuiColorRole.LabelBackground"/>, but <see cref="ForegroundRole"/> and
/// <see cref="BackgroundRole"/> can be set to any other existing role to vary the
/// look per instance (e.g. an error label reusing a warning-themed role) — the fixed
/// EGA palette from M3.5 stays closed: callers pick among existing semantic roles,
/// they never supply a raw <c>SKColor</c>.
/// <para/>
/// <b>Background fill:</b> the declared <see cref="TuiView.Width"/> ×
/// <see cref="TuiView.Height"/> rectangle is always painted opaque with
/// <see cref="BackgroundRole"/>, even where <see cref="Text"/> is shorter — the
/// label never relies on whatever is painted behind it, consistent with
/// <c>TuiWindow</c>/<c>TuiDialog</c> always painting their own interior.
/// </remarks>
public sealed class TuiLabel : TuiView
{
    // ── Backing fields ────────────────────────────────────────────────────────

    // None of these can be auto-properties: every setter validates and/or calls
    // Invalidate() only when the value actually changes. IDE0032 suppressed to
    // avoid a spurious auto-property suggestion, same precedent as TuiView._isDirty.
#pragma warning disable IDE0032
    private string _text = string.Empty;
    private TuiColorRole _foregroundRole = TuiColorRole.LabelForeground;
    private TuiColorRole _backgroundRole = TuiColorRole.LabelBackground;
#pragma warning restore IDE0032

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the text displayed by this label.
    /// </summary>
    /// <value>Defaults to <see cref="string.Empty"/>.</value>
    /// <remarks>
    /// Setting this property to a different value calls <see cref="TuiView.Invalidate"/>
    /// automatically. Setting it to the same value is a no-op (no redraw requested).
    /// </remarks>
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
    /// Gets or sets the semantic color role used to resolve the text color.
    /// </summary>
    /// <value>Defaults to <see cref="TuiColorRole.LabelForeground"/>.</value>
    /// <remarks>
    /// Any existing <see cref="TuiColorRole"/> value may be used here — the fixed
    /// EGA palette stays closed; this only selects which already-mapped role this
    /// particular label instance resolves against. Setting it to the same value is
    /// a no-op.
    /// </remarks>
    public TuiColorRole ForegroundRole
    {
        get => _foregroundRole;
        set
        {
            if (_foregroundRole == value)
                return;

            _foregroundRole = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the semantic color role used to resolve the background fill color.
    /// </summary>
    /// <value>Defaults to <see cref="TuiColorRole.LabelBackground"/>.</value>
    /// <remarks>
    /// See <see cref="ForegroundRole"/> for the same role-selection contract,
    /// applied to the background fill painted by <see cref="Draw"/>.
    /// </remarks>
    public TuiColorRole BackgroundRole
    {
        get => _backgroundRole;
        set
        {
            if (_backgroundRole == value)
                return;

            _backgroundRole = value;
            Invalidate();
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Draws <see cref="Text"/> left-aligned starting at
    /// (<see cref="TuiView.AbsCol"/>, <see cref="TuiView.AbsRow"/>), clipped to
    /// <see cref="TuiView.Width"/>, using <see cref="ForegroundRole"/> and
    /// <see cref="BackgroundRole"/> resolved via <see cref="TuiPalette.Resolve(TuiColorRole)"/>.
    /// </remarks>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (!Visible || Width <= 0 || Height <= 0)
            return;

        ctx.FillRect(AbsCol, AbsRow, Width, Height, BackgroundRole);
        ctx.DrawTextClipped(AbsCol, AbsRow, Width, Text, ForegroundRole, BackgroundRole);
    }
}
