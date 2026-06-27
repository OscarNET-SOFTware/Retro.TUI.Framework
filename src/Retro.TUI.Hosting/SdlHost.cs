// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SdlHost.cs" company="OscarNET-SOFTware">
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

using Silk.NET.SDL;

using SkiaSharp;

namespace Retro.TUI.Hosting;

/// <summary>
/// <see cref="ITuiHost"/> implementation backed by SDL2 (via Silk.NET) with a
/// software renderer and SkiaSharp for 2D drawing.
/// </summary>
/// <remarks>
/// This is the only class in the framework that references SDL2 types directly.
/// No other layer may take a dependency on <c>Silk.NET.SDL</c>.
/// <para/>
/// Rendering strategy — software renderer:
/// <list type="bullet">
///   <item><description>
///     SDL creates a hardware-accelerated or software <c>SDL_Renderer</c> targeting
///     a streaming <c>SDL_Texture</c> (ARGB8888, CPU-writable).
///   </description></item>
///   <item><description>
///     On each frame, the texture is locked to obtain a raw pixel pointer. A
///     <see cref="SKSurface"/> is wrapped directly over that pointer (zero-copy).
///   </description></item>
///   <item><description>
///     After drawing, the texture is unlocked and copied to the screen via
///     <c>SDL_RenderCopy</c> + <c>SDL_RenderPresent</c>.
///   </description></item>
/// </list>
/// This approach requires no OpenGL context and is portable across all SDL2 targets.
/// <para/>
/// Double-click detection: SDL2 does not synthesise double-click events natively.
/// <see cref="SdlHost"/> tracks the timestamp and position of the last
/// <see cref="TuiMouseAction.Click"/> and promotes the second click to
/// <see cref="TuiMouseAction.DoubleClick"/> when it occurs within
/// <see cref="DoubleClickThresholdMs"/> milliseconds at the same character cell.
/// <para/>
/// Resize handling: when <c>SDL_WINDOWEVENT_RESIZED</c> arrives and
/// <see cref="TuiHostOptions.Resizable"/> is <see langword="false"/>, the host calls
/// <c>SDL_SetWindowSize</c> to restore the original logical dimensions — guarding
/// against window managers (e.g. Wayland compositors) that ignore the non-resizable
/// flag. When <see cref="TuiHostOptions.Resizable"/> is <see langword="true"/>, the
/// backing texture and Skia surface are rebuilt to match the new physical size and a
/// <see cref="TuiCommandEvent"/>(<see cref="TuiCommand.Resize"/>) is posted with the
/// new dimensions as its <c>Parameter</c>.
/// </remarks>
public sealed class SdlHost : ITuiHost
{
    // ── Double-click detection constants ─────────────────────────────────

    /// <summary>
    /// Maximum interval in milliseconds between two clicks for them to be
    /// recognised as a double-click.
    /// </summary>
    private const uint DoubleClickThresholdMs = 500;

    // ── SDL2 objects ──────────────────────────────────────────────────────

    private readonly Sdl _sdl;

    private unsafe Window* _window;
    private unsafe Renderer* _renderer;
    private unsafe Silk.NET.SDL.Texture* _texture;

    // ── SkiaSharp surface ─────────────────────────────────────────────────

    private SKSurface? _surface;

    // ── Host state ────────────────────────────────────────────────────────

    // Plain fields: mutated directly by Initialize and HandleWindowResized.
    // IDE0032 suppressed to avoid spurious auto-property suggestions.
#pragma warning disable IDE0032
    private int _pixelWidth;
    private int _pixelHeight;
#pragma warning restore IDE0032

    // Logical (device-independent) dimensions from TuiHostOptions.
    // Kept to restore the window size when Resizable = false and the OS
    // sends an unsolicited SDL_WINDOWEVENT_RESIZED (common on Wayland).
    private int _logicalWidth;
    private int _logicalHeight;

    private bool _resizable;
#pragma warning disable IDE0032
    private float _dpiScale = 1.0f;
#pragma warning restore IDE0032
    private string _title = string.Empty;
    private bool _initialised;
    private bool _disposed;

    // ── Double-click tracking ─────────────────────────────────────────────

    private uint _lastClickTimestamp;
    private float _lastClickPixelX;
    private float _lastClickPixelY;

    // ── Construction ──────────────────────────────────────────────────────

    /// <summary>
    /// Initialises a new instance of <see cref="SdlHost"/>.
    /// </summary>
    /// <remarks>
    /// The SDL subsystem is loaded lazily; the native window is not created
    /// until <see cref="Initialize"/> is called.
    /// </remarks>
    public SdlHost()
    {
        _sdl = Sdl.GetApi();
    }

    // ── ITuiHost: properties ──────────────────────────────────────────────

    /// <inheritdoc/>
    public int PixelWidth => _pixelWidth;

    /// <inheritdoc/>
    public int PixelHeight => _pixelHeight;

    /// <inheritdoc/>
    public float DpiScale => _dpiScale;

    /// <inheritdoc/>
    public string Title
    {
        get => _title;
        set
        {
            _title = value ?? string.Empty;
            unsafe
            {
                if (_window != null)
                {
                    _sdl.SetWindowTitle(_window, _title);
                }
            }
        }
    }

    // ── ITuiHost: methods ─────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Initialize"/> has already been called on this instance.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when SDL2 fails to initialise, create the window, the renderer, or
    /// the backing texture.
    /// </exception>
    public void Initialize(TuiHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        if (_initialised)
        {
            throw new InvalidOperationException(
                "SdlHost has already been initialised. Call Initialize exactly once.");
        }

        InitialiseSdl();
        CreateWindow(options);
        CreateRenderer();
        CreateTexture();
        CreateSkiaSurface();

        _logicalWidth = options.Width;
        _logicalHeight = options.Height;
        _resizable = options.Resizable;
        _title = options.Title;
        _initialised = true;
    }

    /// <inheritdoc/>
    public bool PollEvents(TuiEventQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ThrowIfNotInitialised();
        ThrowIfDisposed();

        unsafe
        {
            Event sdlEvent = default;

            // Drain the SDL event queue; process every pending event this tick.
            while (_sdl.PollEvent(ref sdlEvent) != 0)
            {
                switch ((EventType)sdlEvent.Type)
                {
                    case EventType.Quit:
                        queue.TryPost(new TuiCommandEvent(TuiCommand.Quit));
                        return false;

                    case EventType.Windowevent:
                        HandleWindowEvent(sdlEvent.Window, queue);
                        break;

                    case EventType.Keydown:
                        HandleKeyDown(sdlEvent.Key, queue);
                        break;

                    case EventType.Textinput:
                        HandleTextInput(sdlEvent.Text, queue);
                        break;

                    case EventType.Mousemotion:
                        HandleMouseMotion(sdlEvent.Motion, queue);
                        break;

                    case EventType.Mousebuttondown:
                        HandleMouseButtonDown(sdlEvent.Button, queue);
                        break;

                    case EventType.Mousebuttonup:
                        HandleMouseButtonUp(sdlEvent.Button, queue);
                        break;

                    case EventType.Mousewheel:
                        HandleMouseWheel(sdlEvent.Wheel, queue);
                        break;
                }
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public void Present()
    {
        ThrowIfNotInitialised();
        ThrowIfDisposed();

        unsafe
        {
            // Flush all pending Skia draw calls to the pixel buffer.
            _surface!.Canvas.Flush();

            // Unlock the texture so SDL2 can read the pixel buffer for display.
            // The texture must be unlocked before RenderCopy — it was locked
            // either by CreateTexture (first frame) or by the re-lock at the
            // end of the previous Present call.
            _sdl.UnlockTexture(_texture);

            _sdl.RenderCopy(_renderer, _texture, null, null);
            _sdl.RenderPresent(_renderer);

            // Re-lock the texture so it is ready for the next frame.
            void* pixels;
            int pitch;
            _sdl.LockTexture(_texture, null, &pixels, &pitch);

            // Rewrap the Skia surface over the newly locked buffer.
            RewrapSkiaSurface(pixels, pitch);
        }
    }

    /// <inheritdoc/>
    public SKSurface AcquireRenderSurface()
    {
        ThrowIfNotInitialised();
        ThrowIfDisposed();
        return _surface!;
    }

    // ── IDisposable ───────────────────────────────────────────────────────

    /// <summary>
    /// Releases all SDL2 and SkiaSharp resources owned by this host.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _surface?.Dispose();
        _surface = null;

        unsafe
        {
            if (_texture != null)
            {
                // Unlock before destroying (texture may still be locked from last frame).
                _sdl.UnlockTexture(_texture);
                _sdl.DestroyTexture(_texture);
                _texture = null;
            }

            if (_renderer != null)
            {
                _sdl.DestroyRenderer(_renderer);
                _renderer = null;
            }

            if (_window != null)
            {
                _sdl.DestroyWindow(_window);
                _window = null;
            }
        }

        _sdl.QuitSubSystem(Sdl.InitVideo);
        _sdl.Dispose();
    }

    // ── Private — initialisation ──────────────────────────────────────────

    private void InitialiseSdl()
    {
        if (_sdl.Init(Sdl.InitVideo) < 0)
        {
            ThrowSdlError("Failed to initialise SDL2 video subsystem");
        }
    }

    private unsafe void CreateWindow(TuiHostOptions options)
    {
        uint flags = (uint)WindowFlags.Shown;

        if (options.Resizable)
        {
            flags |= (uint)WindowFlags.Resizable;
        }

        if (options.Borderless)
        {
            flags |= (uint)WindowFlags.Borderless;
        }

        int x = options.CenterOnScreen
            ? Sdl.WindowposCentered
            : Sdl.WindowposUndefined;

        _window = _sdl.CreateWindow(
            options.Title,
            x, x,
            options.Width, options.Height,
            flags);

        if (_window == null)
        {
            ThrowSdlError("Failed to create SDL2 window");
        }

        // Read back actual physical pixel dimensions (may differ on HiDPI).
        int drawW, drawH;
        _sdl.GetWindowSizeInPixels(_window, &drawW, &drawH);
        _pixelWidth = drawW;
        _pixelHeight = drawH;

        // Derive DPI scale from the ratio between physical and logical sizes.
        _dpiScale = drawW > 0 && options.Width > 0
            ? (float)drawW / options.Width
            : 1.0f;

        if (options.HideSystemCursor)
        {
            _sdl.ShowCursor(Sdl.Disable);
        }
    }

    private unsafe void CreateRenderer()
    {
        // SDL_RENDERER_ACCELERATED with fallback to software — cross-platform safe.
        _renderer = _sdl.CreateRenderer(
            _window,
            -1,
            (uint)(RendererFlags.Accelerated | RendererFlags.Presentvsync));

        if (_renderer == null)
        {
            // Fallback: pure software renderer.
            _renderer = _sdl.CreateRenderer(_window, -1, (uint)RendererFlags.Software);
        }

        if (_renderer == null)
        {
            ThrowSdlError("Failed to create SDL2 renderer");
        }
    }

    private unsafe void CreateTexture()
    {
        // Streaming texture: CPU writes pixels, GPU reads for display.
        // ARGB8888 matches SkiaSharp's default SKColorType.Bgra8888 memory layout
        // when the byte order is little-endian (all supported platforms).
        _texture = _sdl.CreateTexture(
            _renderer,
            Sdl.PixelformatArgb8888,
            (int)TextureAccess.Streaming,
            _pixelWidth,
            _pixelHeight);

        if (_texture == null)
        {
            ThrowSdlError("Failed to create SDL2 streaming texture");
        }

        // Lock the texture once to get the initial pixel buffer for Skia.
        void* pixels;
        int pitch;
        int result = _sdl.LockTexture(_texture, null, &pixels, &pitch);

        if (result < 0)
        {
            ThrowSdlError("Failed to lock SDL2 streaming texture");
        }

        // Store the pitch; the Skia surface is created in CreateSkiaSurface().
        _texturePitch = pitch;
        _texturePixels = (nint)pixels;
    }

    private unsafe void CreateSkiaSurface()
    {
        RewrapSkiaSurface((void*)_texturePixels, _texturePitch);
    }

    /// <summary>
    /// Destroys the current SDL texture and Skia surface and recreates them
    /// at the current <see cref="_pixelWidth"/> × <see cref="_pixelHeight"/>.
    /// Called when the window is resized while <see cref="_resizable"/> is
    /// <see langword="true"/>.
    /// </summary>
    private unsafe void RebuildTextureAndSurface()
    {
        // The texture may still be locked from the previous Present() call.
        // Unlock before destroying to avoid an SDL assert on debug builds.
        if (_texture != null)
        {
            _sdl.UnlockTexture(_texture);
            _sdl.DestroyTexture(_texture);
            _texture = null;
        }

        // CreateTexture() uses _pixelWidth/_pixelHeight, which were updated
        // by HandleWindowResized() before this call.
        CreateTexture();
        CreateSkiaSurface();
    }

    // Raw pixel pointer and row stride cached between lock/unlock cycles.
    private int _texturePitch;
    private nint _texturePixels;

    private unsafe void RewrapSkiaSurface(void* pixels, int pitch)
    {
        _surface?.Dispose();

        // Wrap Skia directly over the SDL texture pixel buffer — zero copy.
        // SKColorType.Bgra8888 = ARGB in little-endian memory = SDL ARGB8888.
        var info = new SKImageInfo(
            _pixelWidth,
            _pixelHeight,
            SKColorType.Bgra8888,
            SKAlphaType.Premul);

        _surface = SKSurface.Create(info, (nint)pixels, pitch);
    }

    // ── Private — SDL event translation ──────────────────────────────────

    private unsafe void HandleWindowEvent(WindowEvent wev, TuiEventQueue queue)
    {
        switch ((WindowEventID)wev.Event)
        {
            case WindowEventID.Close:
                queue.TryPost(new TuiCommandEvent(TuiCommand.Close));
                break;

            case WindowEventID.Resized:
                HandleWindowResized(queue);
                break;
        }
    }

    private unsafe void HandleWindowResized(TuiEventQueue queue)
    {
        if (!_resizable)
        {
            // Veto: restore the original logical size.
            // SDL_SetWindowSize operates in logical (device-independent) pixels;
            // the physical pixel dimensions and the Skia surface remain unchanged.
            _sdl.SetWindowSize(_window, _logicalWidth, _logicalHeight);
            return;
        }

        // Read the new physical pixel dimensions reported by the OS.
        int newW, newH;
        _sdl.GetWindowSizeInPixels(_window, &newW, &newH);

        if (newW == _pixelWidth && newH == _pixelHeight)
        {
            // SDL fires RESIZED even when the size has not actually changed
            // (e.g. on some Wayland compositors after a maximize/restore cycle).
            // Avoid the texture rebuild cost in that case.
            return;
        }

        _pixelWidth = newW;
        _pixelHeight = newH;

        // Rebuild the SDL texture and Skia surface at the new dimensions.
        RebuildTextureAndSurface();

        // Notify the framework so that the view tree can relayout.
        // Parameter carries the new physical pixel dimensions.
        queue.TryPost(new TuiCommandEvent(
            TuiCommand.Resize,
            Parameter: (Width: _pixelWidth, Height: _pixelHeight)));
    }

    private static unsafe void HandleKeyDown(KeyboardEvent kev, TuiEventQueue queue)
    {
        // SDL_KEYDOWN carries the virtual key; printable text arrives separately
        // via SDL_TEXTINPUT. Map only special (non-printable) keys here.
        var key = SdlKeyMapper.ToTuiKey(kev.Keysym.Sym);
        var mods = SdlKeyMapper.ToTuiModifiers(kev.Keysym.Mod);

        // Emit a key event for every special key and for modifier-qualified keys
        // (e.g. Ctrl+C), but skip bare printable characters — they arrive via
        // SDL_TEXTINPUT which produces a cleaner KeyChar value.
        if (key != TuiKey.None || mods != TuiModifiers.None)
        {
            queue.TryPost(new TuiKeyEvent(key, '\0', mods));
        }
    }

    private static unsafe void HandleTextInput(TextInputEvent tev, TuiEventQueue queue)
    {
        // SDL_TEXTINPUT delivers the UTF-8 string produced by the OS IME.
        // For single BMP characters (the only case the framework needs today)
        // decode the first scalar and emit a key event with KeyChar set.
        string? text;
        unsafe
        {
            text = new string((sbyte*)tev.Text);
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // Emit one event per character; multi-character IME compositions are rare
        // but must not be silently dropped.
        foreach (char ch in text)
        {
            queue.TryPost(new TuiKeyEvent(TuiKey.None, ch, TuiModifiers.None));
        }
    }

    private static void HandleMouseMotion(MouseMotionEvent mev, TuiEventQueue queue)
    {
        // Col/Row are placeholders (0); TuiMessageLoop recomputes them from
        // PixelX/PixelY using the active TuiGrid before dispatch.
        queue.TryPost(new TuiMouseEvent(
            TuiMouseAction.Move,
            Col: 0,
            Row: 0,
            TuiMouseButton.None,
            PixelX: mev.X,
            PixelY: mev.Y));
    }

    private static void HandleMouseButtonDown(MouseButtonEvent mev, TuiEventQueue queue)
    {
        TuiMouseButton button = MapMouseButton(mev.Button);

        // Col/Row are placeholders (0); TuiMessageLoop recomputes them from
        // PixelX/PixelY using the active TuiGrid before dispatch.
        queue.TryPost(new TuiMouseEvent(
            TuiMouseAction.ButtonDown,
            Col: 0,
            Row: 0,
            button,
            PixelX: mev.X,
            PixelY: mev.Y));
    }

    private void HandleMouseButtonUp(MouseButtonEvent mev, TuiEventQueue queue)
    {
        TuiMouseButton button = MapMouseButton(mev.Button);
        float pixelX = mev.X;
        float pixelY = mev.Y;

        // Col/Row are placeholders (0); TuiMessageLoop recomputes them from
        // PixelX/PixelY using the active TuiGrid before dispatch.

        // Always post ButtonUp.
        queue.TryPost(new TuiMouseEvent(TuiMouseAction.ButtonUp, 0, 0, button, pixelX, pixelY));

        // Synthesise Click.
        queue.TryPost(new TuiMouseEvent(TuiMouseAction.Click, 0, 0, button, pixelX, pixelY));

        // Detect double-click: same button, same pixel position, within threshold.
        uint now = mev.Timestamp;

        if (button == TuiMouseButton.Left
            && now - _lastClickTimestamp <= DoubleClickThresholdMs
            && pixelX == _lastClickPixelX
            && pixelY == _lastClickPixelY)
        {
            queue.TryPost(new TuiMouseEvent(TuiMouseAction.DoubleClick, 0, 0, button, pixelX, pixelY));
            // Reset so a third click does not immediately double-click again.
            _lastClickTimestamp = 0;
        }
        else
        {
            _lastClickTimestamp = now;
            _lastClickPixelX = pixelX;
            _lastClickPixelY = pixelY;
        }
    }

    private static void HandleMouseWheel(MouseWheelEvent mev, TuiEventQueue queue)
    {
        // SDL reports wheel deltas; for now map any wheel activity to a single
        // Wheel event. Col/Row are placeholders (0); TuiMessageLoop recomputes
        // them from PixelX/PixelY using the active TuiGrid before dispatch.
        queue.TryPost(new TuiMouseEvent(
            TuiMouseAction.Wheel,
            Col: 0,
            Row: 0,
            TuiMouseButton.None,
            PixelX: mev.MouseX,
            PixelY: mev.MouseY));
    }

    // ── Private — helpers ─────────────────────────────────────────────────

    private static TuiMouseButton MapMouseButton(byte sdlButton) =>
        sdlButton switch
        {
            Sdl.ButtonLeft => TuiMouseButton.Left,
            Sdl.ButtonRight => TuiMouseButton.Right,
            Sdl.ButtonMiddle => TuiMouseButton.Middle,
            _ => TuiMouseButton.None,
        };

    [DoesNotReturn]
    private void ThrowSdlError(string message)
    {
        string sdlError = _sdl.GetErrorS();
        throw new InvalidOperationException($"{message}: {sdlError}");
    }

    private void ThrowIfNotInitialised()
    {
        if (!_initialised)
        {
            throw new InvalidOperationException(
                "SdlHost has not been initialised. Call Initialize before using the host.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
