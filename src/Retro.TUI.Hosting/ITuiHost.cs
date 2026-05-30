// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITuiHost.cs" company="OscarNET-SOFTware">
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

using SkiaSharp;

namespace Retro.TUI.Hosting;

/// <summary>
/// Abstracts the native window host from the rest of the framework.
/// </summary>
/// <remarks>
/// <see cref="ITuiHost"/> is the only point of contact between the framework and the
/// underlying windowing system (SDL2, or any future backend). No other layer in the
/// framework references SDL2 types directly.
/// <para/>
/// Typical lifetime:
/// <list type="number">
///   <item><description>Create the implementation (e.g. <c>SdlHost</c>).</description></item>
///   <item><description>Call <see cref="Initialize"/> once with the desired options.</description></item>
///   <item><description>
///     On each message-loop iteration: call <see cref="PollEvents"/>, render using the
///     surface from <see cref="AcquireRenderSurface"/>, then call <see cref="Present"/>.
///   </description></item>
///   <item><description>Call <see cref="IDisposable.Dispose"/> when the application exits.</description></item>
/// </list>
/// </remarks>
public interface ITuiHost : IDisposable
{
    /// <summary>Gets the window width in physical pixels.</summary>
    int PixelWidth { get; }

    /// <summary>Gets the window height in physical pixels.</summary>
    int PixelHeight { get; }

    /// <summary>
    /// Gets the DPI scale factor reported by the operating system.
    /// </summary>
    /// <value>
    /// <c>1.0</c> on standard-density displays; <c>2.0</c> on HiDPI / Retina displays.
    /// </value>
    float DpiScale { get; }

    /// <summary>Gets or sets the window title shown in the OS task bar.</summary>
    string Title { get; set; }

    /// <summary>
    /// Creates and shows the native window using the supplied options.
    /// </summary>
    /// <param name="options">Configuration for the window to create.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="options"/> contains invalid values
    /// (e.g. non-positive width or height, or an empty title).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the host has already been initialised.
    /// </exception>
    /// <remarks>
    /// Must be called exactly once, before any other operation on this host.
    /// </remarks>
    void Initialize(TuiHostOptions options);

    /// <summary>
    /// Drains the OS event queue and translates each pending event into a
    /// <see cref="TuiEvent"/>, posting it to <paramref name="queue"/>.
    /// </summary>
    /// <param name="queue">
    /// The framework event queue that receives the translated events.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the message loop should continue;
    /// <see langword="false"/> if the host has requested shutdown
    /// (e.g. the user closed the window).
    /// </returns>
    /// <remarks>
    /// Called on every message-loop iteration. The method is synchronous and
    /// non-blocking: it processes only the events that are already in the OS
    /// queue at the moment of the call.
    /// </remarks>
    bool PollEvents(TuiEventQueue queue);

    /// <summary>
    /// Presents the fully rendered frame to the screen.
    /// </summary>
    /// <remarks>
    /// Must be called at the end of every message-loop iteration, after all
    /// drawing operations on the surface returned by <see cref="AcquireRenderSurface"/>
    /// have been completed and flushed.
    /// </remarks>
    void Present();

    /// <summary>
    /// Returns the <see cref="SKSurface"/> that the framework should draw the
    /// current frame onto.
    /// </summary>
    /// <returns>
    /// A valid <see cref="SKSurface"/> whose dimensions match
    /// <see cref="PixelWidth"/> × <see cref="PixelHeight"/>.
    /// </returns>
    /// <remarks>
    /// The returned surface is owned by the host. Callers must not dispose it.
    /// The surface is valid for a single frame: it must be used before the next
    /// call to <see cref="Present"/>.
    /// </remarks>
    SKSurface AcquireRenderSurface();
}
