using DefaultEcs;

using Friflo.Engine.ECS;

using Hexecs.Benchmarks.Mocks.ActorComponents;

using Hexecsm.Accessors;

using ActorId = Hexecsm.ActorId;
using Entity = DefaultEcs.Entity;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0
//
//     | Method                   | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |------------------------- |------- |----------:|------:|----------:|------------:|
//     | FriFlo_Chunks            | 10000  |  16.10 us |  0.47 |         - |          NA |
//     | FriFlo                   | 10000  |  26.42 us |  0.77 |      88 B |          NA |
//     | DefaultEcs               | 10000  |  28.82 us |  0.84 |         - |          NA |
//     | Hexecs_M_ComponentAccess | 10000  |  34.12 us |  1.00 |         - |          NA |
//     | Hexecs_M                 | 10000  |  35.36 us |  1.04 |         - |          NA |
//     |                          |        |           |       |           |             |
//     | FriFlo_Chunks            | 100000 | 162.70 us |  0.60 |         - |          NA |
//     | DefaultEcs               | 100000 | 251.57 us |  0.93 |         - |          NA |
//     | FriFlo                   | 100000 | 270.36 us |  1.00 |      88 B |          NA |
//     | Hexecs_M_ComponentAccess | 100000 | 271.21 us |  1.00 |         - |          NA |
//     | Hexecs_M                 | 100000 | 323.19 us |  1.19 |         - |          NA |
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

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcs.EntitySet _defaultEntitySet = null!;

    private EntityStore _frifloWorld = null!;
    private ArchetypeQuery<Attack, Defence, Speed> _frifloQuery = null!;

    private Hexecsm.Worlds.World _mWorld = null!;
    private Hexecsm.Filters.Filter<Attack, Defence, Speed> _mFilter = null!;

    [Benchmark]
    public int Hexecs_M()
    {
        var result = 0;

        foreach (Hexecsm.ActorRef<Attack, Defence, Speed> actor in _mFilter)
        {
            result +=
                actor.Component1.Value +
                actor.Component2.Value +
                actor.Component3.Value;
        }

        return result;
    }

    [Benchmark(Baseline = true)]
    public int Hexecs_M_ComponentAccess()
    {
        var result = 0;

        ReadOnlySpan<ActorId> keys = _mFilter.Keys.AsReadOnlySpan();
        ValueAccessor<Attack> attacks = _mWorld.GetComponents<Attack>();
        ValueAccessor<Defence> defences = _mWorld.GetComponents<Defence>();
        ValueAccessor<Speed> speeds = _mWorld.GetComponents<Speed>();

        foreach (ActorId actorId in keys)
        {
            result +=
                attacks.GetValue(actorId).Value +
                defences.GetValue(actorId).Value +
                speeds.GetValue(actorId).Value;
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
            result +=
                attacks[entity].Value +
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
            result +=
                attack.Value +
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
                result +=
                    attacks[i].Value +
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

        _mWorld.Dispose();
        _mWorld = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _defaultWorld = new DefaultEcs.World();
        _frifloWorld = new EntityStore();
        _mWorld = new Hexecsm.Worlds.World(128, 4);

        _defaultEntitySet = _defaultWorld.GetEntities().With<Attack>().With<Defence>().With<Speed>().AsSet();
        _frifloQuery = _frifloWorld.Query<Attack, Defence, Speed>();
        _mFilter = _mWorld.GetFilter<Attack, Defence, Speed>();

        for (var i = 0; i < Count; i++)
        {
            var attack = new Attack { Value = i };

            Entity defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set(in attack);
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _frifloWorld.CreateEntity(attack, new Defence(), new Speed());

            ActorId actorId = _mWorld.CreateActor();
            _mWorld.AddComponent(actorId, in attack);
            _mWorld.AddComponent(actorId, new Defence());
            _mWorld.AddComponent(actorId, new Speed());
            _mWorld.Update();
        }
    }
}
