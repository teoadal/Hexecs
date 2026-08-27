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
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0
//
//     | Method                 | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------------- |------- |----------:|------:|----------:|------------:|
//     | FriFlo_Chunks          | 10000  |  10.87 us |  0.48 |         - |          NA |
//     | Hexecs_ComponentAccess | 10000  |  12.53 us |  0.55 |         - |          NA |
//     | DefaultEcs             | 10000  |  16.50 us |  0.72 |         - |          NA |
//     | Hexecs                 | 10000  |  22.77 us |  1.00 |         - |          NA |
//     | FriFlo                 | 10000  |  22.99 us |  1.01 |      88 B |          NA |
//     |                        |        |           |       |           |             |
//     | FriFlo_Chunks          | 100000 | 107.44 us |  0.46 |         - |          NA |
//     | Hexecs_ComponentAccess | 100000 | 125.53 us |  0.53 |         - |          NA |
//     | DefaultEcs             | 100000 | 158.61 us |  0.67 |         - |          NA |
//     | FriFlo                 | 100000 | 235.57 us |  1.00 |      88 B |          NA |
//     | Hexecs                 | 100000 | 235.74 us |  1.00 |         - |          NA |
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
//     | Method                 | Count  | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
//     |----------------------- |------- |----------:|------:|-------:|----------:|------------:|
//     | FriFlo                 | 10000  |  6.061 us |  0.71 | 0.0076 |      88 B |          NA |
//     | Hexecs_ComponentAccess | 10000  |  6.241 us |  0.73 |      - |         - |          NA |
//     | FriFlo_Chunks          | 10000  |  6.732 us |  0.79 |      - |         - |          NA |
//     | Hexecs                 | 10000  |  8.534 us |  1.00 |      - |         - |          NA |
//     | DefaultEcs             | 10000  |  9.906 us |  1.16 |      - |         - |          NA |
//     |                        |        |           |       |        |           |             |
//     | FriFlo                 | 100000 | 56.486 us |  0.77 |      - |      88 B |          NA |
//     | Hexecs_ComponentAccess | 100000 | 56.937 us |  0.77 |      - |         - |          NA |
//     | FriFlo_Chunks          | 100000 | 67.463 us |  0.92 |      - |         - |          NA |
//     | Hexecs                 | 100000 | 73.653 us |  1.00 |      - |         - |          NA |
//     | DefaultEcs             | 100000 | 93.593 us |  1.27 |      - |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn]
[MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorFilter2EnumerationBenchmark
{
    [Params(10_000, 100_000)] public int Count;

    private ActorContext _context = null!;
    private ActorFilter<Attack, Defence> _filter = null!;
    private World _world = null!;

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcs.EntitySet _defaultEntitySet = null!;

    private EntityStore _frifloWorld = null!;
    private ArchetypeQuery<Attack, Defence> _frifloQuery = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs()
    {
        var result = 0;

        foreach (ActorRef<Attack, Defence> actor in _filter)
        {
            result += actor.Component1.Value +
                      actor.Component2.Value;
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_ComponentAccess()
    {
        var result = 0;

        ComponentsAccess<Attack> attacks = _context.GetComponents<Attack>();
        ComponentsAccess<Defence> defences = _context.GetComponents<Defence>();

        foreach (uint actorId in _filter.Keys)
        {
            result += attacks[actorId].Value +
                      defences[actorId].Value;
        }

        return result;
    }

    [Benchmark]
    public int DefaultEcs()
    {
        Components<Attack> attacks = _defaultWorld.GetComponents<Attack>();
        Components<Defence> defences = _defaultWorld.GetComponents<Defence>();

        var result = 0;

        foreach (Entity entity in _defaultEntitySet.GetEntities())
        {
            result += attacks[entity].Value +
                      defences[entity].Value;
        }

        return result;
    }

    [Benchmark]
    public int FriFlo()
    {
        var result = 0;

        _frifloQuery.ForEachEntity((ref attack, ref defence, _) =>
        {
            result += attack.Value +
                      defence.Value;
        });

        return result;
    }

    [Benchmark]
    public int FriFlo_Chunks()
    {
        var result = 0;

        foreach (Chunks<Attack, Defence> queryChunk in _frifloQuery.Chunks)
        {
            Chunk<Attack> attacks = queryChunk.Chunk1;
            Chunk<Defence> defences = queryChunk.Chunk2;

            for (var i = 0; i < queryChunk.Length; i++)
            {
                result += attacks[i].Value +
                          defences[i].Value;
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

        _defaultEntitySet = _defaultWorld.GetEntities().With<Attack>().With<Defence>().AsSet();
        _filter = _world.Actors.Filter<Attack, Defence>();
        _frifloQuery = _frifloWorld.Query<Attack, Defence>();

        ActorContext context = _world.Actors;

        for (var i = 0; i < Count; i++)
        {
            var attack = new Attack { Value = i };

            Actor actor = context.CreateActor();
            actor.Add(in attack);
            actor.Add(new Defence());

            Entity defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set(in attack);
            defaultEntity.Set<Defence>();

            _frifloWorld.CreateEntity(attack, new Defence(), new Speed());
        }
    }
}
