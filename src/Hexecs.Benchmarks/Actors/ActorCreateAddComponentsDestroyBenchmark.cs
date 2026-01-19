using DefaultEcs;
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
//     | Method                      | Count  | Mean         | Ratio | Gen0     | Allocated  | Alloc Ratio |
//     |---------------------------- |------- |-------------:|------:|---------:|-----------:|------------:|
//     | FriFlo_CreateAddDestroy     | 1000   |     157.5 us |  0.32 |        - |          - |          NA |
//     | DefaultEcs_CreateAddDestroy | 1000   |     400.8 us |  0.83 |   1.4648 |    32000 B |          NA |
//     | Hexecs_CreateAddDestroy     | 1000   |     485.5 us |  1.00 |        - |          - |          NA |
//     |                             |        |              |       |          |            |             |
//     | FriFlo_CreateAddDestroy     | 100000 |  16,503.2 us |  0.26 |        - |       40 B |        1.00 |
//     | Hexecs_CreateAddDestroy     | 100000 |  62,708.1 us |  1.00 |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 100000 |  89,760.2 us |  1.43 | 166.6667 |  3200040 B |   80,001.00 |
//     |                             |        |              |       |          |            |             |
//     | FriFlo_CreateAddDestroy     | 500000 |  84,276.5 us |  0.15 |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 500000 | 457,366.0 us |  0.83 |        - | 16000040 B |  400,001.00 |
//     | Hexecs_CreateAddDestroy     | 500000 | 551,567.4 us |  1.00 |        - |       40 B |        1.00 |
//
// ---------------------------------------------------------------------------------------------------------
//

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorCreateAddComponentsDestroyBenchmark
{
    [Params(1_000, 100_000, 500_000)] public int Count;

    private List<DefaultEcs.Entity> _defaultEntities = null!;
    private List<EntitySet> _defaultSets = null!;
    private DefaultEcs.World _defaultWorld = null!;

    private List<Friflo.Engine.ECS.Entity> _frifloEntities = null!;
    private List<ArchetypeQuery> _frifloQueries = null!;
    private EntityStore _frifloWorld = null!;

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

    [Benchmark]
    public int FriFlo_CreateAddDestroy()
    {
        _frifloEntities.Clear();

        for (var i = 0; i < Count; i++)
        {
            var entity = _frifloWorld.CreateEntity();
            entity.AddComponent(new Attack { Value = i });
            entity.AddComponent(new Defence());
            entity.AddComponent(new Speed());

            _frifloEntities.Add(entity);
        }

        foreach (var entity in _frifloEntities)
        {
            entity.RemoveComponent<Attack>();
            entity.RemoveComponent<Defence>();
            entity.RemoveComponent<Speed>();

            entity.DeleteEntity();
        }

        return _frifloQueries.Sum(static x => x.Count);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultWorld.Dispose();
        _defaultWorld = null!;

        _frifloWorld = null!;

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

        _frifloEntities = new List<Friflo.Engine.ECS.Entity>(Count);
        _frifloWorld = new EntityStore();
        _frifloQueries =
        [
            _frifloWorld.Query<Attack>(),
            _frifloWorld.Query<Defence>(),
            _frifloWorld.Query<Speed>(),
            _frifloWorld.Query<Attack, Defence>(),
            _frifloWorld.Query<Attack, Speed>(),
            _frifloWorld.Query<Defence, Speed>(),
            _frifloWorld.Query<Attack, Defence, Speed>()
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
        ];

        // warmup
        for (var i = 0; i < Count; i++)
        {
            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _defaultEntities.Add(defaultEntity);

            var frifloEntity = _frifloWorld.CreateEntity(new Attack(), new Defence(), new Speed());
            _frifloEntities.Add(frifloEntity);

            var actor = _hexecsContext.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());
            actor.Add(new Speed());

            _hexecsActors.Add(actor);
        }

        foreach (var entity in _defaultEntities)
        {
            entity.Dispose();
        }

        foreach (var entity in _frifloEntities)
        {
            entity.DeleteEntity();
        }

        foreach (var actor in _hexecsActors)
        {
            actor.Destroy();
        }
        
        _defaultEntities.Clear();
        _frifloEntities.Clear();
        _hexecsActors.Clear();
    }
}