// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiApplication.cs" company="OscarNET-SOFTware">
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

using System.Collections;

using Retro.TUI.Events;
using Retro.TUI.Hosting;
using Retro.TUI.Rendering;
using Retro.TUI.Theming;
using Retro.TUI.Views;

namespace Retro.TUI.Core;

/// <summary>
/// Abstract base class for all Retro.TUI applications.
/// </summary>
/// <remarks>
/// Subclass <see cref="TuiApplication"/>, override <see cref="OnInitialize"/> to build
/// the initial view tree, then call <see cref="Run"/> to start the application:
/// <code>
/// internal sealed class MyApp : TuiApplication
/// {
///     protected override void OnInitialize()
///     {
///         // Add windows, dialogs, etc. to Desktop.
///     }
/// }
///
/// // Entry point:
/// using var host = new SdlHost();
/// var options = new TuiHostOptions { Title = "My App", Width = 720, Height = 400 };
/// new MyApp().Run(host, PcTools9Theme.Instance, options);
/// </code>
/// <para/>
/// <b>Lifecycle</b> (executed by <see cref="Run"/> in order):
/// <list type="number">
///   <item><description>
///     <c>host.Initialize(options)</c> — creates and shows the SDL2 window.
///   </description></item>
///   <item><description>
///     Font and grid are initialised from the theme.
///   </description></item>
///   <item><description>
///     <see cref="Desktop"/> is sized to fill the full grid.
///   </description></item>
///   <item><description>
///     <see cref="OnInitialize"/> is called — the subclass populates the view tree.
///   </description></item>
///   <item><description>
///     <see cref="TuiMessageLoop"/> runs until the host signals shutdown or
///     <see cref="RequestQuit"/> is called.
///   </description></item>
///   <item><description>
///     Resources are disposed in reverse order.
///   </description></item>
/// </list>
/// </remarks>
public abstract class TuiApplication : IDisposable
{
    // ── Protected state (available to subclasses after OnInitialize) ──────────

    /// <summary>
    /// Gets the root view of the visual tree.
    /// </summary>
    /// <remarks>
    /// Available from the first line of <see cref="OnInitialize"/> onwards.
    /// Add top-level windows and overlays here.
    /// </remarks>
    protected TuiDesktop Desktop { get; private set; } = null!;

    /// <summary>
    /// Gets the keyboard focus manager for the current session.
    /// </summary>
    /// <remarks>
    /// Available from the first line of <see cref="OnInitialize"/> onwards.
    /// Use <see cref="TuiFocusManager.Register"/> to enrol focusable controls
    /// and <see cref="TuiFocusManager.SetFocus"/> to set the initial focus.
    /// </remarks>
    protected TuiFocusManager FocusManager { get; private set; } = null!;

    // ── Private state ─────────────────────────────────────────────────────────

    private CancellationTokenSource? _cts;
    private bool _disposed;

    // Fields promoted from Run() locals so that RunModal() can drive the same
    // poll → dispatch → render → present cycle inside a nested loop.
    // All four are null outside an active Run() session.
    //
    // CA2213 is suppressed for _queue and _renderCtx: both are created inside
    // Run() with 'using' declarations and are disposed when Run() returns.
    // These fields are non-owning references — TuiApplication.Dispose() must
    // NOT dispose them, as they may already be disposed by the time it is called.
#pragma warning disable CA2213
    private TuiMessageLoop? _loop;
    private ITuiHost? _host;
    private TuiEventQueue? _queue;
    private TuiRenderContext? _renderCtx;
#pragma warning restore CA2213

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes the window host, builds the view tree and runs the message
    /// loop until the application exits.
    /// </summary>
    /// <param name="host">
    /// The native window host to use. The application takes ownership of
    /// <paramref name="host"/> for the duration of the session and disposes it
    /// when <see cref="Run"/> returns.
    /// </param>
    /// <param name="theme">
    /// The visual theme to apply. Controls palette, font, desktop pattern and
    /// shadow parameters.
    /// </param>
    /// <param name="options">
    /// Host configuration: window title, pixel dimensions and display flags.
    /// All three <c>required</c> properties (<c>Title</c>, <c>Width</c>,
    /// <c>Height</c>) must be set before passing this value.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="host"/>, <paramref name="theme"/> or
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method blocks the calling thread until the message loop exits.
    /// It is designed to be called from the application entry point and returns
    /// only when the user closes the window or <see cref="RequestQuit"/> is called.
    /// </remarks>
    public void Run(ITuiHost host, TuiTheme theme, TuiHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(options);

        using ITuiHost ownedHost = host;
        _host = ownedHost;
        _cts = new CancellationTokenSource();

        // ── 1. Initialize the native window ───────────────────────────────────
        ownedHost.Initialize(options);

        // ── 2. Load font and initialize the grid ──────────────────────────────
        using var font = new TuiFont();
        font.Load(
            typeface: ResolveTypeface(theme),
            size: theme.FontSize);

        var grid = new TuiGrid();
        grid.Initialize(ownedHost.PixelWidth, ownedHost.PixelHeight, font.SkFont);

        // ── 3. Create the render context ──────────────────────────────────────
        using var renderCtx = new TuiRenderContext(grid, font, theme);
        _renderCtx = renderCtx;

        // ── 4. Size and expose the desktop ────────────────────────────────────
        Desktop = new TuiDesktop
        {
            Col = 0,
            Row = 0,
            Width = grid.Columns,
            Height = grid.Rows,
        };

        FocusManager = new TuiFocusManager();

        // ── 4b. Wire the desktop command sink ─────────────────────────────────
        // Commands that bubble up through the view tree without being consumed
        // reach the desktop's CommandSink. This is the clean path for application-
        // level commands (Close, custom commands) emitted from inside the tree.
        // Assigned before OnInitialize so views added there can already emit commands.
        Desktop.CommandSink = OnCommand;

        // ── 5. Let the subclass populate the view tree ────────────────────────
        OnInitialize();

        // ── 6. Run the message loop ───────────────────────────────────────────
        using var queue = new TuiEventQueue();
        _queue = queue;

        _loop = new TuiMessageLoop();
        _loop.Run(
            host: ownedHost,
            queue: queue,
            desktop: Desktop,
            focusManager: FocusManager,
            renderCtx: renderCtx,
            ct: _cts.Token,
            onCommand: OnCommand);

        // ── 7. Clear session references ───────────────────────────────────────
        _loop = null;
        _host = null;
        _queue = null;
        _renderCtx = null;
    }

    /// <summary>
    /// Requests the message loop to exit gracefully on the next iteration.
    /// </summary>
    /// <remarks>
    /// Safe to call from <see cref="OnInitialize"/> or from any view's
    /// <c>HandleEvent</c> override. Has no effect if <see cref="Run"/> has not
    /// been called yet or has already returned.
    /// </remarks>
    public void RequestQuit() => _cts?.Cancel();

    /// <summary>
    /// Opens <paramref name="dialog"/> as a modal over <paramref name="desktop"/>,
    /// runs a nested event loop until the dialog closes, then cleans up.
    /// </summary>
    /// <param name="dialog">The dialog to open. Must not be <see langword="null"/>.</param>
    /// <param name="desktop">
    /// The desktop that will host the dialog. Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The <see cref="TuiCommand"/> passed to <see cref="IModalDialog.Result"/>, or
    /// <see cref="TuiCommand.Cancel"/> if the dialog was closed without an explicit
    /// result (e.g. host shutdown).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dialog"/> or <paramref name="desktop"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="dialog"/> does not derive from <see cref="TuiView"/>.
    /// </exception>
    /// <remarks>
    /// <b>Lifecycle managed by this method (in order):</b>
    /// <list type="number">
    ///   <item><description><see cref="TuiDesktop.PushModal"/> — adds the dialog to the stack and to the view tree.</description></item>
    ///   <item><description>Nested event loop — drives the same poll → dispatch → render → present cycle.</description></item>
    ///   <item><description><see cref="TuiDesktop.PopModal"/> — removes the dialog from the stack and from the view tree.</description></item>
    /// </list>
    /// The caller does not need to add or remove the dialog from the desktop manually.
    /// <para/>
    /// <b>Reentrancy:</b> <see cref="RunModal"/> may be called recursively (e.g. a
    /// dialog opens a second dialog). Each call drives its own nested loop. There is
    /// no hard limit enforced here — application code should avoid pathological
    /// nesting depth.
    /// <para/>
    /// This method must only be called after <see cref="Run"/> has initialized the
    /// host, grid and render context (i.e. from <see cref="OnInitialize"/> or from
    /// a view's event handler during the main loop).
    /// </remarks>
    protected TuiCommand RunModal(IModalDialog dialog, TuiDesktop desktop)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(desktop);

        if (dialog is not TuiView dialogView)
            throw new ArgumentException(
                "The dialog must derive from TuiView.", nameof(dialog));

        // Guard: RunModal requires the application to be fully initialized.
        if (_loop is null || _host is null || _queue is null || _renderCtx is null)
            throw new InvalidOperationException(
                "RunModal may only be called after Run() has initialized the application.");

        desktop.PushModal(dialogView);
        _loop.BeginModal();
        try
        {
            while (!dialog.CloseRequested)
            {
                if (!_loop.RunIteration(_host, _queue, desktop, FocusManager, _renderCtx))
                    break;
            }
        }
        finally
        {
            _loop.EndModal();
            desktop.PopModal();
        }

        return dialog.Result;
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <summary>
    /// Releases the <see cref="CancellationTokenSource"/> owned by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources when <paramref name="disposing"/> is
    /// <see langword="true"/>.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>;
    /// <see langword="false"/> when called from the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _cts?.Dispose();

        _disposed = true;
    }

    // ── Abstract / overridable ────────────────────────────────────────────────

    /// <summary>
    /// Called when a <see cref="TuiCommandEvent"/> is dispatched, before the
    /// event is forwarded to the desktop view tree.
    /// </summary>
    /// <param name="ev">The command event being dispatched.</param>
    /// <remarks>
    /// Override this method to handle application-level commands. Always call
    /// <c>base.OnCommand(ev)</c> unless you intentionally want to suppress the
    /// default behaviour.
    /// <para/>
    /// The base implementation calls <see cref="RequestQuit"/> when
    /// <see cref="TuiCommand.Quit"/> is received. <see cref="TuiCommand.Cancel"/>
    /// is intentionally <em>not</em> handled by default — in PC Tools 9.x, Escape
    /// closes the active context (dialog, menu) rather than the application.
    /// Override and add a <c>Cancel → RequestQuit</c> path when the sample or
    /// application requires that behaviour.
    /// </remarks>
    protected virtual void OnCommand(TuiCommandEvent ev)
    {
        ArgumentNullException.ThrowIfNull(ev);

        if (ev.Command == TuiCommand.Quit)
            RequestQuit();
    }

    /// <summary>
    /// Called once after the window, font, grid and desktop have been initialized
    /// but before the message loop starts.
    /// </summary>
    /// <remarks>
    /// Override this method to:
    /// <list type="bullet">
    ///   <item><description>Add views and windows to <see cref="Desktop"/>.</description></item>
    ///   <item><description>Register focusable controls with <see cref="FocusManager"/>.</description></item>
    ///   <item><description>Set the initial keyboard focus.</description></item>
    /// </list>
    /// </remarks>
    protected abstract void OnInitialize();

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the SkiaSharp typeface for the active theme.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="TuiTheme.Typeface"/> directly when non-null (the preferred
    /// path for theme packages that load fonts from embedded resources, since
    /// <c>SKFontManager.RegisterTypeface</c> was removed in SkiaSharp 3.x).
    /// Falls back to <c>SKFontManager.Default.MatchFamily</c> using
    /// <see cref="TuiTheme.FontFamily"/>, and finally to
    /// <c>SKTypeface.Default</c> when neither resolves the typeface.
    /// </remarks>
    private static SkiaSharp.SKTypeface ResolveTypeface(TuiTheme theme)
        => theme.Typeface
           ?? SkiaSharp.SKFontManager.Default.MatchFamily(theme.FontFamily)
           ?? SkiaSharp.SKTypeface.Default;
}
