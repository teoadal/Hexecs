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
//     | Method                 | Count  | Mean      | Ratio | Allocated | Alloc Ratio |
//     |----------------------- |------- |----------:|------:|----------:|------------:|
//     | FriFlo_Chunks          | 10000  |  16.16 us |  0.55 |         - |          NA |
//     | Hexecs_ComponentAccess | 10000  |  25.85 us |  0.89 |         - |          NA |
//     | FriFlo                 | 10000  |  26.17 us |  0.90 |      88 B |          NA |
//     | Hexecs                 | 10000  |  29.12 us |  1.00 |         - |          NA |
//     | DefaultEcs             | 10000  |  29.48 us |  1.01 |         - |          NA |
//     |                        |        |           |       |           |             |
//     | FriFlo_Chunks          | 100000 | 159.17 us |  0.49 |         - |          NA |
//     | FriFlo                 | 100000 | 259.75 us |  0.80 |      88 B |          NA |
//     | Hexecs_ComponentAccess | 100000 | 295.89 us |  0.92 |         - |          NA |
//     | DefaultEcs             | 100000 | 308.48 us |  0.95 |         - |          NA |
//     | Hexecs                 | 100000 | 323.35 us |  1.00 |         - |          NA |
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
//     | Method                 | Count  | Mean       | Ratio | Allocated | Alloc Ratio |
//     |----------------------- |------- |-----------:|------:|----------:|------------:|
//     | FriFlo                 | 10000  |   9.383 us |  0.69 |      88 B |          NA |
//     | FriFlo_Chunks          | 10000  |   9.538 us |  0.70 |         - |          NA |
//     | Hexecs_ComponentAccess | 10000  |  12.621 us |  0.93 |         - |          NA |
//     | DefaultEcs             | 10000  |  13.351 us |  0.99 |         - |          NA |
//     | Hexecs                 | 10000  |  13.534 us |  1.00 |         - |          NA |
//     |                        |        |            |       |           |             |
//     | FriFlo                 | 100000 |  91.925 us |  0.65 |      88 B |          NA |
//     | FriFlo_Chunks          | 100000 |  94.962 us |  0.67 |         - |          NA |
//     | Hexecs_ComponentAccess | 100000 | 131.812 us |  0.93 |         - |          NA |
//     | Hexecs                 | 100000 | 142.264 us |  1.00 |         - |          NA |
//     | DefaultEcs             | 100000 | 158.585 us |  1.11 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorFilter3EnumerationBenchmark
{
    [Params(10_000, 100_000)] public int Count;

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
        foreach (var actor in _filter)
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

        var attacks = _context.GetComponents<Attack>();
        var defences = _context.GetComponents<Defence>();
        var speeds = _context.GetComponents<Speed>();

        foreach (var actorId in _filter.Keys)
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
        var attacks = _defaultWorld.GetComponents<Attack>();
        var defences = _defaultWorld.GetComponents<Defence>();
        var speeds = _defaultWorld.GetComponents<Speed>();

        var result = 0;
        foreach (var entity in _defaultEntitySet.GetEntities())
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

        foreach (var queryChunk in _frifloQuery.Chunks)
        {
            var attacks = queryChunk.Chunk1;
            var defences = queryChunk.Chunk2;
            var speeds = queryChunk.Chunk3;

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

        var context = _world.Actors;
        for (var i = 0; i < Count; i++)
        {
            var attack = new Attack { Value = i };

            var actor = context.CreateActor();
            actor.Add(in attack);
            actor.Add(new Defence());
            actor.Add(new Speed());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set(in attack);
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _frifloWorld.CreateEntity(attack, new Defence(), new Speed());
        }
    }
}