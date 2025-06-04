using System.Runtime.CompilerServices;
using Hexecs.Benchmarks.Mocks;
using Hexecs.Worlds;
using World = Hexecs.Worlds.World;

namespace Hexecs.Benchmarks.Actors;

// componentPool
//    | Method    | Mean     | Ratio | Allocated | Alloc Ratio |
//    |---------- |---------:|------:|----------:|------------:|
//    | Has       | 602.0 us |  1.00 |         - |          NA |
//    | Is        | 627.3 us |  1.04 |         - |          NA |
//    | Reference | 697.3 us |  1.16 |         - |          NA |
[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
public class CheckComponentExistsBenchmark
{
    private const int Count = 100_000;

    private ActorContext _context = null!;
    private DefaultEcs.World _defaultWorld = null!;
    private World _world = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs_Has()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            if (actor.Has<Speed>()) result++;
        }

        return result;
    }

    [Benchmark]
    public int DefaultEcs_Has()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var entity in _defaultWorld)
        {
            if (entity.Has<Speed>())
            {
                result++;
            }
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_Is()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            if (actor.Is<Speed>(out _))
            {
                result++;
            }
        }

        return result;
    }

    [Benchmark]
    public int Hexecs_Reference()
    {
        var result = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var actor in _context)
        {
            ref var reference = ref actor.TryGetRef<Speed>();
            if (!Unsafe.IsNullRef(ref reference))
            {
                result++;
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
        _world = new WorldBuilder().Build();
        _context = _world.Actors;

        for (var i = 0; i < Count; i++)
        {
            var actor = _context.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();

            if (i % 10 != 0) continue;

            actor.Add(new Speed());
            defaultEntity.Set<Speed>();
        }
    }
}