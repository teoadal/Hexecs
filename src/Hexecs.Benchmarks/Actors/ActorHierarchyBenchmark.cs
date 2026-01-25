using Hexecs.Worlds;
using Friflo.Engine.ECS;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.102
//     [Host]    : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method           | Count | Mean         | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------ |-------------:|------:|----------:|------------:|
//     | Hexecs_Hierarchy | 100   |     333.4 us |  1.00 |         - |          NA |
//     | Friflo_Hierarchy | 100   |     709.2 us |  2.13 |         - |          NA |
//     |                  |       |              |       |           |             |
//     | Hexecs_Hierarchy | 1000  |  34,104.9 us |  1.00 |         - |          NA |
//     | Friflo_Hierarchy | 1000  | 129,978.8 us |  3.81 |         - |          NA |
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
//     | Method           | Count | Mean        | Ratio | Allocated | Alloc Ratio |
//     |----------------- |------ |------------:|------:|----------:|------------:|
//     | Hexecs_Hierarchy | 100   |    156.3 us |  1.00 |         - |          NA |
//     | Friflo_Hierarchy | 100   |    315.1 us |  2.02 |         - |          NA |
//     |                  |       |             |       |           |             |
//     | Hexecs_Hierarchy | 1000  | 13,998.8 us |  1.00 |         - |          NA |
//     | Friflo_Hierarchy | 1000  | 89,419.8 us |  6.39 |         - |          NA |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorHierarchyBenchmark
{
    [Params(100, 1_000)] public int Count;

    private ActorContext _actorContext = null!;
    private Actor[] _buffer = null!;
    private Actor[] _parents = null!;
    private Actor[] _children = null!;
    private World _world = null!;

    // Friflo поля
    private Entity[] _friBuffer = null!;
    private Entity[] _friParents = null!;
    private Entity[] _friChildren = null!;
    private EntityStore _friStore = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs_Hierarchy()
    {
        var childIdx = 0;
        foreach (var parent in _parents)
        {
            for (var j = 0; j < Count; j++)
            {
                parent.AddChild(_children[childIdx++]);
            }
        }

        var result = 0;
        var buffer = _buffer;

        foreach (var parent in _parents)
        {
            var children = parent.Children();
            var k = 0;

            foreach (var child in children)
            {
                buffer[k++] = child;
                result++;
            }

            for (var j = 0; j < k; j++)
            {
                parent.RemoveChild(buffer[j]);
            }
        }

        return result;
    }

    [Benchmark]
    public int Friflo_Hierarchy()
    {
        var childIdx = 0;
        foreach (var parent in _friParents)
        {
            for (var j = 0; j < Count; j++)
            {
                parent.AddChild(_friChildren[childIdx++]);
            }
        }

        var result = 0;
        var buffer = _friBuffer;

        for (var i = 0; i < _friParents.Length; i++)
        {
            var parent = _friParents[i];
            var children = parent.ChildEntities;
            var k = 0;

            foreach (var child in children)
            {
                buffer[k++] = child;
                result++;
            }

            for (var j = 0; j < k; j++)
            {
                parent.RemoveChild(buffer[j]);
            }
        }

        return result;
    }

    [GlobalSetup]
    public void Setup()
    {
        // Setup Hexecs
        _world = new WorldBuilder().Build();
        _buffer = new Actor[Count];
        _actorContext = _world.Actors;
        _parents = new Actor[Count];
        _children = new Actor[Count * Count];

        // Setup Friflo
        _friBuffer = new Entity[Count];
        _friParents = new Entity[Count];
        _friChildren = new Entity[Count * Count];
        _friStore = new EntityStore();

        for (var i = 0; i < Count; i++)
        {
            _parents[i] = _actorContext.CreateActor();
            _friParents[i] = _friStore.CreateEntity();

            for (var j = 0; j < Count; j++)
            {
                var index = (i * Count) + j;
                _children[index] = _actorContext.CreateActor();
                _friChildren[index] = _friStore.CreateEntity();
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();
}