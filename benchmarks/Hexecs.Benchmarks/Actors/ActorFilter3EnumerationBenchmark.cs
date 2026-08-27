using DefaultEcs;

using Friflo.Engine.ECS;

using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Utils;
using Hexecs.Worlds;

using Entity = DefaultEcs.Entity;
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
//     | Method                 | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------------- |------- |----------:|------:|----------:|------------:|
//     | FriFlo_Chunks          | 10000  |  16.10 us |  0.55 |         - |          NA |
//     | Hexecs_ComponentAccess | 10000  |  25.10 us |  0.86 |         - |          NA |
//     | FriFlo                 | 10000  |  25.66 us |  0.87 |      88 B |          NA |
//     | Hexecs                 | 10000  |  29.33 us |  1.00 |         - |          NA |
//     | DefaultEcs             | 10000  |  29.34 us |  1.00 |         - |          NA |
//     |                        |        |           |       |           |             |
//     | FriFlo_Chunks          | 100000 | 158.62 us |  0.50 |         - |          NA |
//     | FriFlo                 | 100000 | 253.14 us |  0.79 |      88 B |          NA |
//     | Hexecs_ComponentAccess | 100000 | 285.76 us |  0.89 |         - |          NA |
//     | DefaultEcs             | 100000 | 287.17 us |  0.90 |         - |          NA |
//     | Hexecs                 | 100000 | 320.26 us |  1.00 |         - |          NA |
//
// ------------------------------------------------------------------------------------
//
// BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0
//
//     | Method                 | Count  | Mean       | Ratio | Allocated | Alloc Ratio |
//     |----------------------- |------- |-----------:|------:|----------:|------------:|
//     | Hexecs_ComponentAccess | 10000  |   8.256 us |  0.81 |         - |          NA |
//     | FriFlo                 | 10000  |   8.398 us |  0.82 |      88 B |          NA |
//     | FriFlo_Chunks          | 10000  |   9.560 us |  0.93 |         - |          NA |
//     | Hexecs                 | 10000  |  10.240 us |  1.00 |         - |          NA |
//     | DefaultEcs             | 10000  |  13.360 us |  1.30 |         - |          NA |
//     |                        |        |            |       |           |             |
//     | FriFlo                 | 100000 |  83.226 us |  0.79 |      88 B |          NA |
//     | Hexecs_ComponentAccess | 100000 |  84.102 us |  0.80 |         - |          NA |
//     | FriFlo_Chunks          | 100000 |  95.161 us |  0.91 |         - |          NA |
//     | Hexecs                 | 100000 | 104.746 us |  1.00 |         - |          NA |
//     | DefaultEcs             | 100000 | 162.182 us |  1.55 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn]
[MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorFilter3EnumerationBenchmark
{
    [Params(10_000, 100_000)]
    public int Count;

    private ActorContext _context = null!;
    private ActorFilter<Attack, Defence, Speed> _filter = null!;
    private World _world = null!;

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcs.EntitySet _defaultEntitySet = null!;

    private EntityStore _frifloWorld = null!;
    private ArchetypeQuery<Attack, Defence, Speed> _frifloQuery = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs()
    {
        var result = 0;

        foreach (ActorRef<Attack, Defence, Speed> actor in _filter)
        {
            result += actor.Component1.Value +
                actor.Component2.Value +
                actor.Component3.Value;
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_ComponentAccess()
    {
        var result = 0;

        ComponentsAccess<Attack> attacks = _context.GetComponents<Attack>();
        ComponentsAccess<Defence> defences = _context.GetComponents<Defence>();
        ComponentsAccess<Speed> speeds = _context.GetComponents<Speed>();

        foreach (uint actorId in _filter.Keys)
        {
            result += attacks[actorId].Value +
                defences[actorId].Value +
                speeds[actorId].Value;
        }

        return result;
    }

    [Benchmark]
    public int DefaultEcs()
    {
        Components<Attack> attacks = _defaultWorld.GetComponents<Attack>();
        Components<Defence> defences = _defaultWorld.GetComponents<Defence>();
        Components<Speed> speeds = _defaultWorld.GetComponents<Speed>();

        var result = 0;

        foreach (Entity entity in _defaultEntitySet.GetEntities())
        {
            result += attacks[entity].Value +
                defences[entity].Value +
                speeds[entity].Value;
        }

        return result;
    }

    [Benchmark]
    public int FriFlo()
    {
        var result = 0;

        _frifloQuery.ForEachEntity((ref attack, ref defence, ref speed, _) =>
        {
            result += attack.Value +
                defence.Value +
                speed.Value;
        });

        return result;
    }

    [Benchmark]
    public int FriFlo_Chunks()
    {
        var result = 0;

        foreach (Chunks<Attack, Defence, Speed> queryChunk in _frifloQuery.Chunks)
        {
            Chunk<Attack> attacks = queryChunk.Chunk1;
            Chunk<Defence> defences = queryChunk.Chunk2;
            Chunk<Speed> speeds = queryChunk.Chunk3;

            for (var i = 0; i < queryChunk.Length; i++)
            {
                result += attacks[i].Value +
                    defences[i].Value +
                    speeds[i].Value;
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
        _frifloWorld = new EntityStore();
        _world = new WorldBuilder().Build();
        _context = _world.Actors;

        _defaultEntitySet = _defaultWorld.GetEntities().With<Attack>().With<Defence>().With<Speed>().AsSet();
        _filter = _world.Actors.Filter<Attack, Defence, Speed>();
        _frifloQuery = _frifloWorld.Query<Attack, Defence, Speed>();

        ActorContext context = _world.Actors;

        for (var i = 0; i < Count; i++)
        {
            var attack = new Attack { Value = i };

            Actor actor = context.CreateActor();
            actor.Add(in attack);
            actor.Add(new Defence());
            actor.Add(new Speed());

            Entity defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set(in attack);
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _frifloWorld.CreateEntity(attack, new Defence(), new Speed());
        }
    }
}
