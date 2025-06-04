using Hexecs.Benchmarks.Mocks;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
public class CreateAddComponentsDestroyBenchmark
{
    [Params(1_000, 100_000, 500_000)] public int Count;

    private List<DefaultEcs.EntitySet> _defaultSets = null!;
    private List<DefaultEcs.Entity> _defaultEntities = null!;
    private DefaultEcs.World _defaultWorld = null!;

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

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultWorld.Dispose();
        _defaultWorld = null!;

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
            // warmup
        ];

        // warmup
        for (var i = 0; i < Count; i++)
        {
            var actor = _hexecsContext.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());
            actor.Add(new Speed());

            _hexecsActors.Add(actor);

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();

            _defaultEntities.Add(defaultEntity);
        }

        foreach (var actor in _hexecsActors)
        {
            actor.Destroy();
        }

        foreach (var entity in _defaultEntities)
        {
            entity.Dispose();
        }
    }
}