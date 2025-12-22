using System.Collections.Concurrent;
using Hexecs.Collections;

namespace Hexecs.Benchmarks.Collections;

// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//    .NET SDK 10.0.101
//    [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//    .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method                    | Mean     | Ratio | Gen0     | Allocated | Alloc Ratio |
//    |-------------------------- |---------:|------:|---------:|----------:|------------:|
//    | ThreadLocalStack_Parallel | 1.395 ms |  1.01 |        - |   2.58 KB |        1.00 |
//    | ConcurrentStack_Parallel  | 8.849 ms |  6.39 | 375.0000 | 3127.6 KB |    1,212.67 |
//    | LockedStack_Parallel      | 9.937 ms |  7.18 |        - |   2.59 KB |        1.00 |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
public class ThreadLocalStackBenchmark
{
    [Params(10_000)] 
    public int Count = 10_000;

    private const int ThreadCount = 4;

    private ConcurrentStack<uint> _concurrentStack = null!;
    private Stack<uint> _lockedStack = null!;
    private ThreadLocalStack<uint> _threadLocalStack = null!;
    private readonly Lock _stackLock = new();

    [Benchmark(Baseline = true)]
    public int ThreadLocalStack_Parallel()
    {
        var opsPerThread = Count / ThreadCount;
        var result = 0;
        Parallel.For(0, ThreadCount, _ =>
        {
            for (var i = 0; i < opsPerThread; i++)
            {
                if (_threadLocalStack.TryPop(out var id))
                {
                    Interlocked.Increment(ref result);
                    _threadLocalStack.Push(id);
                }
            }
        });
        return result;
    }

    [Benchmark]
    public int ConcurrentStack_Parallel()
    {
        var opsPerThread = Count / ThreadCount;
        var result = 0;
        Parallel.For(0, ThreadCount, _ =>
        {
            for (var i = 0; i < opsPerThread; i++)
            {
                if (_concurrentStack.TryPop(out var id))
                {
                    Interlocked.Increment(ref result);
                    _concurrentStack.Push(id);
                }
            }
        });
        return result;
    }

    [Benchmark]
    public int LockedStack_Parallel()
    {
        var opsPerThread = Count / ThreadCount;
        var result = 0;
        Parallel.For(0, ThreadCount, _ =>
        {
            for (var i = 0; i < opsPerThread; i++)
            {
                if (TryPopLocked(out var id))
                {
                    Interlocked.Increment(ref result);
                    PushLocked(id);
                }
            }
        });
        return result;
    }

    [GlobalSetup]
    public void Setup()
    {
        _concurrentStack = new ConcurrentStack<uint>();
        _lockedStack = new Stack<uint>(Count);
        _threadLocalStack = new ThreadLocalStack<uint>(Count);

        // Наполняем заново для чистоты итерации
        for (uint i = 0; i < Count; i++)
        {
            _concurrentStack.Push(i);
            _lockedStack.Push(i);
            _threadLocalStack.Push(i);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _concurrentStack.Clear();
        _lockedStack.Clear();
        _threadLocalStack.Dispose();
    }

    private bool TryPopLocked(out uint id)
    {
        using var locker = _stackLock.EnterScope();
        return _lockedStack.TryPop(out id);
    }

    private void PushLocked(uint id)
    {
        using var locker = _stackLock.EnterScope();
        _lockedStack.Push(id);
    }
}