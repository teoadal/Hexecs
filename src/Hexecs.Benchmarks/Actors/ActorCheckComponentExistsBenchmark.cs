using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;
using World = Hexecs.Worlds.World;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.100
//     [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method           | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------- |----------:|------:|----------:|------------:|
//     | Hexecs_Is        | 10000  |  20.42 us |  0.96 |         - |          NA |
//     | Hexecs_Has       | 10000  |  21.19 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 10000  |  24.30 us |  1.15 |         - |          NA |
//     | FriFlo_Has       | 10000  |  40.28 us |  1.90 |         - |          NA |
//     | DefaultEcs_Has   | 10000  |  73.24 us |  3.46 |         - |          NA |
//     |                  |        |           |       |           |             |
//     | Hexecs_Is        | 100000 | 204.98 us |  0.94 |         - |          NA |
//     | Hexecs_Has       | 100000 | 219.12 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 100000 | 251.83 us |  1.15 |         - |          NA |
//     | FriFlo_Has       | 100000 | 409.48 us |  1.87 |         - |          NA |
//     | DefaultEcs_Has   | 100000 | 712.00 us |  3.25 |         - |          NA |
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
//     | Method           | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------- |----------:|------:|----------:|------------:|
//     | Hexecs_Is        | 10000  |  12.17 us |  0.94 |         - |          NA |
//     | Hexecs_Has       | 10000  |  12.97 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 10000  |  14.71 us |  1.13 |         - |          NA |
//     | FriFlo_Has       | 10000  |  16.38 us |  1.26 |         - |          NA |
//     | DefaultEcs_Has   | 10000  |  25.72 us |  1.98 |         - |          NA |
//     |                  |        |           |       |           |             |
//     | Hexecs_Is        | 100000 | 126.14 us |  0.94 |         - |          NA |
//     | Hexecs_Has       | 100000 | 134.68 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 100000 | 153.34 us |  1.14 |         - |          NA |
//     | FriFlo_Has       | 100000 | 159.75 us |  1.19 |         - |          NA |
//     | DefaultEcs_Has   | 100000 | 254.83 us |  1.89 |         - |          NA |


[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorCheckComponentExistsBenchmark
{
    [Params(10_000, 100_000)] public int Count;

    private ActorContext _context = null!;
    private DefaultEcs.World _defaultWorld = null!;
    private EntityStore _frifloWorld = null!;
    private ArchetypeQuery _frifloAllEntitiesQuery = null!;
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
    public int FriFlo_Has()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var entity in _frifloAllEntitiesQuery.Entities)
        {
            if (entity.HasComponent<Speed>())
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

        _frifloWorld = null!;

        _world.Dispose();
        _world = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _defaultWorld = new DefaultEcs.World();
        _frifloWorld = new EntityStore();
        _frifloAllEntitiesQuery = _frifloWorld.Query();
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

            var frifloEntity = _frifloWorld.CreateEntity(new Attack(), new Defence());

            if (i % 10 != 0) continue;

            actor.Add(new Speed());

            defaultEntity.Set<Speed>();
            frifloEntity.Add(new Speed());
        }
    }
}