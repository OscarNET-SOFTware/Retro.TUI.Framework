// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiHostOptions.cs" company="OscarNET-SOFTware">
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

namespace Retro.TUI.Hosting;

/// <summary>
/// Immutable configuration passed to <see cref="ITuiHost.Initialize"/> to describe
/// the window to be created.
/// </summary>
/// <remarks>
/// All properties are <c>init</c>-only. Create the record with an object initialiser:
/// <code>
/// var options = new TuiHostOptions
/// {
///     Title  = "My App",
///     Width  = 800,
///     Height = 600
/// };
/// </code>
/// Validation is performed inside <see cref="ITuiHost.Initialize"/>; the record itself
/// does not throw on construction so that test stubs and deserialisation scenarios can
/// build instances freely and validate lazily.
/// </remarks>
public sealed class TuiHostOptions
{
    /// <summary>Gets the window title displayed in the OS task bar.</summary>
    /// <remarks>Must not be <see langword="null"/> or empty.</remarks>
    public required string Title { get; init; }

    /// <summary>Gets the window width in logical (device-independent) pixels.</summary>
    /// <remarks>Must be greater than zero.</remarks>
    public required int Width { get; init; }

    /// <summary>Gets the window height in logical (device-independent) pixels.</summary>
    /// <remarks>Must be greater than zero.</remarks>
    public required int Height { get; init; }

    /// <summary>
    /// Gets a value indicating whether the window can be resized by the user.
    /// </summary>
    /// <value><see langword="false"/> by default.</value>
    public bool Resizable { get; init; }

    /// <summary>
    /// Gets a value indicating whether the OS mouse cursor is hidden
    /// while the pointer is inside the window.
    /// </summary>
    /// <value>
    /// <see langword="true"/> by default, because the framework renders its own cursor.
    /// </value>
    public bool HideSystemCursor { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the window is centred on the primary monitor
    /// when it first appears.
    /// </summary>
    /// <value><see langword="true"/> by default.</value>
    public bool CenterOnScreen { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the window is created without native OS
    /// decorations (title bar, resize borders, system menu).
    /// </summary>
    /// <value>
    /// <see langword="true"/> by default. When <see langword="true"/>, the framework
    /// is responsible for rendering its own title bar and window chrome.
    /// </value>
    public bool Borderless { get; init; } = true;

    // ── Internal validation ───────────────────────────────────────────────

    /// <summary>
    /// Validates all option values and throws if any constraint is violated.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <see cref="Title"/> is <see langword="null"/> or white-space,
    /// or when <see cref="Width"/> or <see cref="Height"/> is not positive.
    /// </exception>
    /// <remarks>
    /// Called by <see cref="ITuiHost.Initialize"/> as its first operation.
    /// Keeping validation here, rather than in property setters, preserves the
    /// immutability contract of the record and makes the rules directly testable.
    /// </remarks>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new ArgumentException(
                "Window title must not be null or white-space.",
                nameof(Title));
        }

        if (Width <= 0)
        {
            throw new ArgumentException(
                $"Window width must be greater than zero, but was {Width}.",
                nameof(Width));
        }

        if (Height <= 0)
        {
            throw new ArgumentException(
                $"Window height must be greater than zero, but was {Height}.",
                nameof(Height));
        }
    }
}
