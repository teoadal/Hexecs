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
    private ConcurrentQueue<int> _queue = null!;
    private ThreadLocalQueue<int> _localQueue = null!;

    [Benchmark(Baseline = true)]
    public int ConcurrentQueue()
    {
        for (var i = 0; i < 64; i++)
        {
            _queue.Enqueue(i);
        }

        var sum = 0;

        for (var i = 0; i < 64; i++)
        {
            _queue.TryDequeue(out int value);
            sum += value;
        }

        return sum;
    }

    [Benchmark]
    public int ThreadLocalQueue()
    {
        for (var i = 0; i < 64; i++)
        {
            _localQueue.Enqueue(i);
        }

        var sum = 0;

        foreach (ThreadLocalQueue<int>.LocalQueue batch in _localQueue.GetBatches())
        {
            foreach (int i in batch.AsSpan())
            {
                sum += i;
            }

            batch.Clear();
        }

        return sum;
    }

    [GlobalSetup]
    public void Setup()
    {
        _queue = new ConcurrentQueue<int>();
        _localQueue = new ThreadLocalQueue<int>(128);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _localQueue.Dispose();
    }
}
