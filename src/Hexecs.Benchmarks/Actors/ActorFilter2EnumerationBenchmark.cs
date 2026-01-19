using Friflo.Engine.ECS;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;

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
//     | FriFlo_Chunks          | 10000  |  11.12 us |  0.46 |         - |          NA |
//     | DefaultEcs             | 10000  |  15.74 us |  0.65 |         - |          NA |
//     | Hexecs_ComponentAccess | 10000  |  17.25 us |  0.71 |         - |          NA |
//     | FriFlo                 | 10000  |  23.40 us |  0.96 |      88 B |          NA |
//     | Hexecs                 | 10000  |  24.34 us |  1.00 |         - |          NA |
//     |                        |        |           |       |           |             |
//     | FriFlo_Chunks          | 100000 | 109.50 us |  0.47 |         - |          NA |
//     | DefaultEcs             | 100000 | 159.41 us |  0.69 |         - |          NA |
//     | Hexecs_ComponentAccess | 100000 | 174.74 us |  0.76 |         - |          NA |
//     | Hexecs                 | 100000 | 230.64 us |  1.00 |         - |          NA |
//     | FriFlo                 | 100000 | 232.40 us |  1.01 |      88 B |          NA |
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
//     | Method     | Count  | Mean       | Ratio | Allocated | Alloc Ratio |
//     |----------- |------- |-----------:|------:|----------:|------------:|
//     | DefaultEcs | 10000  |   9.473 us |  0.91 |         - |          NA |
//     | Hexecs     | 10000  |  10.361 us |  1.00 |         - |          NA |
//     |            |        |            |       |           |             |
//     | DefaultEcs | 100000 |  88.719 us |  0.88 |         - |          NA |
//     | Hexecs     | 100000 | 101.257 us |  1.00 |         - |          NA |


[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
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
        foreach (var actor in _filter)
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

        var attacks = _context.GetComponents<Attack>();
        var defences = _context.GetComponents<Defence>();

        foreach (var actorId in _filter.Keys)
        {
            result += attacks[actorId].Value +
                      defences[actorId].Value;
        }

        return result;
    }

    [Benchmark]
    public int DefaultEcs()
    {
        var attacks = _defaultWorld.GetComponents<Attack>();
        var defences = _defaultWorld.GetComponents<Defence>();

        var result = 0;
        foreach (var entity in _defaultEntitySet.GetEntities())
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

        foreach (var queryChunk in _frifloQuery.Chunks)
        {
            var attacks = queryChunk.Chunk1;
            var defences = queryChunk.Chunk2;

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

        var context = _world.Actors;
        for (var i = 0; i < Count; i++)
        {
            var attack = new Attack { Value = i };
            
            var actor = context.CreateActor();
            actor.Add(in attack);
            actor.Add(new Defence());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set(in attack);
            defaultEntity.Set<Defence>();
            
            _frifloWorld.CreateEntity(attack, new Defence(), new Speed());
        }
    }
}