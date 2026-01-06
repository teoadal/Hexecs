using System.Runtime.CompilerServices;
using Hexecs.Benchmarks.Mocks;
using Hexecs.Worlds;
using World = Hexecs.Worlds.World;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//    .NET SDK 10.0.100
//    [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//    .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method           | Mean     | Ratio | Allocated | Alloc Ratio |
//    |----------------- |---------:|------:|----------:|------------:|
//    | Hexecs_Is        | 307.7 us |  0.90 |         - |          NA |
//    | Hexecs_Has       | 342.4 us |  1.00 |         - |          NA |
//    | Hexecs_Reference | 380.7 us |  1.11 |         - |          NA |
//    | DefaultEcs_Has   | 713.7 us |  2.08 |         - |          NA |
//
// ------------------------------------------------------------------------------------
//
// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//     .NET SDK 10.0.101
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//     .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method           | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------- |----------:|------:|----------:|------------:|
//     | Hexecs_Is        |  12.76 us |  0.93 |         - |          NA |
//     | Hexecs_Has       |  13.79 us |  1.00 |         - |          NA |
//     | Hexecs_Reference |  15.44 us |  1.12 |         - |          NA |
//     | DefaultEcs_Has   |  25.32 us |  1.84 |         - |          NA |
//     |                  |           |       |           |             |
//     | Hexecs_Is        | 127.64 us |  0.92 |         - |          NA |
//     | Hexecs_Has       | 139.17 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 155.12 us |  1.11 |         - |          NA |
//     | DefaultEcs_Has   | 255.36 us |  1.83 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class CheckComponentExistsBenchmark
{
    [Params(10_000, 100_000)] public int Count;

    private ActorContext _context = null!;
    private DefaultEcs.World _defaultWorld = null!;
    private World _world = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs_Has()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            if (actor.Has<Speed>()) result++;
        }

        return result;
    }

    [Benchmark]
    public int DefaultEcs_Has()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var entity in _defaultWorld)
        {
            if (entity.Has<Speed>())
            {
                result++;
            }
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_Is()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            if (actor.Is<Speed>(out _))
            {
                result++;
            }
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_Reference()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            ref var reference = ref actor.TryGetRef<Speed>();
            if (!Unsafe.IsNullRef(ref reference))
            {
                result++;
            }
        }

        return result;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultWorld.Dispose();
        _defaultWorld = null!;

        _world.Dispose();
        _world = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _defaultWorld = new DefaultEcs.World();
        _world = new WorldBuilder().Build();
        _context = _world.Actors;

        for (var i = 0; i < Count; i++)
        {
            var actor = _context.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();

            if (i % 10 != 0) continue;

            actor.Add(new Speed());
            defaultEntity.Set<Speed>();
        }
    }
}