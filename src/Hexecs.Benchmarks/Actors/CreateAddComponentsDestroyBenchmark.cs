using Hexecs.Benchmarks.Mocks;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//    .NET SDK 10.0.100
//    [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//    .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method                      | Count  | Mean         | Ratio | Gen0     | Allocated  | Alloc Ratio |
//    |---------------------------- |------- |-------------:|------:|---------:|-----------:|------------:|
//    | DefaultEcs_CreateAddDestroy | 1000   |     411.8 us |  0.72 |   1.4648 |    32000 B |          NA |
//    | Hexecs_CreateAddDestroy     | 1000   |     574.8 us |  1.00 |        - |          - |          NA |
//    |                             |        |              |       |          |            |             |
//    | Hexecs_CreateAddDestroy     | 100000 |  76,251.7 us |  1.00 |        - |       40 B |        1.00 |
//    | DefaultEcs_CreateAddDestroy | 100000 |  90,755.1 us |  1.19 | 166.6667 |  3200040 B |   80,001.00 |
//    |                             |        |              |       |          |            |             |
//    | DefaultEcs_CreateAddDestroy | 500000 | 470,864.8 us |  0.86 |        - | 16000040 B |  400,001.00 |
//    | Hexecs_CreateAddDestroy     | 500000 | 548,102.0 us |  1.00 |        - |       40 B |        1.00 |
//
// ------------------------------------------------------------------------------------
// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//     .NET SDK 10.0.101
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//     .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method                      | Count  | Mean         | Ratio | Gen0      | Gen1     | Allocated  | Alloc Ratio |
//     |---------------------------- |------- |-------------:|------:|----------:|---------:|-----------:|------------:|
//     | Hexecs_CreateAddDestroy     | 1000   |     176.0 us |  1.00 |         - |        - |          - |          NA |
//     | DefaultEcs_CreateAddDestroy | 1000   |     207.3 us |  1.18 |    3.6621 |        - |    32000 B |          NA |
//     |                             |        |              |       |           |          |            |             |
//     | Hexecs_CreateAddDestroy     | 100000 |  19,069.0 us |  1.00 |         - |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 100000 |  22,847.4 us |  1.20 |  375.0000 | 156.2500 |  3200040 B |   80,001.00 |
//     |                             |        |              |       |           |          |            |             |
//     | Hexecs_CreateAddDestroy     | 500000 | 113,318.9 us |  1.00 |         - |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 500000 | 121,507.3 us |  1.07 | 1800.0000 | 800.0000 | 16000040 B |  400,001.00 |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class CreateAddComponentsDestroyBenchmark
{
    [Params(1_000, 100_000, 500_000)] public int Count;

    private List<DefaultEcs.EntitySet> _defaultSets = null!;
    private List<DefaultEcs.Entity> _defaultEntities = null!;
    private DefaultEcs.World _defaultWorld = null!;

    private List<Actor> _hexecsActors = null!;
    private ActorContext _hexecsContext = null!;
    private List<IActorFilter> _hexecsFilters = null!;
    private World _hexecsWorld = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs_CreateAddDestroy()
    {
        _hexecsActors.Clear();

        for (var i = 0; i < Count; i++)
        {
            var actor = _hexecsContext.CreateActor();
            actor.Add(new Attack { Value = i });
            actor.Add(new Defence());
            actor.Add(new Speed());

            _hexecsActors.Add(actor);
        }

        foreach (var actor in _hexecsActors)
        {
            actor.Remove<Attack>();
            actor.Remove<Defence>();
            actor.Remove<Speed>();

            actor.Destroy();
        }

        return _hexecsFilters.Sum(static x => x.Length);
    }

    [Benchmark]
    public int DefaultEcs_CreateAddDestroy()
    {
        _defaultEntities.Clear();

        for (var i = 0; i < Count; i++)
        {
            var entity = _defaultWorld.CreateEntity();
            entity.Set(new Attack { Value = i });
            entity.Set(new Defence());
            entity.Set(new Speed());

            _defaultEntities.Add(entity);
        }

        foreach (var entity in _defaultEntities)
        {
            entity.Remove<Attack>();
            entity.Remove<Defence>();
            entity.Remove<Speed>();

            entity.Dispose();
        }

        return _defaultSets.Sum(static x => x.Count);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultWorld.Dispose();
        _defaultWorld = null!;

        _hexecsWorld.Dispose();
        _hexecsWorld = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _defaultEntities = new List<DefaultEcs.Entity>(Count);
        _defaultWorld = new DefaultEcs.World();
        _defaultSets =
        [
            _defaultWorld.GetEntities().With<Attack>().AsSet(),
            _defaultWorld.GetEntities().With<Defence>().AsSet(),
            _defaultWorld.GetEntities().With<Speed>().AsSet(),
            _defaultWorld.GetEntities().With<Attack>().With<Defence>().AsSet(),
            _defaultWorld.GetEntities().With<Attack>().With<Speed>().AsSet(),
            _defaultWorld.GetEntities().With<Defence>().With<Speed>().AsSet(),
            _defaultWorld.GetEntities().With<Attack>().With<Defence>().With<Speed>().AsSet()
        ];

        _hexecsActors = new List<Actor>(Count);
        _hexecsWorld = new WorldBuilder().Build();
        _hexecsContext = _hexecsWorld.Actors;
        _hexecsFilters =
        [
            _hexecsContext.Filter<Attack>(),
            _hexecsContext.Filter<Defence>(),
            _hexecsContext.Filter<Speed>(),
            _hexecsContext.Filter<Attack, Defence>(),
            _hexecsContext.Filter<Attack, Speed>(),
            _hexecsContext.Filter<Defence, Speed>(),
            _hexecsContext.Filter<Attack, Defence, Speed>()
            // warmup
        ];

        // warmup
        for (var i = 0; i < Count; i++)
        {
            var actor = _hexecsContext.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());
            actor.Add(new Speed());

            _hexecsActors.Add(actor);

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _defaultEntities.Add(defaultEntity);
        }

        foreach (var actor in _hexecsActors)
        {
            actor.Destroy();
        }

        foreach (var entity in _defaultEntities)
        {
            entity.Dispose();
        }
    }
}