using Friflo.Engine.ECS;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;
using World = Hexecs.Worlds.World;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.102
//     [Host]    : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method           | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------- |----------:|------:|----------:|------------:|
//     | Hexecs_Is        | 10000  |  14.55 us |  0.88 |         - |          NA |
//     | Hexecs_Reference | 10000  |  15.92 us |  0.96 |         - |          NA |
//     | Hexecs_Has       | 10000  |  16.58 us |  1.00 |         - |          NA |
//     | FriFlo_Has       | 10000  |  40.36 us |  2.43 |         - |          NA |
//     | DefaultEcs_Has   | 10000  |  72.01 us |  4.34 |         - |          NA |
//     |                  |        |           |       |           |             |
//     | Hexecs_Is        | 100000 | 149.57 us |  0.93 |         - |          NA |
//     | Hexecs_Has       | 100000 | 161.46 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 100000 | 163.02 us |  1.01 |         - |          NA |
//     | FriFlo_Has       | 100000 | 409.32 us |  2.54 |         - |          NA |
//     | DefaultEcs_Has   | 100000 | 730.64 us |  4.53 |         - |          NA |
//
// ------------------------------------------------------------------------------------
//
// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//     .NET SDK 10.0.102
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//     .NET 10.0 : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method           | Count  | Mean       | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------- |-----------:|------:|----------:|------------:|
//     | Hexecs_Is        | 10000  |   9.916 us |  0.90 |         - |          NA |
//     | Hexecs_Has       | 10000  |  10.960 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 10000  |  11.301 us |  1.03 |         - |          NA |
//     | FriFlo_Has       | 10000  |  16.540 us |  1.51 |         - |          NA |
//     | DefaultEcs_Has   | 10000  |  25.877 us |  2.36 |         - |          NA |
//     |                  |        |            |       |           |             |
//     | Hexecs_Is        | 100000 |  98.966 us |  0.90 |         - |          NA |
//     | Hexecs_Has       | 100000 | 110.039 us |  1.00 |         - |          NA |
//     | Hexecs_Reference | 100000 | 112.981 us |  1.03 |         - |          NA |
//     | FriFlo_Has       | 100000 | 159.346 us |  1.45 |         - |          NA |
//     | DefaultEcs_Has   | 100000 | 256.135 us |  2.33 |         - |          NA |

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