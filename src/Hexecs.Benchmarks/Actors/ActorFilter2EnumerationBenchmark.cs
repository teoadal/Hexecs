using Hexecs.Benchmarks.Mocks;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//    .NET SDK 10.0.100
//    [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3

// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method     | Mean     | Ratio | Allocated | Alloc Ratio |
//    |----------- |---------:|------:|----------:|------------:|
//    | DefaultEcs | 165.8 us |  0.70 |         - |          NA |
//    | Hexecs     | 236.0 us |  1.00 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class ActorFilter2EnumerationBenchmark
{
    [Params(10_000, 100_000)] 
    public int Count;

    private ActorFilter<Attack, Defence> _filter = null!;
    private World _world = null!;

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcs.EntitySet _defaultEntitySet = null!;

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
        _world = new WorldBuilder().Build();

        _defaultEntitySet = _defaultWorld.GetEntities().With<Attack>().With<Defence>().AsSet();
        _filter = _world.Actors.Filter<Attack, Defence>();
        
        var context = _world.Actors;
        for (var i = 0; i < Count; i++)
        {
            var actor = context.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
        }
    }
}