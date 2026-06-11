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

        // ── 4. Size and expose the desktop ────────────────────────────────────
        Desktop = new TuiDesktop
        {
            Col = 0,
            Row = 0,
            Width = grid.Columns,
            Height = grid.Rows,
        };

        FocusManager = new TuiFocusManager();

        // ── 5. Let the subclass populate the view tree ────────────────────────
        OnInitialize();

        // ── 6. Run the message loop ───────────────────────────────────────────
        using var queue = new TuiEventQueue();

        var loop = new TuiMessageLoop();
        loop.Run(
            host: ownedHost,
            queue: queue,
            desktop: Desktop,
            focusManager: FocusManager,
            renderCtx: renderCtx,
            ct: _cts.Token);
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
    /// Uses <c>SKFontManager.Default</c> to look up the family name declared in
    /// <see cref="TuiTheme.FontFamily"/>. Theme packages (e.g. <c>PcTools9Theme</c>)
    /// register their embedded font with <c>SKFontManager.Default</c> in their static
    /// initializer, so the lookup succeeds as soon as the theme assembly is loaded.
    /// Falls back to <c>SKTypeface.Default</c> when the family is not found.
    /// </remarks>
    private static SkiaSharp.SKTypeface ResolveTypeface(TuiTheme theme)
        => SkiaSharp.SKFontManager.Default.MatchFamily(theme.FontFamily)
           ?? SkiaSharp.SKTypeface.Default;
}
