using DefaultEcs.System;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Mocks;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores
//    .NET SDK 10.0.101
//    [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//    .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method              | Mean       | Ratio | Allocated | Alloc Ratio |
//    |-------------------- |-----------:|------:|----------:|------------:|
//    | Hexecs_Parallel     |   769.8 us |  1.00 |         - |          NA |
//    | DefaultEcs_Parallel | 1,576.7 us |  2.05 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class UpdateSystemWithParallelWorkerBenchmark
{
    [Params(100_000, 1_000_000)] 
    public int Count;

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcsParallelSystem _defaultSystem = null!;

    private World _hexecsWorld = null!;
    private HexecsUpdateParallelSystem _hexecsSystem = null!;

    private WorldTime _state;

    [Benchmark(Baseline = true)]
    public int Hexecs_Parallel()
    {
        _hexecsSystem.Update(_state);
        return _hexecsSystem.Filter.Length;
    }

    [Benchmark]
    public int DefaultEcs_Parallel()
    {
        _defaultSystem.Update(_state);
        return _defaultSystem.Set.Count;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultSystem.Dispose();
        _defaultWorld.Dispose();
        _hexecsWorld.Dispose();
    }

    [GlobalSetup]
    public void Setup()
    {
        _defaultWorld = new DefaultEcs.World();
        _defaultSystem = new DefaultEcsParallelSystem(
            _defaultWorld,
            new DefaultEcs.Threading.DefaultParallelRunner(4));

        _hexecsWorld = new WorldBuilder()
            .UseDefaultParallelWorker(4)
            .UseDefaultActorContext(ctx => ctx.CreateUpdateSystem<HexecsUpdateParallelSystem>())
            .Build();
        _hexecsSystem = _hexecsWorld.Actors.GetUpdateSystem<HexecsUpdateParallelSystem>();

        _state = new WorldTime();

        var context = _hexecsWorld.Actors;
        for (var i = 0; i < Count; i++)
        {
            var actor = context.CreateActor();
            actor.Add(new Attack());
            actor.Add(new Defence());
            actor.Add(new Speed());

            var defaultEntity = _defaultWorld.CreateEntity();
            defaultEntity.Set<Attack>();
            defaultEntity.Set<Defence>();
            defaultEntity.Set<Speed>();
        }
    }

    private sealed class HexecsUpdateParallelSystem : UpdateSystem<Attack, Defence, Speed>
    {
        public HexecsUpdateParallelSystem(
            ActorContext context,
            IParallelWorker parallelWorker) : base(context, parallelWorker: parallelWorker)
        {
        }

        protected override void Update(in ActorRef<Attack, Defence, Speed> actor, in WorldTime time)
        {
            ref var attack = ref actor.Component1;
            ref var defence = ref actor.Component2;
            ref var speed = ref actor.Component3;

            attack.Value++;
            defence.Value++;
            speed.Value++;
        }
    }

    private sealed class DefaultEcsParallelSystem : AEntitySetSystem<WorldTime>
    {
        public DefaultEcsParallelSystem(DefaultEcs.World world, DefaultEcs.Threading.IParallelRunner runner)
            : base(
                world.GetEntities().With<Attack>().With<Defence>().With<Speed>().AsSet(),
                runner)
        {
        }

        protected override void Update(WorldTime state, in DefaultEcs.Entity entity)
        {
            ref var attack = ref entity.Get<Attack>();
            ref var defence = ref entity.Get<Defence>();
            ref var speed = ref entity.Get<Speed>();

            attack.Value++;
            defence.Value++;
            speed.Value++;
        }
    }
}