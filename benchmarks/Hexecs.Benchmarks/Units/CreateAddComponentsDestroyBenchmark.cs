using BenchmarkDotNet.Diagnostics.dotTrace;

using Hexecs.Benchmarks.Mocks.ActorComponents;

using Hexecsm.Filters;

using ActorId = Hexecsm.ActorId;

namespace Hexecs.Benchmarks.Units;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.400
//     [Host]    : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
//
// Runtime=.NET 10.0
//
//     | Method                   | Toolchain              | Mean     | Allocated |
//     |------------------------- |----------------------- |---------:|----------:|
//     | Hexes_M_CreateAddDestroy | Default                | 289.1 ms |      40 B |
//     | Hexes_M_CreateAddDestroy | InProcessEmitToolchain | 290.1 ms |      40 B |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn]
[InProcess]
[MemoryDiagnoser]
[DotTraceDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[BenchmarkCategory("Units")]
public class CreateAddComponentsDestroyBenchmark
{
    private const int Count = 500_000;

    private List<ActorId> _mHexecsActors = null!;
    private Hexecsm.Worlds.World _mWorld = null!;
    private List<IFilter> _mFilters = null!;

    [Benchmark]
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

    [GlobalCleanup]
    public void Cleanup()
    {
        _mWorld.Dispose();
        _mWorld = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _mHexecsActors = [];
        _mWorld = new Hexecsm.Worlds.World(Count, 4);
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
            ActorId actorId = _mWorld.CreateActor();
            _mWorld.AddComponent(actorId, new Attack());
            _mWorld.AddComponent(actorId, new Defence());
            _mWorld.AddComponent(actorId, new Speed());
        }

        _mWorld.Update();

        foreach (ActorId actorId in _mHexecsActors)
        {
            _mWorld.DestroyActor(actorId);
        }

        _mWorld.Update();
        _mHexecsActors.Clear();
    }
}
