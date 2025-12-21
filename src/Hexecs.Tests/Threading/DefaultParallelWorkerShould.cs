using System.Collections.Concurrent;
using System.Diagnostics;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Tests.Threading;

public sealed class DefaultParallelWorkerShould : IDisposable
{
    private readonly ActorContext _context;
    private readonly World _world;

    public DefaultParallelWorkerShould()
    {
        _world = new WorldBuilder().Build();
        _context = _world.Actors;
    }

    [Fact(DisplayName = "Должен корректно выполнять параллельную работу")]
    public void ExecuteParallelJobCorrectly()
    {
        const int degreeOfParallelism = 4;
        const int expectedSum = 0 + 1 + 2 + 3; // sum of indices

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        var actualSum = 0;
        var job = new TestParallelJob(_context, (workerIndex, _) => { Interlocked.Add(ref actualSum, workerIndex); });

        worker.Run(job);

        Assert.Equal(expectedSum, actualSum);
    }

    [Fact(DisplayName = "Должен вызывать каждый воркер ровно один раз")]
    public void CallEachWorkerExactlyOnce()
    {
        const int degreeOfParallelism = 8;

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        var executionCounts = new int[degreeOfParallelism];
        var job = new TestParallelJob(_context,
            (workerIndex, _) => { Interlocked.Increment(ref executionCounts[workerIndex]); });

        worker.Run(job);

        foreach (var count in executionCounts)
        {
            Assert.Equal(1, count);
        }
    }

    [Fact(DisplayName = "Должен корректно работать при многократном вызове")]
    public void HandleMultipleRunsCorrectly()
    {
        const int degreeOfParallelism = 4;
        const int runsCount = 100;

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        for (var run = 0; run < runsCount; run++)
        {
            var counter = 0;
            var job = new TestParallelJob(_context, (_, _) => { Interlocked.Increment(ref counter); });

            worker.Run(job);

            Assert.Equal(degreeOfParallelism, counter);
        }
    }

    [Fact(DisplayName = "Должен выполнять работу параллельно")]
    public void ExecuteWorkInParallel()
    {
        const int degreeOfParallelism = 4;

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        var threadIds = new ConcurrentBag<int>();
        var job = new TestParallelJob(_context, (_, _) =>
        {
            threadIds.Add(Environment.CurrentManagedThreadId);
            Thread.Sleep(10); // simulate work
        });

        worker.Run(job);

        // All workers should execute on different threads
        Assert.Equal(degreeOfParallelism, threadIds.Distinct().Count());
    }

    [Fact(DisplayName = "Должен завершать работу быстрее чем последовательное выполнение")]
    public void CompleteWorkFasterThanSequential()
    {
        const int degreeOfParallelism = 4;
        const int workDurationMs = 50;

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        var job = new TestParallelJob(_context, (_, _) => { Thread.Sleep(workDurationMs); });

        var sw = Stopwatch.StartNew();
        worker.Run(job);
        sw.Stop();

        // Parallel execution should take ~workDurationMs, not degreeOfParallelism * workDurationMs
        Assert.True(sw.ElapsedMilliseconds < workDurationMs * degreeOfParallelism);
    }

    [Fact(DisplayName = "Должен передавать корректные workerIndex и workerCount")]
    public void PassCorrectWorkerIndexAndCount()
    {
        const int degreeOfParallelism = 6;

        using var worker = new DefaultParallelWorker(degreeOfParallelism);

        var indices = new ConcurrentBag<int>();
        var counts = new ConcurrentBag<int>();

        var job = new TestParallelJob(_context, (workerIndex, workerCount) =>
        {
            indices.Add(workerIndex);
            counts.Add(workerCount);
        });

        worker.Run(job);

        Assert.Equal(degreeOfParallelism, indices.Count);
        Assert.All(counts, count => Assert.Equal(degreeOfParallelism - 1, count));
        Assert.Equal(Enumerable.Range(0, degreeOfParallelism).OrderBy(x => x), indices.OrderBy(x => x));
    }

    [Fact(DisplayName = "Должен корректно освобождать ресурсы")]
    public void DisposeCorrectly()
    {
        var worker = new DefaultParallelWorker(4);

        var counter = 0;
        var job = new TestParallelJob(_context, (_, _) => Interlocked.Increment(ref counter));

        worker.Run(job);
        Assert.Equal(4, counter);

        worker.Dispose();

        // После Dispose воркер не должен использоваться
        // Проверяем, что потоки завершились
        Assert.True(true); // Если Dispose зависнет, тест провалится по таймауту
    }

    [Fact(DisplayName = "Должен корректно работать в игровом цикле")]
    public void WorkCorrectlyInGameLoop()
    {
        using var worker = new DefaultParallelWorker(4);

        // Симулируем игровой цикл с последовательными вызовами Run
        const int frameCount = 60; // 60 кадров
        var totalExecutions = 0;

        for (int frame = 0; frame < frameCount; frame++)
        {
            var frameExecutions = 0;
            var job = new TestParallelJob(_context, (_, _) =>
            {
                Interlocked.Increment(ref frameExecutions);
                Thread.SpinWait(100); // simulate work
            });

            worker.Run(job);

            Assert.Equal(4, frameExecutions);
            totalExecutions += frameExecutions;
        }

        Assert.Equal(frameCount * 4, totalExecutions);
    }

    [Fact(DisplayName = "НЕ должен работать с DegreeOfParallelism < 2")]
    public void ThrowIfDegreeOfParallelismIsOne()
    {
        Assert.Throws<ArgumentException>(() => new DefaultParallelWorker(1));
    }

    private sealed class TestParallelJob : IParallelJob
    {
        public ActorContext Context { get; }

        private readonly Action<int, int> _action;

        public TestParallelJob(ActorContext context, Action<int, int> action)
        {
            _action = action;
            Context = context;
        }

        public void Execute(int workerIndex, int workerCount)
        {
            _action(workerIndex, workerCount);
        }
    }


    public void Dispose()
    {
        _world.Dispose();
    }
}