// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TuiEventQueueTests.cs" company="OscarNET-SOFTware">
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
/// Tests for <see cref="TuiEventQueue"/>: posting, reading, capacity behaviour and disposal.
/// </summary>
public sealed class TuiEventQueueTests
{
    private static readonly TuiKeyEvent s_anyKey =
        new(TuiKey.Enter, '\0', TuiModifiers.None);

    // ── Construction ──────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultCapacity_DoesNotThrow()
    {
        using TuiEventQueue queue = new();
        Assert.NotNull(queue);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(64)]
    [InlineData(256)]
    [InlineData(1024)]
    public void Constructor_ValidCapacity_DoesNotThrow(int capacity)
    {
        using TuiEventQueue queue = new(capacity);
        Assert.NotNull(queue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_InvalidCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TuiEventQueue(capacity));
    }

    // ── TryPost ───────────────────────────────────────────────────────────

    [Fact]
    public void TryPost_EmptyQueue_ReturnsTrueAndEventIsEnqueued()
    {
        using TuiEventQueue queue = new();

        bool posted = queue.TryPost(s_anyKey);

        Assert.True(posted);
    }

    [Fact]
    public void TryPost_EventCanBeReadBack()
    {
        using TuiEventQueue queue = new();
        var evt = new TuiCommandEvent(TuiCommand.Ok);

        queue.TryPost(evt);
        bool read = queue.TryRead(out TuiEvent? dequeued);

        Assert.True(read);
        Assert.Equal(evt, dequeued);
    }

    [Fact]
    public void TryPost_AfterDispose_ThrowsObjectDisposedException()
    {
        TuiEventQueue queue = new();
        queue.Dispose();

        Assert.Throws<ObjectDisposedException>(() => queue.TryPost(s_anyKey));
    }

    // ── TryRead ───────────────────────────────────────────────────────────

    [Fact]
    public void TryRead_EmptyQueue_ReturnsFalseAndNullEvent()
    {
        using TuiEventQueue queue = new();

        bool read = queue.TryRead(out TuiEvent? evt);

        Assert.False(read);
        Assert.Null(evt);
    }

    [Fact]
    public void TryRead_AfterDispose_ThrowsObjectDisposedException()
    {
        TuiEventQueue queue = new();
        queue.Dispose();

        Assert.Throws<ObjectDisposedException>(() => queue.TryRead(out _));
    }

    [Fact]
    public void TryRead_PreservesInsertionOrder()
    {
        using TuiEventQueue queue = new();

        var first = new TuiCommandEvent(TuiCommand.Ok);
        var second = new TuiCommandEvent(TuiCommand.Cancel);

        queue.TryPost(first);
        queue.TryPost(second);

        queue.TryRead(out TuiEvent? a);
        queue.TryRead(out TuiEvent? b);

        Assert.Equal(first, a);
        Assert.Equal(second, b);
    }

    // ── ReadAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReadAsync_EventPostedBeforeRead_ReturnsEvent()
    {
        using TuiEventQueue queue = new();
        var evt = new TuiFocusEvent(TuiFocusAction.Gained);
        queue.TryPost(evt);

        TuiEvent result = await queue.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(evt, result);
    }

    [Fact]
    public async Task ReadAsync_EventPostedAfterRead_ReturnsEventWhenAvailable()
    {
        using TuiEventQueue queue = new();
        var evt = new TuiFocusEvent(TuiFocusAction.Lost);

        // Start reading before posting.
        ValueTask<TuiEvent> readTask = queue.ReadAsync(TestContext.Current.CancellationToken);

        // Post on a background thread to simulate the host.
        await Task.Run(() => queue.TryPost(evt));

        TuiEvent result = await readTask;
        Assert.Equal(evt, result);
    }

    [Fact]
    public async Task ReadAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using TuiEventQueue queue = new();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await queue.ReadAsync(cts.Token));
    }

    [Fact]
    public async Task ReadAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        TuiEventQueue queue = new();
        queue.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await queue.ReadAsync(TestContext.Current.CancellationToken));
    }

    // ── PostAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PostAsync_EventCanBeReadBack()
    {
        using TuiEventQueue queue = new();
        var evt = new TuiTimerEvent(TimeSpan.FromSeconds(1));

        await queue.PostAsync(evt, TestContext.Current.CancellationToken);
        TuiEvent result = await queue.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(evt, result);
    }

    [Fact]
    public async Task PostAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        TuiEventQueue queue = new();
        queue.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await queue.PostAsync(s_anyKey, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Use capacity 1 and fill it so PostAsync must wait.
        using TuiEventQueue queue = new(capacity: 1);
        queue.TryPost(s_anyKey); // fill the single slot

        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await queue.PostAsync(s_anyKey, cts.Token));
    }

    // ── Capacity and DropOldest behaviour ─────────────────────────────────

    [Fact]
    public void TryPost_WhenFull_DropOldestAndAcceptsNewEvent()
    {
        // Capacity 2: fill with two events, then post a third.
        // The oldest (first) item should be dropped.
        using TuiEventQueue queue = new(capacity: 2);

        var first = new TuiCommandEvent(TuiCommand.Ok);
        var second = new TuiCommandEvent(TuiCommand.Cancel);
        var third = new TuiCommandEvent(TuiCommand.Close);

        queue.TryPost(first);
        queue.TryPost(second);
        queue.TryPost(third); // triggers DropOldest — first is dropped

        queue.TryRead(out TuiEvent? a);
        queue.TryRead(out TuiEvent? b);

        // Remaining items should be second and third (first was dropped).
        Assert.Equal(second, a);
        Assert.Equal(third, b);
    }

    // ── Dispose ───────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        TuiEventQueue queue = new();

        queue.Dispose();

        // Second call must be a no-op, not an exception.
        var ex = Record.Exception(() => queue.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public async Task Dispose_PendingReadAsync_CompletesWithChannelClosedException()
    {
        TuiEventQueue queue = new();

        // Start awaiting before disposal.
        ValueTask<TuiEvent> pendingRead = queue.ReadAsync(TestContext.Current.CancellationToken);

        queue.Dispose();

        await Assert.ThrowsAsync<ChannelClosedException>(
            async () => await pendingRead);
    }

    // ── Concurrency smoke test ─────────────────────────────────────────────

    [Fact]
    public async Task ConcurrentProducers_AllEventsPostedAndConsumed()
    {
        const int EventCount = 50;
        const int ProducerCount = 5;

        // Use a larger capacity to avoid DropOldest affecting the count.
        using TuiEventQueue queue = new(capacity: EventCount * ProducerCount);

        // Launch multiple producer tasks simultaneously.
        Task[] producers = [.. Enumerable
            .Range(0, ProducerCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < EventCount; i++)
                {
                    queue.TryPost(new TuiTimerEvent(TimeSpan.FromMilliseconds(i)));
                }
            }))];

        await Task.WhenAll(producers);

        // Drain the queue and count consumed events.
        int consumed = 0;
        while (queue.TryRead(out _))
        {
            consumed++;
        }

        Assert.Equal(EventCount * ProducerCount, consumed);
    }
}
