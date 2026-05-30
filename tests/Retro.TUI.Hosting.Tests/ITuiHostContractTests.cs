// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITuiHostContractTests.cs" company="OscarNET-SOFTware">
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
/// Contract tests for <see cref="ITuiHost"/> using a lightweight in-process stub.
/// </summary>
/// <remarks>
/// These tests verify the behavioural contracts that every <see cref="ITuiHost"/>
/// implementation must honour, without requiring SDL2 native libraries.
/// <para/>
/// <see cref="FakeTuiHost"/> lives here, local to the test project, because it is
/// only used for Hosting layer tests. If other layers need it in the future,
/// it will be promoted to a shared <c>Retro.TUI.TestHelpers</c> project.
/// </remarks>
public sealed class ITuiHostContractTests
{
    // ── Initialize — null guard ───────────────────────────────────────────

    [Fact]
    public void Initialize_NullOptions_ThrowsArgumentNullException()
    {
        using var host = new FakeTuiHost();

        Assert.Throws<ArgumentNullException>(() => host.Initialize(null!));
    }

    // ── Initialize — invalid options are rejected ─────────────────────────

    [Fact]
    public void Initialize_InvalidOptions_ThrowsArgumentException()
    {
        using var host = new FakeTuiHost();
        var badOptions = new TuiHostOptions { Title = string.Empty, Width = 800, Height = 600 };

        Assert.Throws<ArgumentException>(() => host.Initialize(badOptions));
    }

    // ── Initialize — double initialisation ───────────────────────────────

    [Fact]
    public void Initialize_CalledTwice_ThrowsInvalidOperationException()
    {
        using var host = new FakeTuiHost();
        var options = ValidOptions();

        host.Initialize(options);

        Assert.Throws<InvalidOperationException>(() => host.Initialize(options));
    }

    // ── Dimensions after Initialize ───────────────────────────────────────

    [Fact]
    public void PixelWidth_AfterInitialize_MatchesOptionsWidth()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions { Title = "T", Width = 1024, Height = 768 });

        Assert.Equal(1024, host.PixelWidth);
    }

    [Fact]
    public void PixelHeight_AfterInitialize_MatchesOptionsHeight()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions { Title = "T", Width = 1024, Height = 768 });

        Assert.Equal(768, host.PixelHeight);
    }

    [Fact]
    public void DpiScale_AfterInitialize_IsPositive()
    {
        using var host = new FakeTuiHost();
        host.Initialize(ValidOptions());

        Assert.True(host.DpiScale > 0f);
    }

    // ── Title ─────────────────────────────────────────────────────────────

    [Fact]
    public void Title_AfterInitialize_MatchesOptionsTitle()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions { Title = "Hello TUI", Width = 1, Height = 1 });

        Assert.Equal("Hello TUI", host.Title);
    }

    [Fact]
    public void Title_SetAfterInitialize_ReturnsNewValue()
    {
        using var host = new FakeTuiHost();
        host.Initialize(ValidOptions());

        host.Title = "Updated";

        Assert.Equal("Updated", host.Title);
    }

    // ── PollEvents — before Initialize ───────────────────────────────────

    [Fact]
    public void PollEvents_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var host = new FakeTuiHost();
        using var queue = new TuiEventQueue();

        Assert.Throws<InvalidOperationException>(() => host.PollEvents(queue));
    }

    // ── PollEvents — returns true when no quit event ──────────────────────

    [Fact]
    public void PollEvents_NoQuitEvent_ReturnsTrue()
    {
        using var host = new FakeTuiHost();
        host.Initialize(ValidOptions());
        using var queue = new TuiEventQueue();

        bool continueLoop = host.PollEvents(queue);

        Assert.True(continueLoop);
    }

    // ── PollEvents — quit event causes false return ───────────────────────

    [Fact]
    public void PollEvents_WithQuitEvent_ReturnsFalseAndPostsQuitCommand()
    {
        using var host = new FakeTuiHost();
        host.Initialize(ValidOptions());
        host.EnqueueQuit();   // Simulate the user closing the window.
        using var queue = new TuiEventQueue();

        bool continueLoop = host.PollEvents(queue);

        Assert.False(continueLoop);
        Assert.True(queue.TryRead(out TuiEvent? evt));
        var commandEvent = Assert.IsType<TuiCommandEvent>(evt);
        Assert.Equal(TuiCommand.Quit, commandEvent.Command);
    }

    // ── AcquireRenderSurface — before Initialize ──────────────────────────

    [Fact]
    public void AcquireRenderSurface_BeforeInitialize_ThrowsInvalidOperationException()
    {
        using var host = new FakeTuiHost();

        Assert.Throws<InvalidOperationException>(() => host.AcquireRenderSurface());
    }

    // ── AcquireRenderSurface — returns non-null surface ───────────────────

    [Fact]
    public void AcquireRenderSurface_AfterInitialize_ReturnsNonNullSurface()
    {
        using var host = new FakeTuiHost();
        host.Initialize(ValidOptions());

        SKSurface surface = host.AcquireRenderSurface();

        Assert.NotNull(surface);
    }

    // ── Dispose — idempotent ──────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var host = new FakeTuiHost();
        host.Initialize(ValidOptions());

        host.Dispose();

        // Second Dispose must be a no-op.
        var ex = Record.Exception(host.Dispose);
        Assert.Null(ex);
    }

    // ── Resize — Resizable = false: no event posted, dimensions unchanged ────

    [Fact]
    public void PollEvents_ResizeWhenNotResizable_ReturnsTrueAndPostsNoEvent()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions
        {
            Title = "T",
            Width = 800,
            Height = 600,
            Resizable = false,
        });
        host.EnqueueResize(1024, 768);
        using var queue = new TuiEventQueue();

        bool continueLoop = host.PollEvents(queue);

        Assert.True(continueLoop);
        // No event must have been posted; the veto is silent.
        Assert.False(queue.TryRead(out _));
    }

    [Fact]
    public void PollEvents_ResizeWhenNotResizable_DimensionsUnchanged()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions
        {
            Title = "T",
            Width = 800,
            Height = 600,
            Resizable = false,
        });
        host.EnqueueResize(1024, 768);
        using var queue = new TuiEventQueue();

        host.PollEvents(queue);

        // Logical dimensions must be restored to the original values.
        Assert.Equal(800, host.PixelWidth);
        Assert.Equal(600, host.PixelHeight);
    }

    // ── Resize — Resizable = true: event posted, dimensions updated ───────────

    [Fact]
    public void PollEvents_ResizeWhenResizable_PostsResizeCommandWithNewDimensions()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions
        {
            Title = "T",
            Width = 800,
            Height = 600,
            Resizable = true,
        });
        host.EnqueueResize(1024, 768);
        using var queue = new TuiEventQueue();

        bool continueLoop = host.PollEvents(queue);

        Assert.True(continueLoop);
        Assert.True(queue.TryRead(out TuiEvent? evt));
        var cmd = Assert.IsType<TuiCommandEvent>(evt);
        Assert.Equal(TuiCommand.Resize, cmd.Command);
        var (w, h) = ((int Width, int Height))cmd.Parameter!;
        Assert.Equal(1024, w);
        Assert.Equal(768, h);
    }

    [Fact]
    public void PollEvents_ResizeWhenResizable_DimensionsUpdated()
    {
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions
        {
            Title = "T",
            Width = 800,
            Height = 600,
            Resizable = true,
        });
        host.EnqueueResize(1024, 768);
        using var queue = new TuiEventQueue();

        host.PollEvents(queue);

        Assert.Equal(1024, host.PixelWidth);
        Assert.Equal(768, host.PixelHeight);
    }

    [Fact]
    public void PollEvents_ResizeSameDimensionsWhenResizable_PostsNoEvent()
    {
        // SDL fires RESIZED even when dimensions are unchanged on some compositors.
        // The host must deduplicate and not post a spurious Resize command.
        using var host = new FakeTuiHost();
        host.Initialize(new TuiHostOptions
        {
            Title = "T",
            Width = 800,
            Height = 600,
            Resizable = true,
        });
        // Enqueue a resize to the same dimensions as the original.
        host.EnqueueResize(800, 600);
        using var queue = new TuiEventQueue();

        host.PollEvents(queue);

        Assert.False(queue.TryRead(out _));
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private static TuiHostOptions ValidOptions() =>
        new() { Title = "Test Window", Width = 800, Height = 600 };
}

// =============================================================================
// FakeTuiHost — in-process stub, no SDL2 required
// =============================================================================

/// <summary>
/// Minimal <see cref="ITuiHost"/> stub for unit tests.
/// </summary>
/// <remarks>
/// Simulates the host lifecycle without any native dependencies.
/// Kept local to the test project; will be promoted to <c>Retro.TUI.TestHelpers</c>
/// if other test projects need it.
/// </remarks>
internal sealed class FakeTuiHost : ITuiHost
{
    private bool _initialised;
    private bool _disposed;
    private bool _quitPending;
    private bool _resizePending;
    private int _pendingResizeW;
    private int _pendingResizeH;
    private bool _resizable;
    private int _logicalWidth;
    private int _logicalHeight;
    private SKSurface? _surface;
    private string _title = string.Empty;

    // ── ITuiHost properties ───────────────────────────────────────────────

    public int PixelWidth { get; private set; }
    public int PixelHeight { get; private set; }
    public float DpiScale { get; private set; } = 1.0f;

    public string Title
    {
        get => _title;
        set => _title = value ?? string.Empty;
    }

    // ── ITuiHost methods ──────────────────────────────────────────────────

    public void Initialize(TuiHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        if (_initialised)
        {
            throw new InvalidOperationException(
                "FakeTuiHost has already been initialised.");
        }

        PixelWidth = options.Width;
        PixelHeight = options.Height;
        _logicalWidth = options.Width;
        _logicalHeight = options.Height;
        _resizable = options.Resizable;
        _title = options.Title;
        _surface = SKSurface.Create(
            new SKImageInfo(PixelWidth, PixelHeight, SKColorType.Bgra8888));
        _initialised = true;
    }

    public bool PollEvents(TuiEventQueue queue)
    {
        ThrowIfNotInitialised();

        if (_quitPending)
        {
            _quitPending = false;
            queue.TryPost(new TuiCommandEvent(TuiCommand.Quit));
            return false;
        }

        if (_resizePending)
        {
            _resizePending = false;
            HandleFakeResize(_pendingResizeW, _pendingResizeH, queue);
        }

        return true;
    }

    private void HandleFakeResize(int newW, int newH, TuiEventQueue queue)
    {
        if (!_resizable)
        {
            // Veto: restore original dimensions, post nothing.
            PixelWidth = _logicalWidth;
            PixelHeight = _logicalHeight;
            return;
        }

        if (newW == PixelWidth && newH == PixelHeight)
        {
            // Same-size resize: skip rebuild and event.
            return;
        }

        PixelWidth = newW;
        PixelHeight = newH;

        // Rebuild the Skia surface at the new dimensions.
        _surface?.Dispose();
        _surface = SKSurface.Create(
            new SKImageInfo(PixelWidth, PixelHeight, SKColorType.Bgra8888));

        queue.TryPost(new TuiCommandEvent(
            TuiCommand.Resize,
            Parameter: (Width: PixelWidth, Height: PixelHeight)));
    }

    public void Present()
    {
        ThrowIfNotInitialised();
        // No-op in the fake.
    }

    public SKSurface AcquireRenderSurface()
    {
        ThrowIfNotInitialised();
        return _surface!;
    }

    // ── Test helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Schedules a quit event to be returned on the next <see cref="PollEvents"/> call.
    /// </summary>
    public void EnqueueQuit() => _quitPending = true;

    /// <summary>
    /// Schedules a resize event to be processed on the next <see cref="PollEvents"/> call.
    /// </summary>
    /// <param name="newWidth">New physical pixel width.</param>
    /// <param name="newHeight">New physical pixel height.</param>
    public void EnqueueResize(int newWidth, int newHeight)
    {
        _pendingResizeW = newWidth;
        _pendingResizeH = newHeight;
        _resizePending = true;
    }

    // ── IDisposable ───────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _surface?.Dispose();
        _surface = null;
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private void ThrowIfNotInitialised()
    {
        if (!_initialised)
        {
            throw new InvalidOperationException(
                "FakeTuiHost has not been initialised. Call Initialize first.");
        }
    }
}
