using Hexecs.Worlds;
using System.Buffers;

namespace Hexecs.Benchmarks.Actors;

[SimpleJob(RuntimeMoniker.Net10_0)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[BenchmarkCategory("Actors")]
public class ActorHierarchyBenchmark
{
    [Params(100, 1_000, 2_000)] public int Count;

    private ActorContext _actorContext = null!;
    private Actor[] _parents = null!;
    private Actor[] _children = null!;
    private World _world = null!;

    [Benchmark]
    public int Do()
    {
        // Часть 1: Построение иерархии
        // Для каждого родителя добавляем Count детей (аналогично бенчмарку отношений)
        var childIdx = 0;
        for (var i = 0; i < _parents.Length; i++)
        {
            var parent = _parents[i];
            for (var j = 0; j < Count; j++)
            {
                parent.AddChild(_children[childIdx++]);
            }
        }

        var result = 0;
        var buffer = ArrayPool<uint>.Shared.Rent(Count);

        // Часть 2: Итерация и удаление
        for (var i = 0; i < _parents.Length; i++)
        {
            var parent = _parents[i];
            var children = parent.Children();
            var k = 0;

            // Сбор детей в буфер
            foreach (var child in children)
            {
                buffer[k++] = child.Id;
                result++;
            }

            // Удаление детей
            for (var j = 0; j < k; j++)
            {
                parent.RemoveChild(new Actor(_actorContext, buffer[j]));
            }
        }

        ArrayPool<uint>.Shared.Return(buffer);
        return result;
    }

    [GlobalSetup]
    public void Setup()
    {
        _world = new WorldBuilder().Build();
        _actorContext = _world.Actors;

        _parents = new Actor[Count];
        _children = new Actor[Count * Count];

        for (var i = 0; i < Count; i++)
        {
            _parents[i] = _actorContext.CreateActor();
            for (var j = 0; j < Count; j++)
            {
                _children[i * Count + j] = _actorContext.CreateActor();
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup() => _world.Dispose();
}