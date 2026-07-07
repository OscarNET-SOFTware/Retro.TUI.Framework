// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiFocusManager.cs" company="OscarNET-SOFTware">
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

using Retro.TUI.Views;

namespace Retro.TUI.Core;

/// <summary>
/// Event argument carrying the view that has gained or lost keyboard focus.
/// </summary>
/// <remarks>
/// Passed by <see cref="TuiFocusManager.FocusChanged"/>. When focus is cleared via
/// <see cref="TuiFocusManager.Clear"/>, <see cref="FocusedView"/> is
/// <see langword="null"/>.
/// </remarks>
/// <remarks>
/// Initializes a new instance with the given focused view.
/// </remarks>
/// <param name="focusedView">The newly focused view, or <see langword="null"/> when focus is cleared.</param>
public sealed class TuiFocusChangedEventArgs(TuiView? focusedView) : EventArgs
{
    /// <summary>Gets the view that now holds keyboard focus, or <see langword="null"/> when cleared.</summary>
    public TuiView? FocusedView { get; } = focusedView;
}

/// <summary>
/// Manages keyboard focus across all focusable views in the application.
/// </summary>
/// <remarks>
/// <see cref="TuiFocusManager"/> maintains an ordered tab list of registered views
/// and tracks the currently focused one. It is owned by <see cref="TuiApplication"/>
/// and used exclusively by <see cref="TuiMessageLoop"/> to route keyboard events and
/// process Tab / Shift+Tab navigation.
/// <para/>
/// <b>Tab order:</b> views are cycled in registration order. <see cref="FocusNext"/>
/// wraps from the last registered view back to the first; <see cref="FocusPrevious"/>
/// wraps from the first back to the last.
/// <para/>
/// <b>Thread safety:</b> this class is not thread-safe. All calls must originate from
/// the message-loop thread.
/// </remarks>
public sealed class TuiFocusManager
{
    // ── State ─────────────────────────────────────────────────────────────────

    private readonly List<TuiView> _tabOrder = [];
#pragma warning disable IDE0032
    private TuiView? _current;
#pragma warning restore IDE0032

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised after the focused view changes.
    /// </summary>
    /// <remarks>
    /// The event argument carries the newly focused view (or <see langword="null"/>
    /// when focus is cleared via <see cref="Clear"/>).
    /// </remarks>
    public event EventHandler<TuiFocusChangedEventArgs>? FocusChanged;

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the view that currently holds keyboard focus, or <see langword="null"/>
    /// if no view is focused.
    /// </summary>
    public TuiView? Current => _current;

    // ── Registration ──────────────────────────────────────────────────────────

    /// <summary>
    /// Adds <paramref name="view"/> to the end of the tab order.
    /// </summary>
    /// <param name="view">
    /// The view to register. Must not be <see langword="null"/> and must have
    /// <see cref="TuiView.Focusable"/> set to <see langword="true"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="view"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="view"/> is not focusable or is already registered.
    /// </exception>
    public void Register(TuiView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (!view.Focusable)
            throw new ArgumentException(
                $"View '{view.GetType().Name}' is not focusable. Set Focusable = true before registering.",
                nameof(view));

        if (_tabOrder.Contains(view))
            throw new ArgumentException(
                $"View '{view.GetType().Name}' is already registered with this focus manager.",
                nameof(view));

        _tabOrder.Add(view);
    }

    /// <summary>
    /// Removes <paramref name="view"/> from the tab order.
    /// If the view currently holds focus, focus is cleared.
    /// </summary>
    /// <param name="view">The view to unregister. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="view"/> is <see langword="null"/>.
    /// </exception>
    public void Unregister(TuiView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        _tabOrder.Remove(view);

        if (ReferenceEquals(_current, view))
            SetCurrentInternal(null);
    }

    // ── Focus control ─────────────────────────────────────────────────────────

    /// <summary>
    /// Moves keyboard focus to <paramref name="view"/>.
    /// </summary>
    /// <param name="view">
    /// The view to focus. Must be registered and not <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="view"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="view"/> is not registered.
    /// </exception>
    public void SetFocus(TuiView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (!_tabOrder.Contains(view))
            throw new ArgumentException(
                $"View '{view.GetType().Name}' is not registered. Call Register() first.",
                nameof(view));

        SetCurrentInternal(view);
    }

    /// <summary>
    /// Attempts to move keyboard focus to <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view to focus. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if focus was transferred to <paramref name="view"/>;
    /// <see langword="false"/> if the view is not registered with this manager.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="view"/> is <see langword="null"/>.
    /// </exception>
    public bool TrySetFocus(TuiView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (!_tabOrder.Contains(view))
            return false;

        SetCurrentInternal(view);
        return true;
    }

    /// <inheritdoc cref="FocusNext"/>
    /// <remarks>
    /// Disabled views are skipped: the next enabled view in tab order receives
    /// focus. If all registered views are disabled, focus is not changed.
    /// </remarks>
    public void FocusNext()
    {
        if (_tabOrder.Count == 0)
            return;

        int start = _current is null
            ? 0
            : (_tabOrder.IndexOf(_current) + 1) % _tabOrder.Count;

        int next = FindEnabledFrom(start, forward: true);
        if (next >= 0)
            SetCurrentInternal(_tabOrder[next]);
    }

    /// <inheritdoc cref="FocusPrevious"/>
    /// <remarks>
    /// Disabled views are skipped: the previous enabled view in tab order
    /// receives focus. If all registered views are disabled, focus is not
    /// changed.
    /// </remarks>
    public void FocusPrevious()
    {
        if (_tabOrder.Count == 0)
            return;

        int start = _current is null
            ? _tabOrder.Count - 1
            : (_tabOrder.IndexOf(_current) - 1 + _tabOrder.Count) % _tabOrder.Count;

        int prev = FindEnabledFrom(start, forward: false);
        if (prev >= 0)
            SetCurrentInternal(_tabOrder[prev]);
    }

    /// <summary>
    /// Searches the tab order starting at <paramref name="start"/>, wrapping
    /// around, and returns the index of the first enabled view found.
    /// Returns <c>-1</c> if all registered views are disabled.
    /// </summary>
    private int FindEnabledFrom(int start, bool forward)
    {
        int count = _tabOrder.Count;
        for (int i = 0; i < count; i++)
        {
            int idx = forward
                ? (start + i) % count
                : (start - i + count) % count;

            if (_tabOrder[idx].Enabled)
                return idx;
        }
        return -1;
    }

    /// <summary>
    /// Returns whether <paramref name="view"/> currently holds keyboard focus.
    /// </summary>
    /// <param name="view">The view to test.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="view"/> is the current focused view.
    /// </returns>
    public bool IsFocused(TuiView view) => ReferenceEquals(_current, view);

    /// <summary>
    /// Returns whether <paramref name="view"/> is registered with this manager.
    /// </summary>
    /// <param name="view">The view to test. Must not be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="view"/> has been added via
    /// <see cref="Register"/> and not yet removed via <see cref="Unregister"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="view"/> is <see langword="null"/>.
    /// </exception>
    public bool IsRegistered(TuiView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        return _tabOrder.Contains(view);
    }

    /// <summary>
    /// Clears all focus state, leaving no view focused.
    /// </summary>
    /// <remarks>
    /// Typically called when a modal dialog is closed and the previous focus
    /// context needs to be reset before the new one is established.
    /// </remarks>
    public void Clear() => SetCurrentInternal(null);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Sets <see cref="_current"/> and raises <see cref="FocusChanged"/> if the
    /// focused view actually changed.
    /// </summary>
    private void SetCurrentInternal(TuiView? view)
    {
        if (ReferenceEquals(_current, view))
            return;

        _current = view;
        FocusChanged?.Invoke(this, new TuiFocusChangedEventArgs(_current));
    }
}
