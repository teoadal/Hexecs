using System.Collections.Concurrent;

using Hexecsm.Utils;

namespace Hexecs.Benchmarks.Collections;

[SimpleJob(RuntimeMoniker.Net10_0)]
[MeanColumn]
[MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Collections")]
public class QueueBenchmark
{
    private const int TotalOperations = 120_000; // Число делится без остатка на 3, 4, 6 потоков
    private const int WorkerThreadsCount = 4;
    private const int OperationsPerWorker = TotalOperations / WorkerThreadsCount;

    private MpscBlockQueue<SampleOperation> _mpscQueue = null!;
    private ConcurrentQueue<SampleOperation> _concurrentQueue = null!;
    private CountdownEvent _countdown = null!;

    // Структуры состояния для передачи в потоки без замыканий и боксинга
    private WorkerState<MpscBlockQueue<SampleOperation>>[] _mpscStates = null!;
    private WorkerState<ConcurrentQueue<SampleOperation>>[] _concurrentStates = null!;

    // Описываем callback-методы статическими, чтобы гарантировать отсутствие захвата контекста
    private static readonly WaitCallback MpscCallback = ExecuteMpscWorker;
    private static readonly WaitCallback ConcurrentCallback = ExecuteConcurrentWorker;

    [Benchmark(Baseline = true)]
    public int Benchmark_ConcurrentQueue()
    {
        _countdown.Reset(WorkerThreadsCount);

        for (var i = 0; i < WorkerThreadsCount; i++)
        {
            ThreadPool.QueueUserWorkItem(ConcurrentCallback, _concurrentStates[i]);
        }

        _countdown.Wait();

        var sum = 0;
        while (_concurrentQueue.TryDequeue(out SampleOperation op))
        {
            sum += op.EntityId + op.ComponentId;
        }

        return sum;
    }

    [Benchmark]
    public int Benchmark_MpscBlockQueue()
    {
        _mpscQueue.Clear();

        _countdown.Reset(WorkerThreadsCount);

        for (var i = 0; i < WorkerThreadsCount; i++)
        {
            ThreadPool.QueueUserWorkItem(MpscCallback, _mpscStates[i]);
        }

        _countdown.Wait();

        var sum = 0;
        foreach (ReadOnlySpan<SampleOperation> block in _mpscQueue)
        {
            foreach (ref readonly SampleOperation op in block)
            {
                sum += op.EntityId + op.ComponentId;
            }
        }

        return sum;
    }

    [GlobalSetup]
    public void Setup()
    {
        _mpscQueue = new MpscBlockQueue<SampleOperation>(1024);
        _concurrentQueue = new ConcurrentQueue<SampleOperation>();
        _countdown = new CountdownEvent(WorkerThreadsCount);

        _mpscStates = new WorkerState<MpscBlockQueue<SampleOperation>>[WorkerThreadsCount];
        _concurrentStates = new WorkerState<ConcurrentQueue<SampleOperation>>[WorkerThreadsCount];

        for (var i = 0; i < WorkerThreadsCount; i++)
        {
            _mpscStates[i] = new WorkerState<MpscBlockQueue<SampleOperation>>(_mpscQueue, _countdown, OperationsPerWorker);
            _concurrentStates[i] = new WorkerState<ConcurrentQueue<SampleOperation>>(_concurrentQueue, _countdown, OperationsPerWorker);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _countdown.Dispose();
    }

    // Класс-контейнер для состояния воркера (создается один раз в Setup)
    private sealed class WorkerState<TQueue>
    {
        public readonly TQueue Queue;
        public readonly CountdownEvent Countdown;
        public readonly int Count;

        public WorkerState(TQueue queue, CountdownEvent countdown, int count)
        {
            Queue = queue;
            Countdown = countdown;
            Count = count;
        }
    }

    // Статические методы полностью исключают выделение памяти на замыкания
    private static void ExecuteMpscWorker(object state)
    {
        var workerState = (WorkerState<MpscBlockQueue<SampleOperation>>)state;
        MpscBlockQueue<SampleOperation> queue = workerState.Queue;
        int count = workerState.Count;

        for (var i = 0; i < count; i++)
        {
            queue.Enqueue(new SampleOperation { EntityId = i, ComponentId = i });
        }

        workerState.Countdown.Signal();
    }

    private static void ExecuteConcurrentWorker(object state)
    {
        var workerState = (WorkerState<ConcurrentQueue<SampleOperation>>)state;
        ConcurrentQueue<SampleOperation> queue = workerState.Queue;
        int count = workerState.Count;

        for (var i = 0; i < count; i++)
        {
            queue.Enqueue(new SampleOperation { EntityId = i, ComponentId = i });
        }

        workerState.Countdown.Signal();
    }

    public struct SampleOperation
    {
        public int EntityId;
        public int ComponentId;
    }
}


