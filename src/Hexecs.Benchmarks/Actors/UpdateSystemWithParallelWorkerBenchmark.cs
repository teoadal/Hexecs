using DefaultEcs.System;
using Friflo.Engine.ECS;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Threading;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.100
//     [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method              | Count   | Mean        | Ratio | Allocated | Alloc Ratio |
//     |-------------------- |-------- |------------:|------:|----------:|------------:|
//     | FriFlo_Parallel     | 100000  |    94.42 us |  0.77 |         - |          NA |
//     | Hexecs_Parallel     | 100000  |   122.17 us |  1.00 |         - |          NA |
//     | DefaultEcs_Parallel | 100000  |   221.83 us |  1.82 |         - |          NA |
//     |                     |         |             |       |           |             |
//     | FriFlo_Parallel     | 1000000 |   842.69 us |  0.91 |         - |          NA |
//     | Hexecs_Parallel     | 1000000 |   931.08 us |  1.00 |         - |          NA |
//     | DefaultEcs_Parallel | 1000000 | 2,370.75 us |  2.55 |         - |          NA |
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
//     | Method              | Count   | Mean      | Ratio | Allocated | Alloc Ratio |
//     |-------------------- |-------- |----------:|------:|----------:|------------:|
//     | FriFlo_Parallel     | 100000  |  30.03 us |  0.60 |         - |          NA |
//     | Hexecs_Parallel     | 100000  |  50.01 us |  1.00 |         - |          NA |
//     | DefaultEcs_Parallel | 100000  |  78.34 us |  1.57 |         - |          NA |
//     |                     |         |           |       |           |             |
//     | FriFlo_Parallel     | 1000000 | 294.39 us |  0.76 |         - |          NA |
//     | Hexecs_Parallel     | 1000000 | 387.52 us |  1.00 |         - |          NA |
//     | DefaultEcs_Parallel | 1000000 | 800.26 us |  2.07 |         - |          NA |


[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class UpdateSystemWithParallelWorkerBenchmark
{
    [Params(100_000, 1_000_000)] public int Count;

    private DefaultEcs.World _defaultWorld = null!;
    private DefaultEcsParallelSystem _defaultSystem = null!;

    private World _hexecsWorld = null!;
    private HexecsUpdateParallelSystem _hexecsSystem = null!;

    private EntityStore _frifloWorld = null!;
    private ArchetypeQuery<Attack, Defence, Speed> _frifloQuery = null!;
    private QueryJob<Attack, Defence, Speed> _frifloJob = null!;

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

    [Benchmark]
    public int FriFlo_Parallel()
    {
        _frifloJob.RunParallel();
        return _frifloQuery.Count;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _defaultSystem.Dispose();
        _defaultWorld.Dispose();
        _hexecsWorld.Dispose();

        _frifloJob.JobRunner.Dispose();
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

        _frifloWorld = new EntityStore { JobRunner = new ParallelJobRunner(4) };
        _frifloQuery = _frifloWorld.Query<Attack, Defence, Speed>();
        _frifloJob = _frifloQuery.ForEach((a, d, s, ch) =>
        {
            for (var i = 0; i < ch.Length; i++)
            {
                a[i].Value++;
                d[i].Value++;
                s[i].Value++;
            }
        });

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

            _frifloWorld.CreateEntity(new Attack(), new Defence(), new Speed());
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