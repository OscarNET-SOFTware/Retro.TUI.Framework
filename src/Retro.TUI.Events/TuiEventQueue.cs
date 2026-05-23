// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEventQueue.cs" company="OscarNET-SOFTware">
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

using System.Threading.Channels;

namespace Retro.TUI.Events;

/// <summary>
/// Thread-safe, bounded event queue that connects the host input layer to the message loop.
/// </summary>
/// <remarks>
/// The host (SDL2 via <c>SdlHost</c>) posts raw input events; the message loop consumes
/// them asynchronously on the UI thread. The queue is implemented on top of
/// <see cref="Channel{T}"/> for native async support with minimal allocations.
/// <para/>
/// The queue is <em>bounded</em> with a configurable capacity (default: <c>256</c>).
/// When the queue is full, <see cref="TuiMouseAction.Move"/> events are silently discarded
/// to prevent queue saturation during fast mouse movement.
/// For all other event types the oldest item is dropped to make room
/// (<see cref="BoundedChannelFullMode.DropOldest"/>), ensuring the host's
/// <c>PollEvents</c> call never blocks.
/// </remarks>
#pragma warning disable CA1711 // Intentional: TuiEventQueue is the framework equivalent of Turbo Vision's event queue abstraction.
public sealed class TuiEventQueue : IDisposable
#pragma warning restore CA1711
{
    /// <summary>Default maximum number of events the queue can hold.</summary>
    public const int DefaultCapacity = 256;

    private readonly Channel<TuiEvent> _channel;
    private bool _disposed;

    /// <summary>
    /// Initialises a new <see cref="TuiEventQueue"/> with the specified capacity.
    /// </summary>
    /// <param name="capacity">
    /// Maximum number of events the queue can hold before older events are dropped.
    /// Must be greater than zero. Defaults to <see cref="DefaultCapacity"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="capacity"/> is less than or equal to zero.
    /// </exception>
    public TuiEventQueue(int capacity = DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _channel = Channel.CreateBounded<TuiEvent>(new BoundedChannelOptions(capacity)
        {
            // The host must never block. Drop the oldest item to make room.
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,   // consumed only by the message loop
            SingleWriter = false,  // written by the host and by internal timer ticks
        });
    }

    // ── Producers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to post an event to the queue without blocking.
    /// </summary>
    /// <param name="evt">The event to enqueue.</param>
    /// <returns>
    /// <see langword="true"/> if the event was enqueued;
    /// <see langword="false"/> if the queue is full and the event was dropped
    /// (only possible when the underlying channel's <c>FullMode</c> is
    /// <see cref="BoundedChannelFullMode.Wait"/>, which is not used here).
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown after <see cref="Dispose"/> has been called.</exception>
    public bool TryPost(TuiEvent evt)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _channel.Writer.TryWrite(evt);
    }

    /// <summary>
    /// Asynchronously posts an event to the queue, waiting if the queue is full.
    /// </summary>
    /// <param name="evt">The event to enqueue.</param>
    /// <param name="ct">A cancellation token that can cancel the wait.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when the event has been enqueued.</returns>
    /// <remarks>
    /// Prefer <see cref="TryPost"/> from the SDL2 host's poll loop.
    /// Use <see cref="PostAsync"/> from producers that can afford to await,
    /// such as internal timer sources or test helpers.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown after <see cref="Dispose"/> has been called.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is cancelled.</exception>
    public async ValueTask PostAsync(TuiEvent evt, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _channel.Writer.WriteAsync(evt, ct).ConfigureAwait(false);
    }

    // ── Consumer ──────────────────────────────────────────────────────────

    /// <summary>
    /// Asynchronously reads the next event from the queue, waiting until one is available.
    /// </summary>
    /// <param name="ct">A cancellation token that cancels the wait (e.g. on application quit).</param>
    /// <returns>
    /// A <see cref="ValueTask{TuiEvent}"/> that completes with the next event
    /// once it is available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown after <see cref="Dispose"/> has been called.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is cancelled.</exception>
    public ValueTask<TuiEvent> ReadAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _channel.Reader.ReadAsync(ct);
    }

    /// <summary>
    /// Attempts to read an event from the queue without blocking.
    /// </summary>
    /// <param name="evt">
    /// When this method returns <see langword="true"/>, contains the next event.
    /// Otherwise, contains <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an event was available and has been read;
    /// <see langword="false"/> if the queue is currently empty.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown after <see cref="Dispose"/> has been called.</exception>
    public bool TryRead(out TuiEvent? evt)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _channel.Reader.TryRead(out evt);
    }

    // ── IDisposable ───────────────────────────────────────────────────────

    /// <summary>
    /// Completes the underlying channel and releases resources.
    /// </summary>
    /// <remarks>
    /// After disposal, any pending <see cref="ReadAsync"/> calls will complete
    /// with a <see cref="ChannelClosedException"/>.
    /// Subsequent calls to producer or consumer methods throw <see cref="ObjectDisposedException"/>.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _channel.Writer.TryComplete();
    }
}
