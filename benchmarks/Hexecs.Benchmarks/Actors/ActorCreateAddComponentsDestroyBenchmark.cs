using DefaultEcs;

using Friflo.Engine.ECS;

using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;

using Hexecsm.Filters;

using ActorId = Hexecsm.ActorId;
using Entity = DefaultEcs.Entity;
using World = Hexecs.Worlds.World;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0
//
//     | Method                      | Count  | Mean         | Ratio | Gen0     | Allocated  | Alloc Ratio |
//     |---------------------------- |------- |-------------:|------:|---------:|-----------:|------------:|
//     | FriFlo_CreateAddDestroy     | 1000   |     160.3 us |  0.31 |        - |          - |          NA |
//     | DefaultEcs_CreateAddDestroy | 1000   |     401.6 us |  0.77 |   1.4648 |    32000 B |          NA |
//     | Hexecs_CreateAddDestroy     | 1000   |     521.4 us |  1.00 |        - |          - |          NA |
//     |                             |        |              |       |          |            |             |
//     | FriFlo_CreateAddDestroy     | 100000 |  16,367.5 us |  0.25 |        - |       40 B |        1.00 |
//     | Hexecs_CreateAddDestroy     | 100000 |  64,667.0 us |  1.00 |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 100000 |  88,922.5 us |  1.38 | 166.6667 |  3200040 B |   80,001.00 |
//     |                             |        |              |       |          |            |             |
//     | FriFlo_CreateAddDestroy     | 500000 |  84,863.0 us |  0.16 |        - |       40 B |        1.00 |
//     | Hexecs_CreateAddDestroy     | 500000 | 545,966.3 us |  1.00 |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 500000 | 676,605.1 us |  1.24 |        - | 16000040 B |  400,001.00 |
//
// ---------------------------------------------------------------------------------------------------------
//
// BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0
//
//     | Method                      | Count  | Mean          | Ratio | Gen0      | Gen1     | Allocated  | Alloc Ratio |
//     |---------------------------- |------- |--------------:|------:|----------:|---------:|-----------:|------------:|
//     | FriFlo_CreateAddDestroy     | 1000   |      64.36 us |  0.40 |         - |        - |          - |          NA |
//     | Hexecs_CreateAddDestroy     | 1000   |     162.46 us |  1.00 |         - |        - |          - |          NA |
//     | DefaultEcs_CreateAddDestroy | 1000   |     207.01 us |  1.27 |    3.6621 |        - |    32000 B |          NA |
//     |                             |        |               |       |           |          |            |             |
//     | FriFlo_CreateAddDestroy     | 100000 |   7,036.00 us |  0.40 |         - |        - |       40 B |        1.00 |
//     | Hexecs_CreateAddDestroy     | 100000 |  17,473.30 us |  1.00 |         - |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 100000 |  22,755.83 us |  1.30 |  375.0000 | 156.2500 |  3200040 B |   80,001.00 |
//     |                             |        |               |       |           |          |            |             |
//     | FriFlo_CreateAddDestroy     | 500000 |  35,389.70 us |  0.37 |         - |        - |       40 B |        1.00 |
//     | Hexecs_CreateAddDestroy     | 500000 |  95,996.62 us |  1.00 |         - |        - |       40 B |        1.00 |
//     | DefaultEcs_CreateAddDestroy | 500000 | 121,362.58 us |  1.26 | 1800.0000 | 800.0000 | 16000040 B |  400,001.00 |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn]
[MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorCreateAddComponentsDestroyBenchmark
{
    [Params(1_000, 100_000, 500_000)]
    public int Count;

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

    private List<ActorId> _mHexecsActors = null!;
    private Hexecsm.Worlds.World _mWorld = null!;
    private List<IFilter> _mFilters = null!;

    // [Benchmark(Baseline = true)]
    // public int Hexecs_CreateAddDestroy()
    // {
    //     _hexecsActors.Clear();
    //
    //     for (var i = 0; i < Count; i++)
    //     {
    //         Actor actor = _hexecsContext.CreateActor();
    //         actor.Add(new Attack { Value = i });
    //         actor.Add(new Defence());
    //         actor.Add(new Speed());
    //
    //         _hexecsActors.Add(actor);
    //     }
    //
    //     foreach (Actor actor in _hexecsActors)
    //     {
    //         actor.Remove<Attack>();
    //         actor.Remove<Defence>();
    //         actor.Remove<Speed>();
    //
    //         actor.Destroy();
    //     }
    //
    //     return _hexecsFilters.Sum(static x => x.Length);
    // }

    [Benchmark(Baseline = true)]
    public int Hexes_M_CreateAddDestroy()
    {
        _mHexecsActors.Clear();

        for (var i = 0; i < Count; i++)
        {
            ActorId actor = _mWorld.CreateActor();
            _mWorld.AddComponent(actor, new Attack { Value = i });
            _mWorld.AddComponent(actor, new Defence());
            _mWorld.AddComponent(actor, new Speed());

            _mHexecsActors.Add(actor);
        }

        _mWorld.Update();

        foreach (ActorId actor in _mHexecsActors)
        {
            _mWorld.RemoveComponent<Attack>(actor);
            _mWorld.RemoveComponent<Defence>(actor);
            _mWorld.RemoveComponent<Speed>(actor);

            _mWorld.DestroyActor(actor);
        }

        _mWorld.Update();

        return _mFilters.Sum(static x => x.Length);
    }

    // [Benchmark]
    // public int DefaultEcs_CreateAddDestroy()
    // {
    //     _defaultEntities.Clear();
    //
    //     for (var i = 0; i < Count; i++)
    //     {
    //         Entity entity = _defaultWorld.CreateEntity();
    //         entity.Set(new Attack { Value = i });
    //         entity.Set(new Defence());
    //         entity.Set(new Speed());
    //
    //         _defaultEntities.Add(entity);
    //     }
    //
    //     foreach (Entity entity in _defaultEntities)
    //     {
    //         entity.Remove<Attack>();
    //         entity.Remove<Defence>();
    //         entity.Remove<Speed>();
    //
    //         entity.Dispose();
    //     }
    //
    //     return _defaultSets.Sum(static x => x.Count);
    // }
    //
    // [Benchmark]
    // public int FriFlo_CreateAddDestroy()
    // {
    //     _frifloEntities.Clear();
    //
    //     for (var i = 0; i < Count; i++)
    //     {
    //         Friflo.Engine.ECS.Entity entity = _frifloWorld.CreateEntity();
    //         entity.AddComponent(new Attack { Value = i });
    //         entity.AddComponent(new Defence());
    //         entity.AddComponent(new Speed());
    //
    //         _frifloEntities.Add(entity);
    //     }
    //
    //     foreach (Friflo.Engine.ECS.Entity entity in _frifloEntities)
    //     {
    //         entity.RemoveComponent<Attack>();
    //         entity.RemoveComponent<Defence>();
    //         entity.RemoveComponent<Speed>();
    //
    //         entity.DeleteEntity();
    //     }
    //
    //     return _frifloQueries.Sum(static x => x.Count);
    // }

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
        _defaultEntities = new List<Entity>(Count);
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

        _mHexecsActors = new List<ActorId>();
        _mWorld = new Hexecsm.Worlds.World(256);
        _mFilters =
        [
            _mWorld.GetFilter<Attack>(),
            _mWorld.GetFilter<Defence>(),
            _mWorld.GetFilter<Speed>(),
            _mWorld.GetFilter<Attack, Defence>(),
            _mWorld.GetFilter<Attack, Speed>(),
            _mWorld.GetFilter<Defence, Speed>(),
            _mWorld.GetFilter<Attack, Defence, Speed>()
        ];

        // warmup
        for (var i = 0; i < Count; i++)
        {
            Entity defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _defaultEntities.Add(defaultEntity);

            Friflo.Engine.ECS.Entity frifloEntity = _frifloWorld.CreateEntity(new Attack(), new Defence(), new Speed());
            _frifloEntities.Add(frifloEntity);

            Actor actor = _hexecsContext.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());
            actor.Add(new Speed());

            _hexecsActors.Add(actor);

            ActorId actorId = _mWorld.CreateActor();
            _mWorld.AddComponent(actorId, new Attack());
            _mWorld.AddComponent(actorId, new Defence());
            _mWorld.AddComponent(actorId, new Speed());
        }

        foreach (Entity entity in _defaultEntities)
        {
            entity.Dispose();
        }

        foreach (Friflo.Engine.ECS.Entity entity in _frifloEntities)
        {
            entity.DeleteEntity();
        }

        foreach (Actor actor in _hexecsActors)
        {
            actor.Destroy();
        }

        foreach (ActorId actorId in _mHexecsActors)
        {
            _mWorld.DestroyActor(actorId);
        }

        _defaultEntities.Clear();
        _frifloEntities.Clear();
        _hexecsActors.Clear();
    }
}
