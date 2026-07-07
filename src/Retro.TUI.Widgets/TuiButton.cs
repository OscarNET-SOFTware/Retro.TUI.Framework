// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiButton.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Events;
using Retro.TUI.Rendering;
using Retro.TUI.Theming;
using Retro.TUI.Views;

using SkiaSharp;

namespace Retro.TUI.Widgets;

/// <summary>
/// A focusable, clickable button that emits a <see cref="TuiCommandEvent"/>
/// when activated by keyboard (<see cref="TuiKey.Enter"/>) or mouse click.
/// </summary>
/// <remarks>
/// <b>Text and accelerator:</b> <see cref="Text"/> may contain a tilde-delimited
/// accelerator marker using the Turbo Vision convention: the character between the
/// first pair of <c>~</c> characters is the accelerator key. For example,
/// <c>"~O~K"</c> displays as <c>OK</c> with <c>O</c> rendered in the accelerator
/// color. Text without tildes is displayed as-is.
/// <para/>
/// <b>Sizing contract:</b> <see cref="TuiView.Width"/> must be at least
/// <see cref="MinWidth"/> (10) columns — the minimum observed in the PC Tools 9.x
/// visual reference. Values below this minimum are silently clamped in
/// <see cref="Draw"/>. <see cref="TuiView.Height"/> is always 1; setting it to
/// any other value has no visual effect.
/// <para/>
/// <b>Rendering:</b> the button is drawn as <c>[ label ]</c> — square brackets
/// and a one-space pad on each side, with the label centered in the remaining
/// width. A one-cell drop shadow is drawn to the right and below using
/// <see cref="TuiColorRole.ButtonShadow"/>. Focus state changes the background
/// and foreground roles automatically.
/// <para/>
/// <b>Color contract:</b> follows the canonical widget color pattern: fixed default
/// roles per type, overridable per instance via <see cref="ForegroundColor"/> and
/// <see cref="BackgroundColor"/> (<see cref="TuiEgaColor"/>). The accelerator color
/// is always resolved from its role and cannot be overridden independently.
/// <para/>
/// <b>Focus:</b> <see cref="TuiView.Focusable"/> is <see langword="true"/> by
/// default. The button tracks focus state internally via
/// <see cref="TuiFocusEvent"/> delivered through <see cref="HandleEvent"/>; it
/// never references <c>TuiFocusManager</c> directly (Core-layer isolation).
/// <para/>
/// <b>Command emission:</b> on activation, a <see cref="TuiCommandEvent"/> carrying
/// <see cref="Command"/> is delivered to <see cref="TuiView.Parent"/> and bubbles
/// up the view tree to the application's <c>OnCommand</c> handler.
/// </remarks>
public sealed class TuiButton : TuiView
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Minimum button width in grid columns, matching the PC Tools 9.x visual
    /// reference. Values below this are silently clamped in <see cref="Draw"/>.
    /// </summary>
    public const int MinWidth = 10;

    /// <summary>
    /// The tilde character used to delimit the accelerator key in
    /// <see cref="Text"/>, following the Turbo Vision convention.
    /// </summary>
    private const char AcceleratorMarker = '~';

    // ── Default color roles (fixed per type, not overridable from outside) ────

    private readonly TuiColorRole _fgRole = TuiColorRole.ButtonForeground;
    private readonly TuiColorRole _bgRole = TuiColorRole.ButtonBackground;
    private readonly TuiColorRole _fgFocRole = TuiColorRole.ButtonFocusForeground;
    private readonly TuiColorRole _bgFocRole = TuiColorRole.ButtonFocusBackground;

    // ── Backing fields ────────────────────────────────────────────────────────

#pragma warning disable IDE0032
    private string _text = string.Empty;
    private TuiEgaColor? _foregroundColor;
    private TuiEgaColor? _backgroundColor;
    private bool _hasFocus;
#pragma warning restore IDE0032

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new <see cref="TuiButton"/> with default settings.
    /// </summary>
    public TuiButton()
    {
        Focusable = true;
        Height = 1;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the label text, optionally containing a tilde-delimited
    /// accelerator marker (e.g. <c>"~O~K"</c>, <c>"~C~ancel"</c>).
    /// </summary>
    /// <value>Defaults to <see cref="string.Empty"/>.</value>
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
    /// Gets the command emitted when the button is activated.
    /// Immutable after construction — set via object initializer.
    /// </summary>
    /// <value>Defaults to <see cref="TuiCommand.Ok"/>.</value>
    public TuiCommand Command { get; init; } = TuiCommand.Ok;

    /// <summary>
    /// Gets or sets an optional EGA color that overrides the default foreground
    /// role for this instance. <see langword="null"/> uses the default role.
    /// </summary>
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
    /// Gets or sets an optional EGA color that overrides the default background
    /// role for this instance. <see langword="null"/> uses the default role.
    /// </summary>
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

    /// <inheritdoc/>
    /// <remarks>
    /// Always <see langword="true"/>: the button intercepts
    /// <see cref="TuiMouseAction.ButtonDown"/> to emit its command on click.
    /// </remarks>
    public override bool HasCustomMouseHandling => true;

    // ── Standard factory properties ───────────────────────────────────────────

    /// <summary>Returns a new <c>OK</c> button wired to <see cref="TuiCommand.Ok"/>.</summary>
    public static TuiButton Ok => new() { Text = "~O~K", Command = TuiCommand.Ok };

    /// <summary>Returns a new <c>Cancel</c> button wired to <see cref="TuiCommand.Cancel"/>.</summary>
    public static TuiButton Cancel => new() { Text = "~C~ancel", Command = TuiCommand.Cancel };

    /// <summary>Returns a new <c>Yes</c> button wired to <see cref="TuiCommand.Yes"/>.</summary>
    public static TuiButton Yes => new() { Text = "~Y~es", Command = TuiCommand.Yes };

    /// <summary>Returns a new <c>No</c> button wired to <see cref="TuiCommand.No"/>.</summary>
    public static TuiButton No => new() { Text = "~N~o", Command = TuiCommand.No };

    /// <summary>Returns a new <c>Abort</c> button wired to <see cref="TuiCommand.Abort"/>.</summary>
    public static TuiButton Abort => new() { Text = "~A~bort", Command = TuiCommand.Abort };

    /// <summary>Returns a new <c>Retry</c> button wired to <see cref="TuiCommand.Retry"/>.</summary>
    public static TuiButton Retry => new() { Text = "~R~etry", Command = TuiCommand.Retry };

    /// <summary>Returns a new <c>Ignore</c> button wired to <see cref="TuiCommand.Ignore"/>.</summary>
    public static TuiButton Ignore => new() { Text = "~I~gnore", Command = TuiCommand.Ignore };

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Draw(TuiRenderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        if (!Visible || Height <= 0)
            return;

        int drawWidth = Math.Max(Width, MinWidth);

        // Resolve active colors based on focus state.
        SKColor fg = _foregroundColor.HasValue
            ? TuiPalette.Resolve(_foregroundColor.Value)
            : TuiPalette.Resolve(_hasFocus ? _fgFocRole : _fgRole);

        SKColor bg = _backgroundColor.HasValue
            ? TuiPalette.Resolve(_backgroundColor.Value)
            : TuiPalette.Resolve(_hasFocus ? _bgFocRole : _bgRole);

        SKColor accelFg = TuiPalette.Resolve(
            _hasFocus
                ? TuiColorRole.ButtonFocusAcceleratorForeground
                : TuiColorRole.ButtonAcceleratorForeground);

        // ── Background fill ───────────────────────────────────────────────────
        ctx.FillRect(AbsCol, AbsRow, drawWidth, 1, bg);

        // ── Shadow (right + below, 1 cell) ────────────────────────────────────
        ctx.DrawShadow(AbsCol, AbsRow, drawWidth, 1);

        // ── Button frame: "[ label ]" ─────────────────────────────────────────
        // Inner label area: drawWidth - 4 chars (2 brackets + 2 spaces).
        int innerWidth = drawWidth - 4;
        string display = BuildDisplayText(innerWidth);

        // Left bracket + space
        ctx.DrawText(AbsCol, AbsRow, "[ ", fg, bg);
        // Right space + bracket
        ctx.DrawText(AbsCol + drawWidth - 2, AbsRow, " ]", fg, bg);

        // ── Label with optional accelerator ───────────────────────────────────
        (string before, char? accel, string after) = ParseAccelerator(display);

        int labelCol = AbsCol + 2;    // after "[ "

        ctx.DrawText(labelCol, AbsRow, before, fg, bg);
        labelCol += before.Length;

        if (accel.HasValue)
        {
            ctx.DrawText(labelCol, AbsRow, accel.Value.ToString(), accelFg, bg);
            labelCol++;
            ctx.DrawText(labelCol, AbsRow, after, fg, bg);
        }
    }

    // ── Event handling ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Handles:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="TuiFocusEvent"/> — updates <c>_hasFocus</c> and requests repaint.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiKeyEvent"/> (<see cref="TuiKey.Enter"/>) — emits
    ///     <see cref="TuiCommandEvent"/> with <see cref="Command"/> when focused.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="TuiMouseEvent"/> (<see cref="TuiMouseAction.ButtonDown"/>) —
    ///     emits <see cref="TuiCommandEvent"/> with <see cref="Command"/> on click.
    ///   </description></item>
    /// </list>
    /// </remarks>
    public override bool HandleEvent(TuiEvent ev)
    {
        switch (ev)
        {
            case TuiFocusEvent focus:
                bool gained = focus.Action == TuiFocusAction.Gained;
                if (_hasFocus == gained)
                    return false;
                _hasFocus = gained;
                Invalidate();
                return false;   // focus events are never "consumed" — propagation continues

            case TuiKeyEvent { Key: TuiKey.Enter } when _hasFocus:
                Activate();
                return true;

            case TuiMouseEvent { Action: TuiMouseAction.ButtonDown }:
                Activate();
                return true;

            default:
                return false;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Emits a <see cref="TuiCommandEvent"/> carrying <see cref="Command"/> to
    /// the parent view, starting the bubble toward the application's
    /// <c>OnCommand</c> handler.
    /// </summary>
    private void Activate()
        => Parent?.HandleEvent(new TuiCommandEvent(Command));

    /// <summary>
    /// Builds the centered display string for the inner label area, stripping
    /// accelerator markers from <see cref="Text"/> before centering.
    /// </summary>
    private string BuildDisplayText(int innerWidth)
    {
        string clean = _text.Replace(
            AcceleratorMarker.ToString(), string.Empty, StringComparison.Ordinal);

        if (clean.Length >= innerWidth)
            return clean[..innerWidth];

        int pad = (innerWidth - clean.Length) / 2;
        return clean.PadLeft(pad + clean.Length).PadRight(innerWidth);
    }

    /// <summary>
    /// Parses the centered display string into three segments: the text before
    /// the accelerator character, the accelerator character itself, and the text
    /// after it. Returns the full string in the first element and
    /// <see langword="null"/> in the second when no accelerator marker is present
    /// in <see cref="Text"/>.
    /// </summary>
    private (string before, char? accel, string after) ParseAccelerator(string displayText)
    {
        // Find the accelerator character position from the original Text,
        // then map it onto the centered displayText via the clean (no-tilde) text.
        int markerIdx = _text.IndexOf(AcceleratorMarker, StringComparison.Ordinal);
        if (markerIdx < 0 || markerIdx + 1 >= _text.Length)
            return (displayText, null, string.Empty);

        // The accelerator character appears in displayText at an offset that
        // accounts for centering padding and any leading clean text.
        string cleanBefore = _text[..markerIdx].Replace(
            AcceleratorMarker.ToString(), string.Empty, StringComparison.Ordinal);

        int padding = displayText.Length > 0
            ? displayText.Length - displayText.TrimStart().Length
            : 0;
        int accelPos = padding + cleanBefore.Length;

        if (accelPos < 0 || accelPos >= displayText.Length)
            return (displayText, null, string.Empty);

        return (
            displayText[..accelPos],
            displayText[accelPos],
            accelPos + 1 < displayText.Length ? displayText[(accelPos + 1)..] : string.Empty
        );
    }
}
